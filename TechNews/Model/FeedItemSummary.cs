using System;
using System.Collections.Generic;

namespace TechNews.Model
{
    public class ParentFeed
    {
        public string Title { get; set; }
        public Uri FeedUri { get; set; }
        public string Link { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class FeedItemSummary
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime DatePublished { get; set; }
        public ParentFeed ParentFeed { get; set; }
    }

    public class FeedItemSummaryComparer : IEqualityComparer<FeedItemSummary>
    {
        #region Implementation of IEqualityComparer<FeedItemSummary>

        public bool Equals(FeedItemSummary x, FeedItemSummary y)
        {
            return x.Title.Equals(y.Title) && x.DatePublished == y.DatePublished;
        }

        public int GetHashCode(FeedItemSummary obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
