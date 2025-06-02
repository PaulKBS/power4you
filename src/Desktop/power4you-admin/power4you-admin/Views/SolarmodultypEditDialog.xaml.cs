using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using power4you_admin.Models;
using power4you_admin.Services;
using MaterialDesignThemes.Wpf; // For DialogHost
using System.Globalization;
using System.Text.RegularExpressions;

namespace power4you_admin.Views;

public partial class SolarmodultypEditDialog : UserControl, INotifyPropertyChanged
{
    private readonly SolarmodultypService _solarmodultypService;
    public Solarmodultyp CurrentSolarmodultyp { get; set; }
    private bool _isNew;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public SolarmodultypEditDialog(Solarmodultyp solarmodultyp, SolarmodultypService solarmodultypService)
    {
        InitializeComponent();
        _solarmodultypService = solarmodultypService;
        CurrentSolarmodultyp = solarmodultyp; // This will be bound to the UI
        DataContext = CurrentSolarmodultyp; 

        _isNew = solarmodultyp.Solarmodultypnummer == 0;
        DialogTitle.Text = _isNew ? "Neuen Solarmodultyp hinzufügen" : "Solarmodultyp bearbeiten";

        // Basic validation for numeric fields - allow only numbers and one decimal separator
        PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        string currentText = textBox?.Text ?? "";
        string newText = currentText.Insert(textBox?.SelectionStart ?? 0, e.Text);

        // Allow only one decimal separator (comma or period) and digits
        // Regex allows numbers like 123, 123.45, 123,45
        Regex regex = new Regex("^[0-9]*([.,][0-9]{0,2})?$"); 
        if (!regex.IsMatch(newText))
        {
            e.Handled = true; // Block input if it doesn't match the pattern
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Perform basic validation before attempting to save
        if (string.IsNullOrWhiteSpace(CurrentSolarmodultyp.Bezeichnung) ||
            CurrentSolarmodultyp.Pmpp <= 0 ||
            CurrentSolarmodultyp.Umpp <= 0 ||
            CurrentSolarmodultyp.Impp <= 0)
        {
            MessageBox.Show("Bitte füllen Sie alle Felder korrekt aus. Leistungs-, Spannungs- und Stromwerte müssen positiv sein.", 
                            "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        DialogLoadingOverlay.Visibility = Visibility.Visible;
        SaveButton.IsEnabled = false;
        CancelButton.IsEnabled = false;

        try
        {
            if (_isNew)
            {
                await _solarmodultypService.AddSolarmodultypAsync(CurrentSolarmodultyp);
            }
            else
            {
                await _solarmodultypService.UpdateSolarmodultypAsync(CurrentSolarmodultyp);
            }
            DialogHost.CloseDialogCommand.Execute(true, this); // Close dialog and return true (success)
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Speichern des Solarmodultyps: {ex.Message}", "Speicherfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            DialogLoadingOverlay.Visibility = Visibility.Collapsed;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }
    }
} 