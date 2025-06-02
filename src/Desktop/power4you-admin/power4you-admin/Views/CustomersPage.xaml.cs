using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;
using power4you_admin.Models;
using power4you_admin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace power4you_admin.Views;

public partial class CustomersPage : UserControl
{
    private readonly DatabaseService _databaseService;
    private readonly AnlageService _anlageService;
    private ObservableCollection<Kunde> _customers = new();
    private ObservableCollection<Kunde> _filteredCustomers = new();
    private ICollectionView _customersView;

    public CustomersPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _anlageService = ((App)Application.Current).ServiceProvider.GetRequiredService<AnlageService>();
        
        _customersView = CollectionViewSource.GetDefaultView(_filteredCustomers);
        CustomersGrid.ItemsSource = _customersView;
        
        Loaded += CustomersPage_Loaded;
    }

    private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            RefreshCustomersButton.IsEnabled = false;
            RefreshCustomersButton.Content = "LADE...";

            var customersFromDb = await _databaseService.GetAllKundenAsync();
            
            _customers.Clear();
            _filteredCustomers.Clear();
            
            foreach (var customer in customersFromDb)
            {
                customer.HasAnlage = await _anlageService.CustomerHasAnlageAsync(customer.Kundennummer);
                _customers.Add(customer);
                _filteredCustomers.Add(customer);
            }

            UpdateCustomerCount();
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Laden der Kunden", ex.Message);
        }
        finally
        {
            RefreshCustomersButton.IsEnabled = true;
            RefreshCustomersButton.Content = "AKTUALISIEREN";
        }
    }

    private void UpdateCustomerCount()
    {
        CustomerCountText.Text = $"({_filteredCustomers.Count} Kunden)";
    }

    private void UpdateEmptyState()
    {
        bool hasCustomers = _filteredCustomers.Any();
        CustomersGrid.Visibility = hasCustomers ? Visibility.Visible : Visibility.Collapsed;
        EmptyStatePanel.Visibility = hasCustomers ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new CustomerEditDialog(_databaseService);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.Customer != null)
            {
                dialog.Customer.HasAnlage = await _anlageService.CustomerHasAnlageAsync(dialog.Customer.Kundennummer);
                _customers.Add(dialog.Customer);
                ApplySearch();
                ShowSuccessMessage("Kunde erfolgreich hinzugefügt!");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Hinzufügen des Kunden", ex.Message);
        }
    }

    private async void EditCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var kunde = GetSelectedCustomerFromButton(sender);
        if (kunde == null) return;

        try
        {
            var dialog = new CustomerEditDialog(_databaseService, kunde);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                await RefreshCustomerInCollection(kunde.Kundennummer);
                ShowSuccessMessage("Kunde erfolgreich aktualisiert!");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Bearbeiten des Kunden", ex.Message);
        }
    }

    private async void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var kunde = GetSelectedCustomerFromButton(sender);
        if (kunde == null) return;

        try
        {
            var result = MessageBox.Show(
                $"Sind Sie sicher, dass Sie den Kunden '{kunde.Vorname} {kunde.Nachname}' löschen möchten?\n\nDiese Aktion kann nicht rückgängig gemacht werden.",
                "Kunde löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                await _databaseService.DeleteKundeAsync(kunde.Kundennummer);
                
                _customers.Remove(kunde);
                _filteredCustomers.Remove(kunde);
                
                UpdateCustomerCount();
                UpdateEmptyState();
                
                ShowSuccessMessage("Kunde erfolgreich gelöscht!");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Löschen des Kunden", ex.Message);
        }
    }

    private void ViewCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var kunde = GetSelectedCustomerFromButton(sender);
        if (kunde == null) return;

        try
        {
            var detailsWindow = CreateCustomerDetailsWindow(kunde);
            detailsWindow.Owner = Window.GetWindow(this);
            detailsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Anzeigen des Kunden", ex.Message);
        }
    }

    private async void ViewAnlageButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(sender is Button clickedButton)) return;
        var kunde = clickedButton.Tag as Kunde;

        if (kunde == null || !kunde.HasAnlage) return;

        var originalContent = clickedButton.Content;
        clickedButton.Content = "Lade...";
        clickedButton.IsEnabled = false;

        try
        {
            var anlage = await _anlageService.GetAnlageByKundennummerAsync(kunde.Kundennummer);
            if (anlage == null)
            {
                ShowErrorMessage("Fehler", "Anlage für diesen Kunden nicht gefunden oder Kunde hat keine Module.");
                return;
            }

            var detailDialog = new AnlageDetailDialog(anlage, _anlageService);
            detailDialog.Owner = Window.GetWindow(this);
            detailDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Fehler beim Anzeigen der Anlage", ex.Message);
        }
        finally
        {
            clickedButton.Content = originalContent;
            clickedButton.IsEnabled = true;
        }
    }

    private async void RefreshCustomersButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadCustomersAsync();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearch();
    }

    private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Clear();
        ApplySearch();
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Handle selection changes if needed
    }

    private void ApplySearch()
    {
        var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;
        
        _filteredCustomers.Clear();
        
        foreach (var customer in _customers)
        {
            if (string.IsNullOrEmpty(searchText) ||
                customer.Vorname.ToLower().Contains(searchText) ||
                customer.Nachname.ToLower().Contains(searchText) ||
                customer.Email.ToLower().Contains(searchText) ||
                customer.Ort.ToLower().Contains(searchText))
            {
                _filteredCustomers.Add(customer);
            }
        }
        
        UpdateCustomerCount();
        UpdateEmptyState();
    }

    private async Task RefreshCustomerInCollection(int kundennummer)
    {
        var updatedCustomer = await _databaseService.GetKundeByIdAsync(kundennummer);
        if (updatedCustomer == null) return;

        updatedCustomer.HasAnlage = await _anlageService.CustomerHasAnlageAsync(kundennummer);

        var existingCustomerInMainList = _customers.FirstOrDefault(k => k.Kundennummer == kundennummer);
        if (existingCustomerInMainList != null)
        {
            var index = _customers.IndexOf(existingCustomerInMainList);
            _customers[index] = updatedCustomer;
        }
        
        var existingCustomerInFilteredList = _filteredCustomers.FirstOrDefault(k => k.Kundennummer == kundennummer);
        if (existingCustomerInFilteredList != null)
        {
             var filteredIndex = _filteredCustomers.IndexOf(existingCustomerInFilteredList);
            _filteredCustomers[filteredIndex] = updatedCustomer;
        }
        
        _customersView.Refresh();
        UpdateCustomerCount();
        UpdateEmptyState();
    }

    private Kunde? GetSelectedCustomerFromButton(object sender)
    {
        if (sender is Button button && button.Tag is Kunde kundeFromTag)
            return kundeFromTag;
        
        if (CustomersGrid.SelectedItem is Kunde selectedKunde)
            return selectedKunde;
        
        return null;
    }

    private Window CreateCustomerDetailsWindow(Kunde kunde)
    {
        var window = new Window
        {
            Title = "Kundendetails",
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 580,
            MaxWidth = 700, 
            MaxHeight = 750, 
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = (System.Windows.Media.Brush)FindResource("MaterialDesignPaper"),
            FontFamily = (System.Windows.Media.FontFamily)FindResource("MaterialDesignFont"),
            ResizeMode = ResizeMode.NoResize
        };

        // Main container
        var mainGrid = new Grid { Margin = new Thickness(0) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // Header card
        var headerCard = new MaterialDesignThemes.Wpf.Card
        {
            Padding = new Thickness(24, 20, 24, 20),
            Margin = new Thickness(16, 16, 16, 8),
            Background = (System.Windows.Media.Brush)FindResource("PrimaryHueMidBrush"),
            UniformCornerRadius = 8,
            Foreground = System.Windows.Media.Brushes.White
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var personIcon = new MaterialDesignThemes.Wpf.PackIcon
        {
            Kind = MaterialDesignThemes.Wpf.PackIconKind.Account,
            Width = 36,
            Height = 36,
            Foreground = System.Windows.Media.Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(personIcon, 0);
        headerGrid.Children.Add(personIcon);

        var headerPanel = new StackPanel { Margin = new Thickness(16, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        var nameTextBlock = new TextBlock
        {
            Text = $"{kunde.Vorname} {kunde.Nachname}",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
        };
        var idTextBlock = new TextBlock
        {
            Text = $"Kunden-ID: {kunde.Kundennummer}",
            FontSize = 13,
            Opacity = 0.85,
            Margin = new Thickness(0, 2, 0, 0)
        };
        headerPanel.Children.Add(nameTextBlock);
        headerPanel.Children.Add(idTextBlock);
        Grid.SetColumn(headerPanel, 1);
        headerGrid.Children.Add(headerPanel);

        headerCard.Content = headerGrid;
        Grid.SetRow(headerCard, 0);
        mainGrid.Children.Add(headerCard);

        // Details ScrollViewer and Card
        var detailsScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(16, 0, 16, 8)
        };

        var detailsCard = new MaterialDesignThemes.Wpf.Card
        {
            Padding = new Thickness(24),
            UniformCornerRadius = 8
        };

        var detailsPanel = new StackPanel();

        AddSectionHeader(detailsPanel, "Kontaktinformationen");
        AddDetailItem(detailsPanel, MaterialDesignThemes.Wpf.PackIconKind.EmailOutline, "E-Mail", kunde.Email);
        AddDetailItem(detailsPanel, MaterialDesignThemes.Wpf.PackIconKind.PhoneOutline, "Telefon", kunde.Telefonnummer);

        AddSectionHeader(detailsPanel, "Adresse");
        AddDetailItem(detailsPanel, MaterialDesignThemes.Wpf.PackIconKind.MapMarkerOutline, "Adresse", 
            $"{kunde.Strasse} {kunde.Hausnummer}\n{kunde.Postleitzahl} {kunde.Ort}");

        AddSectionHeader(detailsPanel, "Account-Informationen");
        AddDetailItem(detailsPanel, MaterialDesignThemes.Wpf.PackIconKind.AccountCircleOutline, "Benutzername", kunde.User?.Username ?? "N/A");
        AddDetailItem(detailsPanel, MaterialDesignThemes.Wpf.PackIconKind.SolarPanel, "Anlage vorhanden", kunde.HasAnlage ? "Ja" : "Nein");

        detailsCard.Content = detailsPanel;
        detailsScrollViewer.Content = detailsCard;
        Grid.SetRow(detailsScrollViewer, 1);
        mainGrid.Children.Add(detailsScrollViewer);

        // Buttons Panel (outside cards, at the bottom of the mainGrid)
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(16, 8, 16, 16)
        };

        if (kunde.HasAnlage)
        {
            var viewAnlageButton = new Button
            {
                Content = "ANLAGE ANZEIGEN",
                Style = (Style)FindResource("MaterialDesignOutlinedButton"),
                Margin = new Thickness(0, 0, 12, 0),
                BorderBrush = (System.Windows.Media.Brush)FindResource("PrimaryHueMidBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("PrimaryHueMidBrush")
            };
            viewAnlageButton.Click += (s, e) => 
            {
                window.Close();
                var anlageButtonTrigger = new Button { Tag = kunde };
                ViewAnlageButton_Click(anlageButtonTrigger, new RoutedEventArgs());
            };
            buttonPanel.Children.Add(viewAnlageButton);
        }

        var closeButton = new Button
        {
            Content = "SCHLIESSEN",
            Style = (Style)FindResource("MaterialDesignRaisedButton")
        };
        closeButton.Click += (s, e) => window.Close();
        buttonPanel.Children.Add(closeButton);
        Grid.SetRow(buttonPanel, 2);
        mainGrid.Children.Add(buttonPanel);

        window.Content = mainGrid;
        return window;
    }

    private void AddSectionHeader(StackPanel parent, string title)
    {
        var headerTextBlock = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = (System.Windows.Media.Brush)FindResource("PrimaryHueMidBrush"),
            Margin = new Thickness(0, parent.Children.Count > 0 ? 20 : 0, 0, 10)
        };
        parent.Children.Add(headerTextBlock);
    }

    private void AddDetailItem(StackPanel parent, MaterialDesignThemes.Wpf.PackIconKind iconKind, string label, string value)
    {
        var itemGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var icon = new MaterialDesignThemes.Wpf.PackIcon
        {
            Kind = iconKind,
            Width = 18,
            Height = 18,
            Foreground = (System.Windows.Media.Brush)FindResource("MaterialDesignBodyLight"),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 3, 0, 0)
        };
        Grid.SetColumn(icon, 0);
        itemGrid.Children.Add(icon);

        var textPanel = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
        
        var labelTextBlock = new TextBlock
        {
            Text = label,
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            Foreground = (System.Windows.Media.Brush)FindResource("MaterialDesignBodyLight"),
            Margin = new Thickness(0, 0, 0, 1)
        };
        textPanel.Children.Add(labelTextBlock);

        var valueTextBlock = new TextBlock
        {
            Text = value,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (System.Windows.Media.Brush)FindResource("MaterialDesignBody"),
            LineHeight = 18
        };
        textPanel.Children.Add(valueTextBlock);

        Grid.SetColumn(textPanel, 1);
        itemGrid.Children.Add(textPanel);

        parent.Children.Add(itemGrid);
    }

    private void ShowErrorMessage(string title, string message)
    {
        MessageBox.Show($"{title}\n\n{message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowSuccessMessage(string message)
    {
        MessageBox.Show(message, "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
    }
} 