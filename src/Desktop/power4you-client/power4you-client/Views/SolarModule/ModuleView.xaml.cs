using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using power4you_client.Models;
using power4you_client.Services;

namespace power4you_client.Views.SolarModule
{
    public partial class ModuleView : UserControl
    {
        private List<SolarModulTyp> _alleModule;

        public ModuleView()
        {
            InitializeComponent();
            LoadModule();
        }

        private void LoadModule()
        {
            _alleModule = SolarModulService.GetAllModulTypen();
            UpdateModuleList(_alleModule);
        }

        private void UpdateModuleList(IEnumerable<SolarModulTyp> module)
        {
            ModuleDataGrid.ItemsSource = null;
            ModuleDataGrid.ItemsSource = module;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            var filteredModule = _alleModule.Where(m =>
                m.Bezeichnung.ToLower().Contains(searchText)
            ).ToList();

            UpdateModuleList(filteredModule);
        }

        private void AddModul_Click(object sender, RoutedEventArgs e)
        {
            var editView = new ModulEditView();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = editView;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SolarModulTyp modul)
            {
                var editView = new ModulEditView(modul);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
                {
                    mainFrame.Content = editView;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SolarModulTyp modul)
            {
                var result = MessageBox.Show(
                    $"Möchten Sie das Modul \"{modul.Bezeichnung}\" wirklich löschen?",
                    "Modul löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SolarModulService.DeleteModulTyp(modul);
                    LoadModule();
                }
            }
        }
    }
} 