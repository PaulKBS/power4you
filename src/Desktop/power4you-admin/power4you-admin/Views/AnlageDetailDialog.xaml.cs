using System.Windows;
using power4you_admin.Models;
using power4you_admin.Services;

namespace power4you_admin.Views;

public partial class AnlageDetailDialog : Window
{
    private readonly Anlage _anlage;
    private readonly AnlageService _anlageService;

    public AnlageDetailDialog(Anlage anlage, AnlageService anlageService)
    {
        InitializeComponent();
        _anlage = anlage;
        _anlageService = anlageService;
        
        LoadAnlageDetails();
    }

    private void LoadAnlageDetails()
    {
        // Fenstertitel und Untertitel setzen
        TitleText.Text = $"Anlagendetails - {_anlage.FormattedName}";
        SubtitleText.Text = $"Kunde: {_anlage.Kunde.Vorname} {_anlage.Kunde.Nachname}";
        
        // Grundlegende Informationen setzen
        NameText.Text = _anlage.Name;
        CustomerText.Text = $"{_anlage.Kunde.Vorname} {_anlage.Kunde.Nachname}";
        LocationText.Text = _anlage.Standort ?? "Nicht angegeben";
        ModuleCountText.Text = _anlage.AnzahlModule.ToString();
        TotalCapacityText.Text = $"{_anlage.GesamtleistungKWp:F2} kWp";
        CurrentOutputText.Text = _anlage.LetztePowerAusgabe.HasValue ? $"{_anlage.LetztePowerAusgabe.Value:F0} W" : "Keine Daten";
        
        // Moduldaten laden
        ModulesDataGrid.ItemsSource = _anlage.Solarmodule;
    }

    private void ViewPerformanceButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var performanceDialog = new AnlagePerformanceDialog(_anlage, _anlageService);
            performanceDialog.Owner = this;
            performanceDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Öffnen der Leistungsdetails: {ex.Message}", 
                          "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 