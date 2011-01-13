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

        /// <summary>
        /// Uri to which the Uri display binds
        /// </summary>
        private Uri _displayUri;
        public Uri DisplayUri
        {
            get { return _displayUri; }
            set
            {
                if (_displayUri == value) return;
                _displayUri = value;
                RaisePropertyChanged("DisplayUri");
            }
        }

        /// <summary>
        /// Uri to which the browser control itself binds
        /// </summary>
        private Uri _browserUri;
        public Uri BrowserUri
        {
            get { return _browserUri; }
            set {
                if (_browserUri == value) return;
                _browserUri = value;
                RaisePropertyChanged("BrowserUri");
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
                                                                                                     DisplayUri = uri;
                                                                                                     BrowserUri = uri;
                                                                                                 }));


                NavigationComplete = new RelayCommand<NavigationEventArgs>(args => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                                                                                                             {
                                                                                                                                 DisplayUri = args.Uri;
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