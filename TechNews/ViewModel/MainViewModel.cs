using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
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
        private readonly IFeedQueryService _queryService;

        //Used for quick and easy computation
        private IList<FeedItemSummary> RawFeedItems { get; set; }
        public ObservableCollection<FeedItemSummary> FeedItems { get; private set; }

        #region Commands

        public RelayCommand QueryFeeds { get; private set; }

        #endregion 

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            FeedItems = new ObservableCollection<FeedItemSummary>();
            RawFeedItems = new List<FeedItemSummary>();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                PopulateFeedItems(FeedSummarizer.SummarizeFeeds(DesignTimeData.Feeds));
            }
            else
            {
                // Code runs "for real"
                _feedLocator = new LocalFeedLocationService();
                _queryService = new FeedQueryService(new HttpFeedFactory());
                QueryFeeds = new RelayCommand(() =>
                                                  {
                                                      foreach (var feed in _feedLocator.GetFeeds())
                                                      {
                                                          _queryService.BeginQueryFeeds(feed, async =>
                                                                                                  {
                                                                                                      var feedResult =
                                                                                                          _queryService.
                                                                                                              EndQueryFeeds
                                                                                                              (async);
                                                                                                      DispatcherHelper.CheckBeginInvokeOnUI(() => AppendFeed(feedResult));
                                                                                                  });
                                                      }
                                                  });
                
            }
        }

        public void AppendFeed(IFeed feeds)
        {
            var feedItems = FeedSummarizer.SummarizeFeeds(new[] {feeds});
            RawFeedItems = FeedSummarizer.MergeFeedItems(RawFeedItems, feedItems);
            PopulateFeedItems(RawFeedItems);
        }

        public void PopulateFeedItems(IList<FeedItemSummary> summaries)
        {
            FeedItems.Clear();
            foreach (var item in summaries)
            {
                FeedItems.Add(item);
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}