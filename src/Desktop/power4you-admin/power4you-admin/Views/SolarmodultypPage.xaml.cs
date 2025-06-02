using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using power4you_admin.Models;
using power4you_admin.Services;
using MaterialDesignThemes.Wpf; // For dialogs and icons

namespace power4you_admin.Views;

public partial class SolarmodultypPage : UserControl, INotifyPropertyChanged
{
    private readonly SolarmodultypService _solarmodultypService;
    private readonly ObservableCollection<Solarmodultyp> _solarmodultypen;
    private readonly ICollectionView _solarmodultypenView;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public SolarmodultypPage()
    {
        InitializeComponent();
        DataContext = this;

        _solarmodultypService = ((App)Application.Current).ServiceProvider.GetRequiredService<SolarmodultypService>();
        _solarmodultypen = new ObservableCollection<Solarmodultyp>();
        _solarmodultypenView = CollectionViewSource.GetDefaultView(_solarmodultypen);
        // TODO: Add filtering if search is implemented: _solarmodultypenView.Filter = FilterSolarmodultypen;
        
        SolarmodultypenDataGrid.ItemsSource = _solarmodultypenView;
        Loaded += async (s, e) => await LoadSolarmodultypenAsync();
    }

    private async Task LoadSolarmodultypenAsync()
    {
        ShowLoading(true);
        StatusText.Text = "Lade Modultypen...";
        try
        {
            var typen = await _solarmodultypService.GetAllSolarmodultypenAsync();
            _solarmodultypen.Clear();
            foreach (var typ in typen)
            {
                _solarmodultypen.Add(typ);
            }
            UpdateStatusText();
            EmptyStateTextBlock.Visibility = !_solarmodultypen.Any() ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Fehler beim Laden.";
            MessageBox.Show($"Fehler beim Laden der Solarmodultypen: {ex.Message}", "Ladefehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void UpdateStatusText()
    {
        StatusText.Text = $"{_solarmodultypen.Count} Modultyp(en) geladen";
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        AddSolarmodultypButton.IsEnabled = !isLoading;
        RefreshButton.IsEnabled = !isLoading;
        // Disable DataGrid interactions while loading?
        SolarmodultypenDataGrid.IsEnabled = !isLoading;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadSolarmodultypenAsync();
    }

    private void AddSolarmodultypButton_Click(object sender, RoutedEventArgs e)
    {
        ShowEditSolarmodultypDialog(new Solarmodultyp());
    }

    private void EditSolarmodultypButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Solarmodultyp typToEdit })
        {
            ShowEditSolarmodultypDialog(typToEdit);
        }
    }

    private async void DeleteSolarmodultypButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Solarmodultyp typToDelete })
        {
            var result = MessageBox.Show($"Möchten Sie den Solarmodultyp '{typToDelete.Bezeichnung}' wirklich löschen?",
                                           "Löschen bestätigen",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ShowLoading(true);
                try
                {
                    await _solarmodultypService.DeleteSolarmodultypAsync(typToDelete.Solarmodultypnummer);
                    _solarmodultypen.Remove(typToDelete);
                    UpdateStatusText();
                    EmptyStateTextBlock.Visibility = !_solarmodultypen.Any() ? Visibility.Visible : Visibility.Collapsed;
                    MessageBox.Show("Solarmodultyp erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (InvalidOperationException ex) // Catch specific exception for in-use error
                {
                    MessageBox.Show(ex.Message, "Löschfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Löschen des Solarmodultyps: {ex.Message}", "Löschfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }
    }

    private async void ShowEditSolarmodultypDialog(Solarmodultyp typ)
    {
        var dialog = new SolarmodultypEditDialog(typ, _solarmodultypService);
        var result = await DialogHost.Show(dialog, "SolarmodultypPageDialogHost");

        if (result is bool boolResult && boolResult)
        {
            await LoadSolarmodultypenAsync(); // Reload data if changes were saved
        }
    }
} 