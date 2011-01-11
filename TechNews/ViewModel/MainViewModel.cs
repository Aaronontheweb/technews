using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using IsolatedStorageExtensions;
using QDFeedParser;
using TechNews.Design;
using TechNews.Helpers;
using TechNews.Model;

namespace TechNews.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IFeedLocationService _feedLocator;
        private readonly IFeedQueryService _remoteQueryService;
        private readonly IFeedQueryService _localQueryService;
        private IFeedQueryService _activeQueryService;
        private IDictionary<Uri, DateTime> CacheDictionary;

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

        private ObservableCollection<FeedItemSummary> _feedItems;
        private ObservableCollection<string> _feedTitles;
        private IList<ParentFeed> _feeds;

        public ObservableCollection<string> FeedTitles { get { return _feedTitles; } set { _feedTitles = value; RaisePropertyChanged("FeedTitles"); } }

        public IList<ParentFeed> Feeds { get { return _feeds; } set { _feeds = value; RaisePropertyChanged("Feeds"); } }

        public ObservableCollection<FeedItemSummary> FeedItems
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

        public RelayCommand<SelectionChangedEventArgs> QueryFeed { get; private set; }

        public RelayCommand LoadTechCrunch { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _feedLocator = new LocalFeedLocationService();
            Feeds = _feedLocator.GetFeeds();
            FeedTitles = new ObservableCollection<string>();
            CacheDictionary = new Dictionary<Uri, DateTime>();
            PopulateFeedTitles();

            FeedItems = new ObservableCollection<FeedItemSummary>();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                var designFeed = FeedSummarizer.SummarizeFeed(DesignTimeData.Feeds.First());

                foreach (var item in designFeed)
                {
                    FeedItems.Add(item);
                }
            }
            else
            {
                // Code runs "for real"

                _remoteQueryService = new FeedQueryService(new HttpFeedFactory());
                _localQueryService = new FeedQueryService(new IsolatedStorageFeedFactory());

                QueryFeed = new RelayCommand<SelectionChangedEventArgs>(FindFeedAndExecuteQuery);
                LoadTechCrunch = new RelayCommand(() => ExecuteQuery(Feeds[0]));
                NavigateToUri = new RelayCommand<string>(uri => Messenger.Default.Send<string>(uri, "NavigationRequest"));
            }
        }

        private void FindFeedAndExecuteQuery(SelectionChangedEventArgs args)
        {
            if (args == null) return;
            var selected = args.AddedItems[0];
            var feed = Feeds.Single(x => x.Title.Equals(selected));

            ExecuteQuery(feed);
        }

        private void ExecuteQuery(ParentFeed feed)
        {
            IsLoading = true;

            var filepath = IsolatedStorageHelper.GetSafeFileName(feed.FeedUri.LocalPath + ".xml");
            var queryUri = feed.FeedUri;
            if(IsolatedStorageHelper.FileExists(filepath) 
                && CacheDictionary.ContainsKey(feed.FeedUri)
                && DateTime.UtcNow - CacheDictionary[feed.FeedUri] < _remoteQueryService.CacheExpirationWindow)
            {
                _activeQueryService = _localQueryService;
                queryUri = new Uri(filepath, UriKind.Relative);
            }
            else
            {
                _activeQueryService = _remoteQueryService;
            }
             

            _activeQueryService.BeginQueryFeeds(queryUri, async =>
                {
                    
                    var feedResult = _activeQueryService.EndQueryFeeds(async);

                    var feedItemSummaries = FeedSummarizer.SummarizeFeed(feedResult, feed, feedResult.Items.Count);

                    DispatcherHelper.CheckBeginInvokeOnUI(() => PopulateFeedItems(feedItemSummaries));
                    DispatcherHelper.CheckBeginInvokeOnUI(() => { IsLoading = false; });

                    //Cache the file to Isolated Storage
                    if (_activeQueryService.GetHashCode() == _remoteQueryService.GetHashCode())
                    {
                        IsolatedStorageHelper.MakeFile(feedResult.XmlFeed, filepath);
                        if (CacheDictionary.ContainsKey(feedResult.FeedUri))
                            CacheDictionary[feedResult.FeedUri] = DateTime.UtcNow;
                        else
                        {
                            CacheDictionary.Add(feedResult.FeedUri, DateTime.UtcNow);
                        }
                    }
                        

                });
        }

        private void PopulateFeedItems(IEnumerable<FeedItemSummary> summaries)
        {
            var newItems = new ObservableCollection<FeedItemSummary>();
            foreach (var summary in summaries)
            {
                newItems.Add(summary);
            }

            FeedItems = newItems;
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