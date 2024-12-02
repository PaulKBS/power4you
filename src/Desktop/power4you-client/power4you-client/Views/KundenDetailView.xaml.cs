using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace power4you_client.Views
{
    public partial class KundenDetailView : UserControl
    {
        public KundenDetailView()
        {
            InitializeComponent();
        }

        public void LoadCustomerDetails(Kunde kunde)
        {
            VornameTextBox.Text = kunde.Vorname;
            NachnameTextBox.Text = kunde.Nachname;
            EmailTextBox.Text = kunde.Email;
            TelefonTextBox.Text = kunde.Telefon;
            StrasseTextBox.Text = kunde.Strasse;
            HausnummerTextBox.Text = kunde.Hausnummer;
            PlzTextBox.Text = kunde.PLZ;
            OrtTextBox.Text = kunde.Ort;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.MainFrame.Navigate(new KundenUebersichtView());
        }
    }

    public class Kunde
    {
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Strasse { get; set; }
        public string Hausnummer { get; set; }
        public string PLZ { get; set; }
        public string Ort { get; set; }
    }
}
