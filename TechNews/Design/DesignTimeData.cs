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

namespace TechNews.Design
{
    public static class DesignTimeData
    {
        public static IList<IFeed> Feeds { get; private set; }

        static DesignTimeData()
        {
            Feeds = new List<IFeed>();

            var techcrunch = new Rss20Feed(string.Empty, new Uri("http://feedproxy.google.com/TechCrunch"))
                                 {
                                     Description =
                                         "TechCrunch is a group-edited blog that profiles the companies, products and events defining and transforming the new web.",
                                     Generator = "http://wordpress.com/",
                                     LastUpdated = DateTime.Parse("Sat, 08 Jan 2011 17:49:35 +0000"),
                                     Link = "http://www.techcrunch.com/",
                                     Title = "TechCrunch"
                                 };

            techcrunch.Items.Add(new Rss20FeedItem
                                     {
                                         Id = "http://techcrunch.com/?p=260279",
                                         Author = "Jon Evans",
                                         Title = "Can Google Get Its Mojo Back?",
                                         DatePublished = DateTime.Parse("Sat, 08 Jan 2011 17:49:35 +0000"),
                                         Link = "http://feedproxy.google.com/~r/Techcrunch/~3/IdYe_HJ2kZQ/",
                                         Comments = "http://techcrunch.com/2011/01/08/google-mojo/#comments",
                                         Content = "Here is some sample content for the sake of having some test data."
                                     });

            techcrunch.Items.Add(new Rss20FeedItem
                                     {
                                         Id = "http://techcrunch.com/?p=261810",
                                         Author = "Robin Wauters",
                                         Title = "Google Bangladesh Site “OwN3D by TiGER-M@TE”",
                                         DatePublished = DateTime.Parse("Sat, 08 Jan 2011 16:27:24 +0000"),
                                         Link = "http://feedproxy.google.com/~r/Techcrunch/~3/sz7peVBocew/",
                                         Comments = "http://techcrunch.com/2011/01/08/google-bangladesh-hacked/#comments",
                                         Content = "Here is some more, different sample content for the sake of having some test data."
                                     });


            var aaronontheweb = new Rss20Feed(string.Empty, new Uri("http://www.aaronstannard.com/syndication.axd"))
                                    {
                                        Description = "Hacking .NET and Startups",
                                        Generator = "BlogEngine.NET 2.0.0.0",
                                        LastUpdated = DateTime.Parse("Thu, 06 Jan 2011 12:01:00 -1300"),
                                        Link = "http://www.aaronstannard.com/",
                                        Title = "Aaronontheweb"
                                    };

            aaronontheweb.Items.Add(new Rss20FeedItem
                                        {
                                            Id = "http://www.aaronstannard.com/post.aspx?id=ffcc4ee5-34f6-448b-b4df-4cb7d108d055",
                                            Author = "Aaronontheweb",
                                            Title = "How to Use Asynchronous Controllers in ASP.NET MVC2 & MVC3",
                                            DatePublished = DateTime.Parse("Thu, 06 Jan 2011 12:01:00 -1300"),
                                            Link = "http://www.aaronstannard.com/post/2011/01/06/asynchonrous-controllers-ASPNET-mvc.aspx",
                                            Comments = "http://www.aaronstannard.com/post/2011/01/06/asynchonrous-controllers-ASPNET-mvc.aspx#comment",
                                            Content = "First piece of test data from Aaronontheweb."
                                        });

            aaronontheweb.Items.Add(new Rss20FeedItem
                                        {
                                            Id = "http://www.aaronstannard.com/post.aspx?id=280eda38-df4f-4ec1-871f-e9ef50eb5982",
                                            Author = "Aaronontheweb",
                                            Title = "How to Make Any Operation Asynchronous in .NET",
                                            DatePublished = DateTime.Parse("Mon, 03 Jan 2011 11:40:00 -1300"),
                                            Link = "http://www.aaronstannard.com/post/2011/01/03/How-to-Make-Any-Operation-Asynchronous-in-NET.aspx",
                                            Comments = "http://www.aaronstannard.com/post/2011/01/03/How-to-Make-Any-Operation-Asynchronous-in-NET.aspx#comment",
                                            Content = "Second piece of test data from Aaronontheweb."
                                        });

            Feeds.Add(techcrunch);
            Feeds.Add(aaronontheweb);

        }
    }
}
