using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using power4you_admin.Models;
using power4you_admin.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace power4you_admin.Views;

public partial class AnlagenPage : UserControl
{
    private readonly AnlageService _anlageService;
    private readonly CustomerService _customerService;
    private readonly SolarmodultypService _solarmodultypService;
    private readonly ObservableCollection<Anlage> _allAnlagen;
    private readonly ICollectionView _anlagenView;

    public AnlagenPage()
    {
        InitializeComponent();
        
        // Get services from DI container
        _anlageService = ((App)Application.Current).ServiceProvider.GetRequiredService<AnlageService>();
        _customerService = ((App)Application.Current).ServiceProvider.GetRequiredService<CustomerService>();
        _solarmodultypService = ((App)Application.Current).ServiceProvider.GetRequiredService<SolarmodultypService>();
        
        // Initialize collections
        _allAnlagen = new ObservableCollection<Anlage>();
        _anlagenView = CollectionViewSource.GetDefaultView(_allAnlagen);
        _anlagenView.Filter = FilterAnlagen;
        
        // Set DataGrid source
        AnlagenDataGrid.ItemsSource = _anlagenView;
        
        // Load data
        Loaded += async (s, e) => await LoadAnlagenAsync();
    }

    private async Task LoadAnlagenAsync()
    {
        // Operations that modify UI directly MUST be on the UI thread.
        // Database operations can be on a background thread.

        await Dispatcher.InvokeAsync(() => ShowLoading(true));
        await Dispatcher.InvokeAsync(() => StatusText.Text = "Lade Installationen..."); // Translated
        
        List<Anlage> anlagenList = new List<Anlage>();
        bool success = false;

        try
        {
            // Perform the database/service call on a background thread explicitly
            anlagenList = await Task.Run(() => _anlageService.GetAllAnlagenAsync());
            success = true;
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() => 
            {
                MessageBox.Show($"Fehler beim Laden der Installationen: {ex.Message}", 
                              "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Laden der Installationen"; // Translated
            });
        }
        finally
        {
            if (success)
            {
                // UI updates back on the UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    _allAnlagen.Clear();
                    foreach (var anlage in anlagenList)
                    {
                        _allAnlagen.Add(anlage);
                    }
                    _anlagenView.Refresh();
                    UpdateStatusText();
                });
            }
            await Dispatcher.InvokeAsync(() => ShowLoading(false));
        }
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        CreateAnlageButton.IsEnabled = !isLoading;
        RefreshButton.IsEnabled = !isLoading;
        ExportButton.IsEnabled = !isLoading;
        SearchTextBox.IsEnabled = !isLoading;
        PerformanceFilterComboBox.IsEnabled = !isLoading;
        SizeFilterComboBox.IsEnabled = !isLoading;
        ClearFiltersButton.IsEnabled = !isLoading;
    }

    private void UpdateStatusText()
    {
        var totalCount = _allAnlagen.Count;
        var filteredAnlagen = _anlagenView.Cast<Anlage>().ToList();
        var filteredCount = filteredAnlagen.Count;
        var totalPowerKWp = _allAnlagen.Sum(a => a.GesamtleistungKWp);
        var filteredPowerKWp = filteredAnlagen.Sum(a => a.GesamtleistungKWp);

        if (filteredCount == totalCount)
        {
            StatusText.Text = $"{totalCount} installation(s) • {totalPowerKWp:F1} kWp total capacity";
        }
        else
        {
            StatusText.Text = $"{filteredCount} of {totalCount} installation(s) ({filteredPowerKWp:F1} kWp) • {totalPowerKWp:F1} kWp total capacity";
        }
    }

    private bool FilterAnlagen(object item)
    {
        if (item is not Anlage anlage)
            return false;

        // Search filter
        var searchText = SearchTextBox.Text?.ToLower() ?? "";
        if (!string.IsNullOrEmpty(searchText))
        {
            var searchMatch = (anlage.Name?.ToLower().Contains(searchText) == true) ||
                              (anlage.Kunde?.Vorname?.ToLower().Contains(searchText) == true) ||
                              (anlage.Kunde?.Nachname?.ToLower().Contains(searchText) == true) ||
                              (anlage.Standort?.ToLower().Contains(searchText) == true);
            
            if (!searchMatch)
                return false;
        }

        // Performance filter
        if (PerformanceFilterComboBox.SelectedItem is ComboBoxItem performanceItem && performanceItem.Tag is string performanceTag)
        {
            var performanceCategory = GetPerformanceCategory(anlage.LetztePowerAusgabe, anlage.GesamtleistungKWp);
            if (performanceTag != "Alle" && performanceCategory != performanceTag)
                return false;
        }

        // Size filter
        if (SizeFilterComboBox.SelectedItem is ComboBoxItem sizeItem && sizeItem.Tag is string sizeTag)
        {
            var moduleCount = anlage.AnzahlModule;
            bool sizeMatch = sizeTag switch
            {
                "Klein" => moduleCount >= 1 && moduleCount <= 5,
                "Mittel" => moduleCount >= 6 && moduleCount <= 15,
                "Groß" => moduleCount >= 16,
                _ => true // "Alle"
            };
            if (!sizeMatch)
                return false;
        }

        return true;
    }

    private static string GetPerformanceCategory(double? currentOutput, double totalCapacityKWp)
    {
        if (!currentOutput.HasValue || totalCapacityKWp == 0)
            return "Keine Daten";

        var capacityW = totalCapacityKWp * 1000.0;
        var efficiency = currentOutput.Value / capacityW;

        return efficiency switch
        {
            >= 0.7 => "Hohe Leistung",
            >= 0.4 => "Mittlere Leistung",
            _ => "Niedrige Leistung"
        };
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _anlagenView.Refresh();
        UpdateStatusText();
    }

    private void PerformanceFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_anlagenView != null)
        {
            _anlagenView.Refresh();
            UpdateStatusText();
        }
    }

    private void SizeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_anlagenView != null)
        {
            _anlagenView.Refresh();
            UpdateStatusText();
        }
    }

    private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Text = "";
        PerformanceFilterComboBox.SelectedIndex = 0;
        SizeFilterComboBox.SelectedIndex = 0;
        _anlagenView.Refresh();
        UpdateStatusText();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadAnlagenAsync();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var filteredAnlagen = _anlagenView.Cast<Anlage>().ToList();
            
            if (!filteredAnlagen.Any())
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Anlagen_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportToCsv(filteredAnlagen, saveFileDialog.FileName);
                MessageBox.Show($"Data exported successfully to {saveFileDialog.FileName}", 
                              "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting data: {ex.Message}", 
                          "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ExportToCsv(List<Anlage> anlagen, string filePath)
    {
        using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8); 
        
        // Use semicolon as delimiter for better Excel compatibility in some regions
        writer.WriteLine("Anlagenname;Kunden Vorname;Kunden Nachname;Standort;Modulanzahl;Gesamtleistung (kWp);Aktuelle Leistung (W);Status");
        
        foreach (var anlage in anlagen)
        {
            var status = GetPerformanceCategory(anlage.LetztePowerAusgabe, anlage.GesamtleistungKWp);
            // Ensure strings with potential commas or semicolons are quoted
            var line = string.Format("\"{0}\";\"{1}\";\"{2}\";\"{3}\";{4};{5:F2};{6};\"{7}\"",
                                 anlage.FormattedName?.Replace("\"", "\"\""), 
                                 anlage.Kunde?.Vorname?.Replace("\"", "\"\""), 
                                 anlage.Kunde?.Nachname?.Replace("\"", "\"\""), 
                                 anlage.Standort?.Replace("\"", "\"\""), 
                                 anlage.AnzahlModule, 
                                 anlage.GesamtleistungKWp, 
                                 anlage.LetztePowerAusgabe?.ToString("F0") ?? "N/A",
                                 status?.Replace("\"", "\"\""));
            writer.WriteLine(line);
        }
    }

    private void AnlagenDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (AnlagenDataGrid.SelectedItem is Anlage selectedAnlage)
        {
            ShowAnlageDetails(selectedAnlage);
        }
    }

    private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Anlage anlage)
        {
            ShowAnlageDetails(anlage);
        }
    }

    private void ViewPerformanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Anlage anlage })
        {
            ShowPerformanceDetails(anlage);
        }
    }

    private void EditAnlageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Anlage anlage })
        {
            ShowAnlageEditDialog(anlage, isNewAnlage: false);
        }
    }

    private void CreateAnlageButton_Click(object sender, RoutedEventArgs e)
    {
        var createDialog = new AnlageCreateDialog(_anlageService, _customerService, _solarmodultypService)
        {
            Owner = Window.GetWindow(this)
        };

        var result = createDialog.ShowDialog();
        if (result == true) // Anlage was created
        {
            Task.Run(async () => await LoadAnlagenAsync()); // Refresh the Anlagen list
        }
    }

    private void ShowAnlageDetails(Anlage anlage)
    {
        try
        {
            var detailDialog = new AnlageDetailDialog(anlage, _anlageService);
            detailDialog.Owner = Window.GetWindow(this);
            detailDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening installation details: {ex.Message}", 
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowPerformanceDetails(Anlage anlage)
    {
        try
        {
            var performanceDialog = new AnlagePerformanceDialog(anlage, _anlageService);
            performanceDialog.Owner = Window.GetWindow(this);
            performanceDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening performance details: {ex.Message}", 
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowAnlageEditDialog(Anlage anlage, bool isNewAnlage)
    {
        var editDialog = new AnlageEditDialog(anlage, _anlageService, _solarmodultypService)
        {
            Owner = Window.GetWindow(this)
        };

        var result = editDialog.ShowDialog();
        if (result == true)
        {
            Task.Run(async () => await LoadAnlagenAsync());
        }
    }

    private async void DeleteAnlageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Anlage anlageToDelete })
        {
            var result = MessageBox.Show($"Möchten Sie die Anlage '{anlageToDelete.FormattedName}' und alle zugehörigen Module und Leistungsdaten wirklich unwiderruflich löschen?",
                                           "Anlage löschen bestätigen",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ShowLoading(true);
                try
                {
                    await _anlageService.DeleteAnlageAsync(anlageToDelete.AnlageId);
                    _allAnlagen.Remove(anlageToDelete);
                    _anlagenView.Refresh(); // Refresh view after removal
                    UpdateStatusText(); // Update status text
                    MessageBox.Show("Anlage erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Löschen der Anlage: {ex.Message}", "Löschfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }
    }
} 