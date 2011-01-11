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
    public class FeedQueryService : IFeedQueryService
    {
        public TimeSpan CacheExpirationWindow { get; private set; }
        private static readonly TimeSpan DefaultCacheExpriationWindow = new TimeSpan(0,0,20,0);

        private IFeedFactory Factory { get; set; }

        public FeedQueryService(IFeedFactory factory, TimeSpan cacheDuration)
        {
            CacheExpirationWindow = cacheDuration;
            Factory = factory;
        }
        
        public FeedQueryService(IFeedFactory factory) : this(factory, DefaultCacheExpriationWindow)
        {
        }

        #region Implementation of IFeedQueryService

        public IAsyncResult BeginQueryFeeds(Uri uri, AsyncCallback callback)
        {
            return Factory.BeginCreateFeed(uri, callback);
        }

        public IFeed EndQueryFeeds(IAsyncResult result)
        {
            var feedData = Factory.EndCreateFeed(result);
            return feedData;
        }

        public IFeed CreateFeed(string xml, Uri feeduri)
        {
            return Factory.CreateFeed(feeduri, Factory.CheckFeedType(xml), xml);
        }

        #endregion
    }
}
