using System;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace TechNews.ViewModel
{
    public class BrowserViewModel : ViewModelBase
    {

        //Used for quick and easy computation

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
                RaisePropertyChanged("IsProgressBarVisible");
            }
        }

        public Visibility IsProgressBarVisible
        {
            get
            {
                return IsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Uri _uri;
        public Uri Uri
        {
            get { return _uri; }
            set { 
                _uri = value;
                RaisePropertyChanged("Uri");
            }
        }

        #region Commands

        public RelayCommand<NavigationEventArgs> NavigationComplete { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the BrowserViewModel class.
        /// </summary>
        public BrowserViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                Messenger.Default.Register<Uri>(this, "BrowserNavigationMessage",
                                                uri => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                                                                                 {
                                                                                                     IsLoading = true;
                                                                                                     Uri = uri;
                                                                                                 }));


                NavigationComplete = new RelayCommand<NavigationEventArgs>(args => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                                                                                                             {
                                                                                                                                 Uri = args.Uri;
                                                                                                                                 IsLoading = false;
                                                                                                                             }));
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}