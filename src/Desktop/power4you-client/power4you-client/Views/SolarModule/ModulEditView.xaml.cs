using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using power4you_client.Models;
using power4you_client.Services;

namespace power4you_client.Views.SolarModule
{
    public partial class ModulEditView : UserControl
    {
        private SolarModulTyp _currentModul;

        public ModulEditView(SolarModulTyp modul = null)
        {
            InitializeComponent();
            _currentModul = modul;

            // Start form animation
            var formFadeIn = (Storyboard)FindResource("FormFadeIn");
            formFadeIn.Begin(this);

            if (modul != null)
            {
                // Edit mode
                HeaderText.Text = "Modul bearbeiten";
                LoadModulData(modul);
            }
        }

        private void LoadModulData(SolarModulTyp modul)
        {
            BezeichnungTextBox.Text = modul.Bezeichnung;
            PmppTextBox.Text = modul.Pmpp.ToString();
            UmppTextBox.Text = modul.Umpp.ToString();
            ImppTextBox.Text = modul.Impp.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var modul = _currentModul ?? new SolarModulTyp();
            modul.Bezeichnung = BezeichnungTextBox.Text;
            modul.Pmpp = float.Parse(PmppTextBox.Text);
            modul.Umpp = float.Parse(UmppTextBox.Text);
            modul.Impp = float.Parse(ImppTextBox.Text);

            if (_currentModul == null)
            {
                SolarModulService.AddModulTyp(modul);
            }
            else
            {
                SolarModulService.UpdateModulTyp(modul);
            }

            // Navigate back to ModuleView
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = new ModuleView();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(BezeichnungTextBox.Text))
            {
                ShowValidationError("Bitte geben Sie eine Bezeichnung ein.");
                return false;
            }

            if (!float.TryParse(PmppTextBox.Text, out float pmpp) || pmpp <= 0)
            {
                ShowValidationError("Bitte geben Sie eine gültige Peak-Leistung ein.");
                return false;
            }

            if (!float.TryParse(UmppTextBox.Text, out float umpp) || umpp <= 0)
            {
                ShowValidationError("Bitte geben Sie eine gültige Spannung ein.");
                return false;
            }

            if (!float.TryParse(ImppTextBox.Text, out float impp) || impp <= 0)
            {
                ShowValidationError("Bitte geben Sie einen gültigen Strom ein.");
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to ModuleView
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                mainFrame.Content = new ModuleView();
            }
        }
    }
} 