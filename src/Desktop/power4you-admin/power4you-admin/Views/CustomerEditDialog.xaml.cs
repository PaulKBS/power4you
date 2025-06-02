using System.Windows;
using power4you_admin.Models;
using power4you_admin.Services;

namespace power4you_admin.Views;

public partial class CustomerEditDialog : Window
{
    private readonly DatabaseService _databaseService;
    private Kunde? _kunde;
    private bool _isEditMode;

    public Kunde? Customer => _kunde;

    public CustomerEditDialog(DatabaseService databaseService, Kunde? kunde = null)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _kunde = kunde;
        _isEditMode = kunde != null;

        LoadCustomerData();
        ConfigureForMode();
    }

    private void LoadCustomerData()
    {
        if (_kunde != null)
        {
            UserIdTextBox.Text = _kunde.UserId.ToString();
            UsernameTextBox.Text = _kunde.User?.Username ?? "";
            VornameTextBox.Text = _kunde.Vorname;
            NachnameTextBox.Text = _kunde.Nachname;
            StrasseTextBox.Text = _kunde.Strasse;
            HausnummerTextBox.Text = _kunde.Hausnummer;
            PostleitzahlTextBox.Text = _kunde.Postleitzahl;
            OrtTextBox.Text = _kunde.Ort;
            EmailTextBox.Text = _kunde.Email;
            TelefonnummerTextBox.Text = _kunde.Telefonnummer;
        }
    }

    private void ConfigureForMode()
    {
        if (_isEditMode)
        {
            Title = "Kunde bearbeiten";
            UserIdTextBox.Visibility = Visibility.Visible;
            UsernameTextBox.IsReadOnly = true;
            PasswordBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            Title = "Neuen Kunden hinzufügen";
            UserIdTextBox.Visibility = Visibility.Collapsed;
            UsernameTextBox.IsReadOnly = false;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidateInput())
                return;

            SaveButton.IsEnabled = false;
            SaveButton.Content = "SPEICHERE...";

            if (_isEditMode)
            {
                await UpdateCustomerAsync();
            }
            else
            {
                await CreateCustomerAsync();
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Speichern des Kunden: {ex.Message}", 
                            "Speicherfehler", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Content = "SPEICHERN";
        }
    }

    private bool ValidateInput()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(VornameTextBox.Text))
            errors.Add("Vorname ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(NachnameTextBox.Text))
            errors.Add("Nachname ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(StrasseTextBox.Text))
            errors.Add("Straße ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(HausnummerTextBox.Text))
            errors.Add("Hausnummer ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(PostleitzahlTextBox.Text))
            errors.Add("Postleitzahl ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(OrtTextBox.Text))
            errors.Add("Stadt ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            errors.Add("E-Mail ist ein Pflichtfeld.");

        if (string.IsNullOrWhiteSpace(TelefonnummerTextBox.Text))
            errors.Add("Telefonnummer ist ein Pflichtfeld.");

        if (!_isEditMode)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                errors.Add("Benutzername ist ein Pflichtfeld.");

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                errors.Add("Passwort ist ein Pflichtfeld.");
        }

        if (errors.Any())
        {
            MessageBox.Show(string.Join("\n", errors), 
                            "Validierungsfehler", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private async Task UpdateCustomerAsync()
    {
        if (_kunde == null) return;

        _kunde.Vorname = VornameTextBox.Text.Trim();
        _kunde.Nachname = NachnameTextBox.Text.Trim();
        _kunde.Strasse = StrasseTextBox.Text.Trim();
        _kunde.Hausnummer = HausnummerTextBox.Text.Trim();
        _kunde.Postleitzahl = PostleitzahlTextBox.Text.Trim();
        _kunde.Ort = OrtTextBox.Text.Trim();
        _kunde.Email = EmailTextBox.Text.Trim();
        _kunde.Telefonnummer = TelefonnummerTextBox.Text.Trim();

        await _databaseService.UpdateKundeAsync(_kunde);
    }

    private async Task CreateCustomerAsync()
    {
        // First create the user
        var user = new User
        {
            Username = UsernameTextBox.Text.Trim(),
            Password = PasswordBox.Password, // In real app, this should be hashed
            ApiKey = Guid.NewGuid().ToString()
        };

        var createdUser = await _databaseService.CreateUserAsync(user);

        // Then create the customer
        _kunde = new Kunde
        {
            UserId = createdUser.UserId,
            Vorname = VornameTextBox.Text.Trim(),
            Nachname = NachnameTextBox.Text.Trim(),
            Strasse = StrasseTextBox.Text.Trim(),
            Hausnummer = HausnummerTextBox.Text.Trim(),
            Postleitzahl = PostleitzahlTextBox.Text.Trim(),
            Ort = OrtTextBox.Text.Trim(),
            Email = EmailTextBox.Text.Trim(),
            Telefonnummer = TelefonnummerTextBox.Text.Trim()
        };

        await _databaseService.CreateKundeAsync(_kunde);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
} 