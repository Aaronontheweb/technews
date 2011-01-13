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
        //Service used for populating the list of feeds on the pivot
        private readonly IFeedLocationService _feedLocator;

        //Service used for querying remote feeds
        private readonly IFeedQueryService _remoteQueryService;

        //Locking mechanism used for synchronizing access to the cache
        private readonly object _cacheLock = new object();

        //In-memory cache dictionary
        public IDictionary<Uri, KeyValuePair<DateTime, IList<FeedItemSummary>>> CacheDictionary { get; set; }

        //Used for tolling the IsInDeterminate field of the progress bar
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

        //Used for toggling the visibility property of the progress bar
        public Visibility IsProgressBarVisible
        {
            get
            {
                return IsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        //Uri of the feed currently being viewed by the end-user
        public ParentFeed CurrentFeedUri { get; set; }

        //Internal collection of FeedItemSummaries to which the UI binds
        private SmartObservableCollection<FeedItemSummary> _feedItems;

        //Internal collection of FeedTitles to which the UI binds
        private ObservableCollection<string> _feedTitles;

        //Internal dictionary of feed URIs, used for resolving the URI of the current feed based on the title
        // - the SelectionChanged event raised by the pivot control only passes the name back, so this is why we have to do a name-lookup
        private IList<ParentFeed> _feeds;

        /// <summary>
        /// The names of all of the titles in the feed, to which the UI binds
        /// </summary>
        public ObservableCollection<string> FeedTitles { get { return _feedTitles; } set { _feedTitles = value; RaisePropertyChanged("FeedTitles"); } }

        /// <summary>
        /// All of the feeds
        /// </summary>
        public IList<ParentFeed> Feeds { get { return _feeds; } set { _feeds = value; RaisePropertyChanged("Feeds"); } }

        /// <summary>
        /// The feed items which populate the current list in the UI
        /// </summary>
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

        /// <summary>
        /// Command used for browsing to a specific item in a list of feed items
        /// </summary>
        public RelayCommand<string> NavigateToUri { get; private set; }

        /// <summary>
        /// Resolves the URI of the current feed selected in the pivot
        /// </summary>
        public RelayCommand<SelectionChangedEventArgs> LocateFeedUri { get; private set; }

        /// <summary>
        /// Loads the last or the default feed
        /// </summary>
        public RelayCommand LoadLastOrDefaultFeed { get; private set; }

        /// <summary>
        /// Turns on the loading bar
        /// </summary>
        public RelayCommand BeginLoading { get; private set; }

        /// <summary>
        /// Turns off the loading bar
        /// </summary>
        public RelayCommand EndLoading { get; private set; }

        /// <summary>
        /// Binds the elements of the active list
        /// </summary>
        public RelayCommand PopulateItemsList { get; private set; }

        public RelayCommand BuildAllItems { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _feedLocator = new LocalFeedLocationService();
            CacheDictionary = new Dictionary<Uri, KeyValuePair<DateTime, IList<FeedItemSummary>>>();
            FeedTitles = new ObservableCollection<string>();
            FeedItems = new SmartObservableCollection<FeedItemSummary>();

            Feeds = _feedLocator.GetFeeds(); //Initialize the feeds collection
            CurrentFeedUri = Feeds[0]; //Initialize the CurrentFeedUri
            PopulateFeedTitles(); //Initialize the feed titles collection

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

                BuildAllItems = new RelayCommand(() =>
                                                     {
                                                         foreach(var feed in Feeds)
                                                         {
                                                             ExecuteQuery(feed);
                                                         }
                                                     });
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

                //Synchronize access to the cache
                lock (_cacheLock)
                {
                    //Cache the processed feed to memory
                    if (CacheDictionary.ContainsKey(feedResult.FeedUri))
                        CacheDictionary.Remove(feedResult.FeedUri);
                    CacheDictionary.Add(feedResult.FeedUri,
                                        new KeyValuePair<DateTime, IList<FeedItemSummary>>(DateTime.UtcNow,
                                                                                           feedItemSummaries));
                }

                //Perform a UI bind update if one is requested
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
            foreach (var item in Feeds)
            {
                FeedTitles.Add(item.Title);
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}