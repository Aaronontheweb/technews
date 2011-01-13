using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using IsolatedStorageExtensions;
using QDFeedParser;
using TechNews.Design;
using TechNews.Helpers;
using TechNews.Model;
using TechNews.PerformanceProfiling;

namespace TechNews.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFeedLocationService _feedLocator;
        private readonly IFeedQueryService _remoteQueryService;
        private object _cacheLock = new object();
        public IDictionary<Uri, KeyValuePair<DateTime, IList<FeedItemSummary>>> CacheDictionary { get; set; }

        //Used for quick and easy computation

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
                RaisePropertyChanged("IsProgressBarVisible");
            }
        }
        public Visibility IsProgressBarVisible
        {
            get
            {
                return IsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ParentFeed _currentFeedUri;
        public ParentFeed CurrentFeedUri
        {
            get { return _currentFeedUri; }
            set
            {
                _currentFeedUri = value;
            }
        }

        private SmartObservableCollection<FeedItemSummary> _feedItems;
        private ObservableCollection<string> _feedTitles;
        private IList<ParentFeed> _feeds;

        public ObservableCollection<string> FeedTitles { get { return _feedTitles; } set { _feedTitles = value; RaisePropertyChanged("FeedTitles"); } }

        public IList<ParentFeed> Feeds { get { return _feeds; } set { _feeds = value; RaisePropertyChanged("Feeds"); } }

        public SmartObservableCollection<FeedItemSummary> FeedItems
        {
            get { return _feedItems; }
            set
            {
                _feedItems = value;
                RaisePropertyChanged("FeedItems");
            }
        }

        #region Commands

        public RelayCommand<string> NavigateToUri { get; private set; }

        public RelayCommand<SelectionChangedEventArgs> LocateFeedUri { get; private set; }

        public RelayCommand LoadLastOrDefaultFeed { get; private set; }

        public RelayCommand BeginLoading { get; private set; }

        public RelayCommand EndLoading { get; private set; }

        public RelayCommand PopulateItemsList { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _feedLocator = new LocalFeedLocationService();
            Feeds = _feedLocator.GetFeeds();
            CurrentFeedUri = Feeds[0];
            FeedTitles = new ObservableCollection<string>();
            CacheDictionary = new Dictionary<Uri, KeyValuePair<DateTime, IList<FeedItemSummary>>>();
            PopulateFeedTitles();

            FeedItems = new SmartObservableCollection<FeedItemSummary>();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                var designFeed = FeedSummarizer.SummarizeFeed(DesignTimeData.Feeds.First());

                PopulateFeedItems(designFeed);
            }
            else
            {
                // Code runs "for real"

                _remoteQueryService = new FeedQueryService(new HttpFeedFactory());

                LocateFeedUri = new RelayCommand<SelectionChangedEventArgs>(ResolveFeedUri);
                LoadLastOrDefaultFeed = new RelayCommand(() => ExecuteQuery(CurrentFeedUri, true));
                NavigateToUri = new RelayCommand<string>(uri => Messenger.Default.Send<string>(uri, "NavigationRequest"));

                BeginLoading = new RelayCommand(() => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                                                                                {
                                                                                                    IsLoading = true;
                                                                                                }));
                PopulateItemsList = new RelayCommand(() =>
                                                         {
                                                             if (FeedIsInCache(CurrentFeedUri.FeedUri))
                                                             {
                                                                 UpdateBindings(CacheDictionary[CurrentFeedUri.FeedUri].Value);
                                                             }
                                                             else
                                                             {
                                                                 ExecuteQuery(CurrentFeedUri, bind: true);
                                                             }
                                                         });
                EndLoading = new RelayCommand(() => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                                                                              {
                                                                                                  if(FeedIsInCache(CurrentFeedUri.FeedUri))
                                                                                                    IsLoading = false;
                                                                                              }));
            }
        }

        private void ResolveFeedUri(SelectionChangedEventArgs args)
        {
            if (args == null) return;
            var selected = args.AddedItems[0];
            var feed = Feeds.Single(x => x.Title.Equals(selected));
            CurrentFeedUri = feed;
        }

        private void ExecuteQuery(ParentFeed feed, bool bind = false)
        {

            _remoteQueryService.BeginQueryFeeds(feed.FeedUri, async =>
            {

                var feedResult = _remoteQueryService.EndQueryFeeds(async);

                var feedItemSummaries = FeedSummarizer.SummarizeFeed(feedResult, feed, feedResult.Items.Count);

                //Cache the processed feed to memory


                lock (_cacheLock)
                {
                    if (CacheDictionary.ContainsKey(feedResult.FeedUri))
                        CacheDictionary.Remove(feedResult.FeedUri);
                    CacheDictionary.Add(feedResult.FeedUri,
                                        new KeyValuePair<DateTime, IList<FeedItemSummary>>(DateTime.UtcNow,
                                                                                           feedItemSummaries));
                }

                if (bind)
                {
                    UpdateBindings(feedItemSummaries);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => { IsLoading = false; });
                }

            });

        }

        private bool FeedIsInCache(Uri feed)
        {
            lock(_cacheLock)
            {
                return CacheDictionary.ContainsKey(feed) && (DateTime.UtcNow - CacheDictionary[feed].Key < _remoteQueryService.CacheExpirationWindow);
            }
        }

        private void UpdateBindings(IEnumerable<FeedItemSummary> feedItemSummaries)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => PopulateFeedItems(feedItemSummaries));
        }

        private void PopulateFeedItems(IEnumerable<FeedItemSummary> summaries)
        {
            GC.Collect();
            DebugWatch.Start();
            FeedItems.ReplaceAll(summaries);
            DebugWatch.Stop();
            DebugWatch.Print("PopulateFeedItems Method");
            DebugWatch.Reset();
        }

        private void PopulateFeedTitles()
        {
            GC.Collect();
            DebugWatch.Start();
            foreach (var item in Feeds)
            {
                FeedTitles.Add(item.Title);
            }
            DebugWatch.Stop();
            DebugWatch.Print("PopulateFeedTitles Method");
            DebugWatch.Reset();
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}