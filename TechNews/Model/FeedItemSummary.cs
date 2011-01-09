using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using QDFeedParser;

namespace TechNews.Model
{
    public class ParentFeed
    {
        public string Title { get; set; }
        public Uri FeedUri { get; set; }
        public string Link { get; set; }
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
