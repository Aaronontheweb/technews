using Microsoft.Phone.Controls;

namespace TechNews
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ViewModelLocator.MainStatic.QueryFeeds.Execute(null);
        }
    }
}
