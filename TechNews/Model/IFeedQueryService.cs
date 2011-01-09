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
    public interface IFeedQueryService
    {
        IAsyncResult BeginQueryFeeds(Uri uri, AsyncCallback callback);
        IFeed EndQueryFeeds(IAsyncResult result);
    }
}
