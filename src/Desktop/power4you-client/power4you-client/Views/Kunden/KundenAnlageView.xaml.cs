using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using power4you_client.Models.Kunden;
using power4you_client.Services;

namespace power4you_client.Views
{
    public partial class KundenAnlageView : UserControl
    {
        private readonly Regex emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private readonly Regex phoneRegex = new Regex(@"^\d{6,15}$");
        private readonly Regex plzRegex = new Regex(@"^\d{5}$");
        private readonly Kunde _kundeToEdit;

        private readonly SolidColorBrush defaultBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        private readonly SolidColorBrush errorBorderBrush = new SolidColorBrush(Colors.Red);

        public KundenAnlageView(Kunde kundeToEdit = null)
        {
            InitializeComponent();
            Opacity = 0;
            
            // Start the animation
            var storyboard = (Storyboard)FindResource("FormFadeIn");
            storyboard.Begin(this);
            
            _kundeToEdit = kundeToEdit;

            if (_kundeToEdit != null)
            {
                // Fill the form with customer data
                VornameTextBox.Text = _kundeToEdit.Vorname;
                NachnameTextBox.Text = _kundeToEdit.Nachname;
                EmailTextBox.Text = _kundeToEdit.Email;
                TelefonTextBox.Text = _kundeToEdit.Telefon?.Replace("+49 ", "");
                StrasseTextBox.Text = _kundeToEdit.Strasse;
                HausnummerTextBox.Text = _kundeToEdit.Hausnummer;
                PlzTextBox.Text = _kundeToEdit.PLZ;
                OrtTextBox.Text = _kundeToEdit.Ort;

                // Update UI elements for edit mode
                HeaderText.Text = "Kunde bearbeiten";
                KundeAnlegenButton.Content = "Speichern";
            }
            else
            {
                HeaderText.Text = "Neuer Kunde";
                KundeAnlegenButton.Content = "Kunde anlegen";
            }
        }

        private void AbbrechenButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to KundenView
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = new KundenView();
            }
        }

        private void KundeAnlegenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var kunde = _kundeToEdit ?? new Kunde();
            
            // Update customer data
            kunde.Vorname = VornameTextBox.Text.Trim();
            kunde.Nachname = NachnameTextBox.Text.Trim();
            kunde.Email = EmailTextBox.Text.Trim();
            kunde.Telefon = $"{VorwahlComboBox.Text} {TelefonTextBox.Text.Trim()}";
            kunde.Strasse = StrasseTextBox.Text.Trim();
            kunde.Hausnummer = HausnummerTextBox.Text.Trim();
            kunde.PLZ = PlzTextBox.Text.Trim();
            kunde.Ort = OrtTextBox.Text.Trim();

            if (_kundeToEdit == null)
            {
                kunde.ErstelltAm = DateTime.Now;
                KundenService.AddKunde(kunde);
                MessageBox.Show("Kunde wurde erfolgreich angelegt.", "Erfolg", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                KundenService.UpdateKunde(kunde);
                MessageBox.Show("Kunde wurde erfolgreich aktualisiert.", "Erfolg", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Navigate back to KundenView
            AbbrechenButton_Click(sender, e);
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            isValid &= ValidateRequired(VornameTextBox, "Vorname");
            isValid &= ValidateRequired(NachnameTextBox, "Nachname");
            isValid &= ValidateEmail();
            isValid &= ValidatePhone();
            isValid &= ValidateRequired(StrasseTextBox, "Straße");
            isValid &= ValidateRequired(HausnummerTextBox, "Hausnummer");
            isValid &= ValidatePLZ();
            isValid &= ValidateRequired(OrtTextBox, "Ort");

            if (!isValid)
            {
                MessageBox.Show("Bitte füllen Sie alle Pflichtfelder korrekt aus.", 
                    "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateEmail();
        }

        private void TelefonTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidatePhone();
        }

        private bool ValidateEmail()
        {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                ShowError(EmailTextBox, EmailErrorText, "E-Mail-Adresse ist erforderlich.");
                return false;
            }

            if (!emailRegex.IsMatch(email))
            {
                ShowError(EmailTextBox, EmailErrorText, "Bitte geben Sie eine gültige E-Mail-Adresse ein.");
                return false;
            }

            HideError(EmailTextBox, EmailErrorText);
            return true;
        }

        private bool ValidatePhone()
        {
            string phone = TelefonTextBox.Text.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                ShowError(TelefonTextBox, TelefonErrorText, "Telefonnummer ist erforderlich.");
                return false;
            }

            string numericPhone = Regex.Replace(phone, @"[^\d]", "");

            if (!phoneRegex.IsMatch(numericPhone))
            {
                ShowError(TelefonTextBox, TelefonErrorText, "Bitte geben Sie eine gültige Telefonnummer ein (6-15 Ziffern).");
                return false;
            }

            HideError(TelefonTextBox, TelefonErrorText);
            return true;
        }

        private bool ValidateRequired(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = errorBorderBrush;
                MessageBox.Show($"{fieldName} ist ein Pflichtfeld.", "Validierungsfehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            textBox.BorderBrush = defaultBorderBrush;
            return true;
        }

        private bool ValidatePLZ()
        {
            string plz = PlzTextBox.Text.Trim();

            if (string.IsNullOrEmpty(plz))
            {
                PlzTextBox.BorderBrush = errorBorderBrush;
                MessageBox.Show("PLZ ist ein Pflichtfeld.", "Validierungsfehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!plzRegex.IsMatch(plz))
            {
                PlzTextBox.BorderBrush = errorBorderBrush;
                MessageBox.Show("Bitte geben Sie eine gültige 5-stellige PLZ ein.", "Validierungsfehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            PlzTextBox.BorderBrush = defaultBorderBrush;
            return true;
        }

        private void ShowError(TextBox textBox, TextBlock errorText, string message)
        {
            textBox.BorderBrush = errorBorderBrush;
            errorText.Text = message;
            errorText.Visibility = Visibility.Visible;
        }

        private void HideError(TextBox textBox, TextBlock errorText)
        {
            textBox.BorderBrush = defaultBorderBrush;
            errorText.Visibility = Visibility.Collapsed;
        }
    }
}