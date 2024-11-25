using System.Windows;
using power4you_client.Views;

namespace power4you_client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Start with Dashboard view
            MainFrame.Navigate(new DashboardView());
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardView());
        }

        private void KundenButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new KundenDetailView());
        }

        private void AnlagenButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SolarModulTypenView());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new SettingsView());
        }
    }
}
