using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TechNews.Model
{
    public class LocalFeedLocationService : IFeedLocationService
    {
        public IList<Uri> GetFeeds()
        {
            var returnList = new List<Uri>
                                 {
                                     new Uri("http://feedproxy.google.com/TechCrunch"),
                                     new Uri("http://news.ycombinator.com/rss"),
                                     new Uri("http://feeds.feedburner.com/ommalik"),
                                     new Uri("http://feeds.venturebeat.com/Venturebeat"),
                                     new Uri("http://feeds.feedburner.com/readwriteweb")
                                 };

            return returnList;
        }
    }
}