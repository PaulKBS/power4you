using power4you_admin.Models;
using power4you_admin.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks; // Added
using System.Collections.Generic; // Added

namespace power4you_admin.Views;

public partial class AnlageCreateDialog : Window, INotifyPropertyChanged
{
    private readonly AnlageService _anlageService;
    private readonly CustomerService _customerService;
    private readonly SolarmodultypService _solarmodultypService;

    private Kunde? _selectedCustomer;
    private bool _isCustomerSelectionEnabled = true;
    private bool _isCreateButtonEnabled = false;

    public ObservableCollection<Kunde> EligibleCustomers { get; set; }
    public ObservableCollection<Solarmodultyp> AvailableSolarmodultypen { get; set; }
    public ObservableCollection<Solarmodul> StagedSolarmoduleView { get; set; }

    public bool IsCustomerSelectionEnabled
    {
        get => _isCustomerSelectionEnabled;
        set { _isCustomerSelectionEnabled = value; OnPropertyChanged(nameof(IsCustomerSelectionEnabled)); }
    }

    public bool IsCreateButtonEnabled
    {
        get => _isCreateButtonEnabled;
        set 
        {
            if (_isCreateButtonEnabled != value)
            {
                _isCreateButtonEnabled = value;
                OnPropertyChanged(nameof(IsCreateButtonEnabled));
                System.Diagnostics.Debug.WriteLine($"IsCreateButtonEnabled_Set: Value changed to {value}. OnPropertyChanged invoked.");
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public AnlageCreateDialog(AnlageService anlageService, CustomerService customerService, SolarmodultypService solarmodultypService)
    {
        InitializeComponent();
        DataContext = this;

        _anlageService = anlageService;
        _customerService = customerService;
        _solarmodultypService = solarmodultypService;

        EligibleCustomers = new ObservableCollection<Kunde>();
        AvailableSolarmodultypen = new ObservableCollection<Solarmodultyp>();
        StagedSolarmoduleView = new ObservableCollection<Solarmodul>();

        Loaded += async (s, e) => await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        ShowLoading(true);
        try
        {
            var allCustomers = await _customerService.GetAllKundenAsync();
            // Need to know which customers already have an Anlage.
            // One way is to fetch all Anlagen and get their Kundennummern.
            var anlagen = await _anlageService.GetAllAnlagenAsync(); // Assumes this is efficient enough
            var kundenMitAnlage = anlagen.Select(a => a.Kundennummer).ToHashSet();
            
            EligibleCustomers.Clear();
            foreach (var kunde in allCustomers.Where(k => !kundenMitAnlage.Contains(k.Kundennummer)))
            {
                EligibleCustomers.Add(kunde);
            }

            if (!EligibleCustomers.Any())
            {
                 MessageBox.Show("Alle vorhandenen Kunden haben bereits eine Anlage oder es sind keine Kunden registriert.", "Keine berechtigten Kunden", MessageBoxButton.OK, MessageBoxImage.Information);
                 // Consider disabling the dialog or parts of it
                 CustomerComboBox.IsEnabled = false;
                 ModuleSectionCard.IsEnabled = false;
                 IsCreateButtonEnabled = false;
            }

            var modultypen = await _solarmodultypService.GetAllSolarmodultypenAsync();
            AvailableSolarmodultypen.Clear();
            foreach (var typ in modultypen)
            {
                AvailableSolarmodultypen.Add(typ);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Laden der initialen Daten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ShowLoading(false);
            UpdateCreateButtonState(); // Initial state based on selections
        }
    }

    private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerComboBox.SelectedItem is Kunde selectedKunde)
        {
            _selectedCustomer = selectedKunde;
            SelectedCustomerIdText.Text = selectedKunde.Kundennummer.ToString();
            SelectedCustomerNameText.Text = selectedKunde.VollerName;
            SelectedCustomerAddressText.Text = selectedKunde.VollständigeAdresse;
            SelectedCustomerDetailsBorder.Visibility = Visibility.Visible;
            
            ModuleSectionCard.IsEnabled = true;
            // Explicitly enable child controls that depend on _selectedCustomer here
            SolarmodultypComboBox.IsEnabled = true;
            ModuleQuantityTextBox.IsEnabled = true;
            AddStagedModulButton.IsEnabled = true;

            IsCustomerSelectionEnabled = false; // Lock customer selection once chosen
        }
        else
        {
            _selectedCustomer = null;
            SelectedCustomerDetailsBorder.Visibility = Visibility.Collapsed;
            ModuleSectionCard.IsEnabled = false;
            // Explicitly disable child controls if customer is de-selected (though not typical flow here)
            SolarmodultypComboBox.IsEnabled = false;
            ModuleQuantityTextBox.IsEnabled = false;
            AddStagedModulButton.IsEnabled = false;
        }
        UpdateCreateButtonState();
    }

    private void AddStagedModulButton_Click(object sender, RoutedEventArgs e)
    {
        if (SolarmodultypComboBox.SelectedItem is Solarmodultyp selectedTyp)
        {
            if (!int.TryParse(ModuleQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Bitte eine gültige Anzahl (größer als 0) eingeben.", "Ungültige Anzahl", MessageBoxButton.OK, MessageBoxImage.Warning);
                ModuleQuantityTextBox.Focus();
                ModuleQuantityTextBox.SelectAll();
                return;
            }

            for (int i = 0; i < quantity; i++)
            {
                var stagedModul = new Solarmodul
                {
                    Solarmodultypnummer = selectedTyp.Solarmodultypnummer,
                    Solarmodultyp = selectedTyp 
                };
                StagedSolarmoduleView.Add(stagedModul);
            }
            UpdateNoStagedModulesTextVisibility();
            ModuleQuantityTextBox.Text = "1"; // Reset quantity to 1 after adding
        }
        else
        {
            MessageBox.Show("Bitte einen Solarmodultyp auswählen.", "Auswahl erforderlich", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        UpdateCreateButtonState();
    }

    private void RemoveStagedModulButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Solarmodul modulToRemove)
        {
            StagedSolarmoduleView.Remove(modulToRemove);
            UpdateNoStagedModulesTextVisibility();
        }
        UpdateCreateButtonState();
    }

    private async void CreateAnlageInternalButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCustomer == null)
        {
            MessageBox.Show("Bitte zuerst einen Kunden auswählen.", "Kunde fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!StagedSolarmoduleView.Any())
        {
            MessageBox.Show("Bitte mindestens ein Solarmodul hinzufügen, um die Anlage zu erstellen.", "Module fehlen", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ShowLoading(true);
        IsCreateButtonEnabled = false;
        IsCustomerSelectionEnabled = false;
        try
        {
            // Anlage object itself is not persisted, modules are linked to Kunde
            foreach (var stagedModul in StagedSolarmoduleView)
            {
                var addedModul = await _anlageService.AddSolarmodulToKundeAsync(_selectedCustomer.Kundennummer, stagedModul.Solarmodultypnummer);
                if (addedModul == null)
                {
                    MessageBox.Show($"Fehler beim Speichern von Modul: {stagedModul.Solarmodultyp.Bezeichnung}. Die Erstellung wird abgebrochen.", "Fehler beim Speichern", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Potentially offer to retry or clean up already added modules if partial success is an issue
                    return; 
                }
            }
            MessageBox.Show("Neue Anlage erfolgreich für den Kunden erstellt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ein Fehler ist beim Erstellen der Anlage aufgetreten: {ex.Message}", "Erstellungsfehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ShowLoading(false);
            // Re-enable if staying on dialog, but we close on success.
            // If error, user might want to retry, so consider re-enabling relevant controls.
            if (DialogResult != true) 
            {
                IsCustomerSelectionEnabled = _selectedCustomer == null; // only re-enable if no customer selected before fail
                IsCreateButtonEnabled = _selectedCustomer != null && StagedSolarmoduleView.Any();
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void UpdateNoStagedModulesTextVisibility()
    {
        NoStagedModulesText.Visibility = StagedSolarmoduleView.Any() ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateCreateButtonState()
    {
        bool shouldBeEnabled = _selectedCustomer != null && StagedSolarmoduleView.Any();
        IsCreateButtonEnabled = shouldBeEnabled; // Keep this for the binding system
        
        // For debugging & direct control:
        System.Diagnostics.Debug.WriteLine($"UpdateCreateButtonState: _selectedCustomer is null? {(_selectedCustomer == null)}, StagedModules count: {StagedSolarmoduleView.Count}, shouldBeEnabled: {shouldBeEnabled}");
        CreateAnlageInternalButton.IsEnabled = shouldBeEnabled; // Direct manipulation
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingOverlayDialog.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        CustomerComboBox.IsEnabled = !isLoading && IsCustomerSelectionEnabled; 
        
        bool moduleSectionShouldBeEnabled = !isLoading && _selectedCustomer != null;
        ModuleSectionCard.IsEnabled = moduleSectionShouldBeEnabled;
        SolarmodultypComboBox.IsEnabled = moduleSectionShouldBeEnabled;
        ModuleQuantityTextBox.IsEnabled = moduleSectionShouldBeEnabled; // Explicitly handle TextBox
        AddStagedModulButton.IsEnabled = moduleSectionShouldBeEnabled;
        StagedModulesDataGrid.IsEnabled = !isLoading; 

        CreateAnlageInternalButton.IsEnabled = !isLoading && IsCreateButtonEnabled; 
        CancelButton.IsEnabled = !isLoading; 
    }

    private void ModuleQuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Allow only digits
        if (!char.IsDigit(e.Text, e.Text.Length - 1))
        {
            e.Handled = true; // Mark the event as handled, so the character is not entered
        }
    }
} 