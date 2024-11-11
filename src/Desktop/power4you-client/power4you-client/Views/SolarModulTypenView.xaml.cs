using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using power4you_client.Models;

namespace power4you_client.Views
{
    public partial class SolarModulTypenView : UserControl
    {
        private ObservableCollection<SolarModulTyp> _solarModule;
        private ICollectionView _solarModuleView;

        public SolarModulTypenView()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            // Beispieldaten - ersetzen Sie dies durch Ihre Datenbankabfrage
            _solarModule = new ObservableCollection<SolarModulTyp>
            {
                new SolarModulTyp
                {
                    Solarmodultypnummer = 1,
                    Bezeichnung = "Beispielmodul 1",
                    Umpp = 31.2f,
                    Impp = 9.8f,
                    Pmpp = 305.0f
                }
                // Weitere Module hier hinzufügen
            };

            _solarModuleView = CollectionViewSource.GetDefaultView(_solarModule);
            SolarModuleGrid.ItemsSource = _solarModuleView;
            UpdateStatusBar();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                _solarModuleView.Filter = null;
            }
            else
            {
                _solarModuleView.Filter = item =>
                {
                    var modul = item as SolarModulTyp;
                    return modul.Bezeichnung.ToLower().Contains(searchText);
                };
            }

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            int totalCount = _solarModule.Count;
            int filteredCount = _solarModuleView.Cast<SolarModulTyp>().Count();

            if (totalCount == filteredCount)
            {
                TotalCountText.Text = $"Gesamt: {totalCount} Einträge";
            }
            else
            {
                TotalCountText.Text = $"Gefiltert: {filteredCount} von {totalCount} Einträgen";
            }
        }

        private void AddNewType_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementieren Sie hier die Logik zum Öffnen eines neuen Dialogs für die Modulerstellung
            var newDialog = new SolarModulTypDialog();
            if (newDialog.ShowDialog() == true)
            {
                // Fügen Sie das neue Modul zur Collection hinzu
                _solarModule.Add(newDialog.SolarModulTyp);
                UpdateStatusBar();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var modulTyp = button.DataContext as SolarModulTyp;

            if (modulTyp != null)
            {
                var result = MessageBox.Show(
                    $"Möchten Sie den Solarmodultyp '{modulTyp.Bezeichnung}' wirklich löschen?",
                    "Löschen bestätigen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: Implementieren Sie hier die Löschlogik für die Datenbank
                    _solarModule.Remove(modulTyp);
                    UpdateStatusBar();
                    StatusText.Text = "Solarmodultyp wurde gelöscht";
                }
            }
        }

        private void SolarModuleGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var modulTyp = e.Row.Item as SolarModulTyp;
                if (modulTyp != null)
                {
                    // TODO: Implementieren Sie hier die Update-Logik für die Datenbank
                    StatusText.Text = $"Solarmodultyp '{modulTyp.Bezeichnung}' wurde aktualisiert";
                }
            }
        }
    }
}