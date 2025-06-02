using Microsoft.Extensions.DependencyInjection;
using power4you_admin.Models;
using power4you_admin.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace power4you_admin.Views;

public partial class AnlageEditDialog : Window, INotifyPropertyChanged
{
    private readonly AnlageService _anlageService;
    private readonly SolarmodultypService _solarmodultypService;
    private Anlage _currentAnlage;
    private List<Solarmodultyp> _availableSolarmodultypen = new List<Solarmodultyp>();

    public ObservableCollection<Solarmodul> SolarmoduleView { get; set; }

    public Anlage CurrentAnlage
    {
        get => _currentAnlage;
        set
        {
            _currentAnlage = value;
            OnPropertyChanged(nameof(CurrentAnlage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public AnlageEditDialog(Anlage anlage, AnlageService anlageService, SolarmodultypService solarmodultypService)
    {
        InitializeComponent();
        DataContext = this;

        _currentAnlage = anlage;
        CurrentAnlage = anlage;

        _anlageService = anlageService;
        _solarmodultypService = solarmodultypService;

        bool isNewAnlage = anlage.AnlageId == 0 && (anlage.Solarmodule == null || !anlage.Solarmodule.Any());

        if (isNewAnlage)
        {
            TitleText.Text = "Neue Anlage erstellen";
            this.Title = "Neue Anlage erstellen";
        }
        else
        {
            TitleText.Text = $"Anlage bearbeiten: {anlage.Name}";
            this.Title = $"Anlage bearbeiten: {anlage.Name}";
        }

        SolarmoduleView = new ObservableCollection<Solarmodul>(anlage.Solarmodule ?? new List<Solarmodul>());
        SolarmoduleDataGrid.ItemsSource = SolarmoduleView;
        UpdateNoModulesTextVisibility();

        Loaded += async (s, e) => await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        ShowLoading(true);
        try
        {
            _availableSolarmodultypen = await _solarmodultypService.GetAllSolarmodultypenAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Laden der Solarmodultypen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void UpdateNoModulesTextVisibility()
    {
        NoModulesText.Visibility = !SolarmoduleView.Any() ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void AddModulButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_availableSolarmodultypen.Any())
        {
            MessageBox.Show("Keine Solarmodultypen verfügbar. Bitte zuerst Modultypen anlegen.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var addModuleDialog = new Window
        {
            Title = "Solarmodule hinzufügen",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            Background = (System.Windows.Media.Brush)FindResource("MaterialDesignPaper"),
            Foreground = (System.Windows.Media.Brush)FindResource("MaterialDesignBody"),
            Padding = new Thickness(32),
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow // Removes Min/Max buttons
        };

        var mainPanel = new StackPanel { MinWidth = 350 };

        var titleText = new TextBlock
        {
            Text = "Solarmodule hinzufügen",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 24),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        mainPanel.Children.Add(titleText);

        // Solarmodultyp ComboBox
        var comboBox = new ComboBox
        {
            ItemsSource = _availableSolarmodultypen,
            DisplayMemberPath = "Bezeichnung",
            Margin = new Thickness(0, 0, 0, 20),
            Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedComboBox"),
            
        };
        HintAssist.SetHint(comboBox, "Solarmodultyp auswählen");
        mainPanel.Children.Add(comboBox);

        // Quantity TextBox (replaces NumericUpDown)
        var quantityTextBox = new TextBox 
        {
            Margin = new Thickness(0, 0, 0, 24),
            Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedTextBox") // Use Material Design style if available
        };
        HintAssist.SetHint(quantityTextBox, "Anzahl (z.B. 1)"); 
        mainPanel.Children.Add(quantityTextBox);

        // Buttons Panel
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,16,0,0) };
        
        var cancelButtonDialog = new Button
        {
            Content = "ABBRECHEN",
            Style = (Style)FindResource("MaterialDesignOutlinedButton"),
            Margin = new Thickness(0, 0, 8, 0),
            IsCancel = true
        };
        cancelButtonDialog.Click += (s, args) => addModuleDialog.DialogResult = false;
        buttonPanel.Children.Add(cancelButtonDialog);

        var addButtonDialog = new Button
        {
            Content = "HINZUFÜGEN",
            Style = (Style)FindResource("MaterialDesignRaisedButton"),
            IsDefault = true
        };
       
        addButtonDialog.Click += async (s, args) =>
        {
            Solarmodultyp? selectedTyp = comboBox.SelectedItem as Solarmodultyp;
            if (selectedTyp != null && int.TryParse(quantityTextBox.Text, out int quantity) && quantity > 0)
            {
                addModuleDialog.DialogResult = true; 
                addModuleDialog.Close(); 

                ShowLoading(true);
                int successCount = 0;
                try
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        var newModul = await _anlageService.AddSolarmodulToKundeAsync(CurrentAnlage.Kundennummer, selectedTyp.Solarmodultypnummer);
                        if (newModul != null)
                        {
                            newModul.Solarmodultyp = _availableSolarmodultypen.FirstOrDefault(t => t.Solarmodultypnummer == newModul.Solarmodultypnummer);
                            SolarmoduleView.Add(newModul);
                            successCount++;
                        }
                    }
                    UpdateNoModulesTextVisibility();
                    if (successCount < quantity)
                    {
                         MessageBox.Show($"{successCount} von {quantity} Modul(en) erfolgreich hinzugefügt. Einige konnten nicht hinzugefügt werden.", "Teilweiser Erfolg", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Hinzufügen der Module: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ShowLoading(false);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Modultyp und geben Sie eine gültige positive Zahl für die Anzahl an.", "Eingabe erforderlich", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };
        buttonPanel.Children.Add(addButtonDialog);
        mainPanel.Children.Add(buttonPanel);
        addModuleDialog.Content = mainPanel;

        addModuleDialog.ShowDialog();
    }

    private async void RemoveModulButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int modulnummer)
        {
            var result = MessageBox.Show("Möchten Sie dieses Modul wirklich entfernen? Zugehörige Leistungsdaten werden ebenfalls gelöscht.", 
                                       "Modul entfernen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ShowLoading(true);
                try
                {
                    bool success = await _anlageService.RemoveSolarmodulAsync(modulnummer);
                    if (success)
                    {
                        var modulToRemove = SolarmoduleView.FirstOrDefault(m => m.Modulnummer == modulnummer);
                        if (modulToRemove != null)
                        {
                            SolarmoduleView.Remove(modulToRemove);
                            UpdateNoModulesTextVisibility();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Entfernen des Moduls.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally { ShowLoading(false); }
            }
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // In this design, changes (add/remove) are made directly via service calls.
        // The "Save" button here effectively just closes the dialog as changes are already persisted.
        // If we were batching changes, this is where _anlageService.UpdateAnlageAsync(CurrentAnlage) would be called.
        // For now, we can just confirm and close or refresh the underlying page if necessary.
        
        // Potentially trigger a refresh on the calling page if data has changed.
        DialogResult = true; // Indicates changes were made (or attempted)
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // If we had complex un-saved changes, we might confirm here.
        DialogResult = false;
        Close();
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingOverlayDialog.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        // Consider disabling main content grid as well
    }
} 