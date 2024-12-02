using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using power4you_client.Models;
using power4you_client.Services;

namespace power4you_client.Views.SolarAnlage
{
    public partial class AnlageEditView : UserControl
    {
        private Anlage _currentAnlage;
        private ObservableCollection<SolarModulTyp> _module;
        private List<string> _mockKunden = new List<string> 
        { 
            "Peter Meyer", 
            "Anna Schmidt", 
            "Max Mustermann" 
        };
        private List<SolarModulTyp> _verfuegbareModule;

        public AnlageEditView(Anlage anlage = null)
        {
            InitializeComponent();
            _currentAnlage = anlage;
            _module = new ObservableCollection<SolarModulTyp>();

            // Initialize ComboBoxes
            KundenComboBox.ItemsSource = _mockKunden;
            _verfuegbareModule = SolarModulService.GetAllModulTypen();
            ModulTypComboBox.ItemsSource = _verfuegbareModule;
            ModulTypComboBox.DisplayMemberPath = "Bezeichnung";

            if (anlage != null)
            {
                // Edit mode
                HeaderText.Text = "Anlage bearbeiten";
                LoadAnlageData(anlage);
                foreach (var modul in anlage.Module)
                {
                    _module.Add(modul);
                }
            }

            ModuleDataGrid.ItemsSource = _module;
            UpdateGesamtleistung();
        }

        private void LoadAnlageData(Anlage anlage)
        {
            NameTextBox.Text = anlage.Name;
            KundenComboBox.SelectedItem = anlage.KundenName;
            InstallationsDatumPicker.SelectedDate = anlage.InstallationsDatum;
            StandortTextBox.Text = anlage.Standort;
        }

        private void UpdateGesamtleistung()
        {
            double gesamtleistung = _module.Sum(m => m.Pmpp) / 1000.0; // Convert to kWp
            GesamtleistungRun.Text = gesamtleistung.ToString("N2");
        }

        private void AddModul_Click(object sender, RoutedEventArgs e)
        {
            if (ModulTypComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(ModulAnzahlTextBox.Text))
            {
                MessageBox.Show("Bitte wählen Sie einen Modultyp und geben Sie die Anzahl ein.", "Eingabe erforderlich", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ModulAnzahlTextBox.Text, out int anzahl) || anzahl <= 0)
            {
                MessageBox.Show("Bitte geben Sie eine gültige Anzahl ein.", "Ungültige Eingabe", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedModul = (SolarModulTyp)ModulTypComboBox.SelectedItem;
            
            for (int i = 0; i < anzahl; i++)
            {
                _module.Add(new SolarModulTyp
                {
                    Bezeichnung = selectedModul.Bezeichnung,
                    Pmpp = selectedModul.Pmpp,
                    Umpp = selectedModul.Umpp,
                    Impp = selectedModul.Impp
                });
            }

            UpdateGesamtleistung();

            // Clear inputs
            ModulTypComboBox.SelectedIndex = -1;
            ModulAnzahlTextBox.Text = "";
        }

        private void RemoveModul_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is SolarModulTyp modul)
            {
                _module.Remove(modul);
                UpdateGesamtleistung();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var anlage = _currentAnlage ?? new Anlage();
            anlage.Name = NameTextBox.Text;
            anlage.KundenName = KundenComboBox.SelectedItem?.ToString();
            anlage.InstallationsDatum = InstallationsDatumPicker.SelectedDate ?? DateTime.Now;
            anlage.Standort = StandortTextBox.Text;
            anlage.Module = new List<SolarModulTyp>(_module);
            anlage.GesamtLeistung = _module.Sum(m => m.Pmpp) / 1000.0; // Convert to kWp

            if (_currentAnlage == null)
            {
                AnlagenService.AddAnlage(anlage);
            }
            else
            {
                AnlagenService.UpdateAnlage(anlage);
            }

            // Navigate back to AnlagenView
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = new AnlagenView();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowValidationError("Bitte geben Sie einen Namen für die Anlage ein.");
                return false;
            }

            if (KundenComboBox.SelectedItem == null)
            {
                ShowValidationError("Bitte wählen Sie einen Kunden aus.");
                return false;
            }

            if (!InstallationsDatumPicker.SelectedDate.HasValue)
            {
                ShowValidationError("Bitte wählen Sie ein Installationsdatum aus.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(StandortTextBox.Text))
            {
                ShowValidationError("Bitte geben Sie einen Standort ein.");
                return false;
            }

            if (_module.Count == 0)
            {
                ShowValidationError("Bitte fügen Sie mindestens ein Modul hinzu.");
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to AnlagenView
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = new AnlagenView();
            }
        }
    }
} 