using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using power4you_client.Models.Kunden;
using power4you_client.Services;

namespace power4you_client.Views
{
    public partial class KundenView : UserControl
    {
        private List<Kunde> _alleKunden;
        private List<Kunde> _angezeigeKunden;
        private Storyboard _fadeIn;

        public KundenView()
        {
            InitializeComponent();
            _fadeIn = (Storyboard)FindResource("FadeIn");
            _fadeIn.Begin(this);
            LoadKunden();
        }

        private void LoadKunden()
        {
            _alleKunden = KundenService.GetAllKunden();
            _angezeigeKunden = new List<Kunde>(_alleKunden);
            KundenItemsControl.ItemsSource = _angezeigeKunden;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _angezeigeKunden = new List<Kunde>(_alleKunden);
            }
            else
            {
                _angezeigeKunden = _alleKunden.Where(k =>
                    k.FullName.ToLower().Contains(searchText) ||
                    k.Email.ToLower().Contains(searchText) ||
                    k.FullAddress.ToLower().Contains(searchText) ||
                    k.Telefon.ToLower().Contains(searchText)
                ).ToList();
            }

            KundenItemsControl.ItemsSource = null;
            KundenItemsControl.ItemsSource = _angezeigeKunden;
        }

        private void AddKunde_Click(object sender, RoutedEventArgs e)
        {
            var kundenAnlageView = new KundenAnlageView();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = kundenAnlageView;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var kunde = button.Tag as Kunde;
            
            if (kunde != null)
            {
                var kundenAnlageView = new KundenAnlageView(kunde);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
                {
                    mainFrame.Content = kundenAnlageView;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var kunde = button.Tag as Kunde;
            
            if (kunde != null)
            {
                var result = MessageBox.Show(
                    $"Möchten Sie den Kunden {kunde.FullName} wirklich löschen?",
                    "Kunde löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    KundenService.DeleteKunde(kunde);
                    LoadKunden();
                }
            }
        }

        private void ViewAnlagen_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var kunde = button.Tag as Kunde;
            
            if (kunde != null)
            {
                // TODO: Implement filtered AnlagenView
                MessageBox.Show($"Anlagen von {kunde.FullName} werden angezeigt...");
            }
        }
    }
} 