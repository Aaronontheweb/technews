using System;
using System.Collections.Generic;
using System.Linq;
using QDFeedParser;
using TechNews.Model;

namespace TechNews.Helpers
{
    public class FeedSummarizer
    {
        private static readonly FeedItemSummaryComparer Comparer = new FeedItemSummaryComparer();

        public static IList<FeedItemSummary> SummarizeFeed(IFeed feed, int itemCount = 3)
        {
            var parentFeed = new ParentFeed {FeedUri = feed.FeedUri, Link = feed.Link, Title = feed.Title};

            return feed.Items.Reverse().Skip(Math.Max(0, feed.Items.Count - itemCount)).Select(feedItem => new FeedItemSummary
                                                     {
                                                         DatePublished = feedItem.DatePublished, Link = feedItem.Link, ParentFeed = parentFeed, Title = feedItem.Title
                                                     }).OrderByDescending(x => x.DatePublished).ToList();
        }

        public static IList<FeedItemSummary> MergeFeedItems(IEnumerable<FeedItemSummary> list1, IEnumerable<FeedItemSummary> list2)
        {
            return list1.Concat(list2).OrderByDescending(x => x.DatePublished).Distinct(Comparer).ToList();
        }
    }
}
