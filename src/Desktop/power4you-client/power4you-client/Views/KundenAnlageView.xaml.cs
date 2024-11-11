using System.Windows;
using System.Windows.Controls;

namespace power4you_client.Views
{
    public partial class KundenAnlageView : UserControl
    {
        public KundenAnlageView()
        {
            InitializeComponent();
        }

        private void AbbrechenButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all form fields
            VornameTextBox.Clear();
            NachnameTextBox.Clear();
            EmailTextBox.Clear();
            TelefonTextBox.Clear();
            StrasseTextBox.Clear();
            HausnummerTextBox.Clear();
            PlzTextBox.Clear();
            OrtTextBox.Clear();
        }

        private void KundeAnlegenButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement customer creation logic
            MessageBox.Show("Kunde wurde erfolgreich angelegt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Clear the form after successful creation
            AbbrechenButton_Click(sender, e);
        }
    }
} 