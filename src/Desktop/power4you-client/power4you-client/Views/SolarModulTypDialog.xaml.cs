using System;
using System.Windows;
using power4you_client.Models;

namespace power4you_client.Views
{
    public partial class SolarModulTypDialog : Window
    {
        public SolarModulTyp SolarModulTyp { get; private set; }

        public SolarModulTypDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                SolarModulTyp = new SolarModulTyp
                {
                    Bezeichnung = BezeichnungTextBox.Text.Trim(),
                    Umpp = float.Parse(UmppTextBox.Text),
                    Impp = float.Parse(ImppTextBox.Text),
                    Pmpp = float.Parse(PmppTextBox.Text)
                };

                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(BezeichnungTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie eine Bezeichnung ein.", "Validierungsfehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!ValidateFloat(UmppTextBox.Text, "Umpp") ||
                !ValidateFloat(ImppTextBox.Text, "Impp") ||
                !ValidateFloat(PmppTextBox.Text, "Pmpp"))
            {
                return false;
            }

            return true;
        }

        private bool ValidateFloat(string value, string fieldName)
        {
            if (!float.TryParse(value, out float result))
            {
                MessageBox.Show($"Bitte geben Sie einen gültigen Wert für {fieldName} ein.",
                    "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}