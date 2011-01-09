using System;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;

namespace TechNews
{
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Messenger.Default.Register<string>(this, "NavigationRequest", uri => NavigationService.Navigate(new Uri(string.Format("/BrowserPage.xaml?uri={0}", uri),UriKind.RelativeOrAbsolute)));
        }
    }
}
