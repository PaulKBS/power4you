using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using power4you_client.Models;
using power4you_client.Services;

namespace power4you_client.Views
{
    public partial class KundenView : Page
    {
        private readonly IKundenService _kundenService;
        private ObservableCollection<Kunde> _kunden;
        private CollectionViewSource _viewSource;

        public KundenView()
        {
            InitializeComponent();
            InitializeView();
        }

        private async void InitializeView()
        {
            try
            {
                var kundenListe = await _kundenService.GetAllKundenAsync();
                _kunden = new ObservableCollection<Kunde>(kundenListe);
                _viewSource = new CollectionViewSource { Source = _kunden };
                CustomerGrid.ItemsSource = _viewSource.View;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Kunden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Laden der Daten";
            }
        }

        private void UpdateStatusBar()
        {
            TotalCountText.Text = $"Gesamt: {_kunden.Count} Einträge";
            StatusText.Text = "Bereit";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewSource == null || _viewSource.View == null) return;

            _viewSource.View.Filter = obj =>
            {
                var kunde = obj as Kunde;
                if (kunde == null) return false;

                var searchText = SearchBox.Text.ToLower();
                return kunde.Vorname.ToLower().Contains(searchText) ||
                       kunde.Nachname.ToLower().Contains(searchText) ||
                       kunde.Email.ToLower().Contains(searchText) ||
                       kunde.Ort.ToLower().Contains(searchText) ||
                       kunde.PLZ.Contains(searchText);
            };

            UpdateStatusBar();
        }

        private async void AddNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var neuerKunde = new Kunde
                {
                    Vorname = "Neu",
                    Nachname = "Kunde"
                };

                var addedKunde = await _kundenService.AddKundeAsync(neuerKunde);
                _kunden.Add(addedKunde);
                UpdateStatusBar();
                StatusText.Text = "Neuer Kunde wurde hinzugefügt";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Kunden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Hinzufügen";
            }
        }

        private async void CustomerGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            var kunde = e.Row.Item as Kunde;
            if (kunde == null) return;

            try
            {
                await _kundenService.UpdateKundeAsync(kunde);
                StatusText.Text = "Kunde wurde aktualisiert";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Aktualisieren des Kunden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Aktualisieren";
                InitializeView();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedKunde = CustomerGrid.SelectedItem as Kunde;
            if (selectedKunde == null) return;

            var result = MessageBox.Show(
                $"Möchten Sie den Kunden {selectedKunde.Vorname} {selectedKunde.Nachname} wirklich löschen?",
                "Kunde löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _kundenService.DeleteKundeAsync(selectedKunde.KundenID);
                _kunden.Remove(selectedKunde);
                UpdateStatusBar();
                StatusText.Text = "Kunde wurde gelöscht";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen des Kunden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Löschen";
            }
        }
    }
}