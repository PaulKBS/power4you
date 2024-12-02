using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using power4you_client.Models;
using power4you_client.Services;

namespace power4you_client.Views.SolarAnlage
{
    public partial class AnlagenView : UserControl
    {
        public event Action<UserControl> OnNavigate;
        private ObservableCollection<Anlage> _allAnlagen;

        public UserControl CurrentView
        {
            get { return this; }
        }

        public AnlagenView()
        {
            InitializeComponent();
            LoadMockData();
        }

        private void LoadMockData()
        {
            _allAnlagen = new ObservableCollection<Anlage>(AnlagenService.GetAllAnlagen());
            UpdateAnlagenList(_allAnlagen);
        }

        private void UpdateAnlagenList(IEnumerable<Anlage> anlagen)
        {
            AnlagenItemsControl.ItemsSource = anlagen;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            var filteredAnlagen = _allAnlagen.Where(a =>
                a.Name?.ToLower().Contains(searchText) == true ||
                a.KundenName?.ToLower().Contains(searchText) == true ||
                a.Standort?.ToLower().Contains(searchText) == true
            ).ToList();

            UpdateAnlagenList(filteredAnlagen);
        }

        private void AddAnlage_Click(object sender, RoutedEventArgs e)
        {
            var editView = new AnlageEditView();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = editView;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Anlage anlage)
            {
                var editView = new AnlageEditView(anlage);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
                {
                    mainFrame.Content = editView;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Anlage anlage)
            {
                var result = MessageBox.Show(
                    $"Möchten Sie die Anlage \"{anlage.Name}\" wirklich löschen?",
                    "Anlage löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AnlagenService.DeleteAnlage(anlage);
                    _allAnlagen.Remove(anlage);
                }
            }
        }
    }
} 