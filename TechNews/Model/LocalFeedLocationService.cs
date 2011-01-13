using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TechNews.Model
{
    public class LocalFeedLocationService : IFeedLocationService
    {
        public IList<ParentFeed> GetFeeds()
        {
            var returnList = new List<ParentFeed>
                                 {
                                     new ParentFeed{FeedUri = new Uri("http://feedproxy.google.com/TechCrunch"), Link = "http://www.techcrunch.com/", Title = "TechCrunch"},
                                     new ParentFeed{FeedUri = new Uri("http://news.ycombinator.com/rss"), Link = "http://news.ycombinator.com/", Title = "Hacker News"},
                                     new ParentFeed{FeedUri =  new Uri("http://feeds.feedburner.com/ommalik"), Link = "http://gigaom.com/", Title = "GigaOm"},
                                     new ParentFeed{FeedUri = new Uri("http://feeds.venturebeat.com/Venturebeat"), Link = "http://www.venturebeat.com/", Title = "VentureBeat"},
                                     new ParentFeed{FeedUri =  new Uri("http://feeds.feedburner.com/readwriteweb"), Link="http://www.readwriteweb.com/", Title = "ReadWriteWeb"},
                                     new ParentFeed{FeedUri = new Uri("http://www.engadget.com/rss.xml"), Link = "http://www.engadget.com/", Title = "Engadget"},
                                     new ParentFeed{FeedUri = new Uri("http://techzulu.com/feed"), Link = "http://techzulu.com/", Title = "TechZulu"}
                                 };

            return returnList;
        }
    }
}