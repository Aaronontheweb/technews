using System;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;

namespace TechNews
{
    /// <summary>
    /// Description for BrowserPage.
    /// </summary>
    public partial class BrowserPage : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the BrowserPage class.
        /// </summary>
        public BrowserPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var uriStart = e.Uri.OriginalString.IndexOf("http://");
            var targetString = e.Uri.OriginalString.Substring(uriStart, e.Uri.OriginalString.Length - uriStart);
            var targetUri = new Uri(targetString);
            Messenger.Default.Send<Uri>(targetUri, "BrowserNavigationMessage");
        }
    }
}