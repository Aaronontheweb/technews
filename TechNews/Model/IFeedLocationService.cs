using System;
using System.Collections.Generic;

namespace TechNews.Model
{
    public interface IFeedLocationService
    {
        IList<Uri> GetFeeds();
    }
}