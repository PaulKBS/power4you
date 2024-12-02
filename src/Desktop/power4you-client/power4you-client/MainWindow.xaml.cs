using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using power4you_client.Views;
using power4you_client.Views.SolarAnlage;
using power4you_client.Views.SolarModule;
using power4you_client.Views.Settings;

namespace power4you_client
{
    public partial class MainWindow : Window
    {
        private Button _activeButton;

        public MainWindow()
        {
            InitializeComponent();
            
            // Animate menu items on load
            foreach (FrameworkElement item in NavigationItems.Children)
            {
                item.Opacity = 0;
                var storyboard = (Storyboard)FindResource("MenuItemFadeIn");
                Storyboard.SetTarget(storyboard, item);
                storyboard.Begin();
            }

            // Set default view and active button
            SetActiveButton(DashboardButton);
            MainFrame.Content = new DashboardView();
        }

        private void SetActiveButton(Button button)
        {
            if (_activeButton != null)
                _activeButton.Tag = null;
            
            _activeButton = button;
            _activeButton.Tag = "Active";
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);
            MainFrame.Content = new DashboardView();
        }

        private void KundenButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(KundenButton);
            MainFrame.Content = new KundenView();
        }

        private void AnlagenButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(AnlagenButton);
            MainFrame.Content = new AnlagenView();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SettingsButton);
            MainFrame.Content = new SettingsView();
        }

        private void ModuleButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ModuleButton);
            MainFrame.Content = new ModuleView();
        }
    }
}
