using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace power4you_client.Views
{
    public partial class DashboardView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<StatCard> StatCards { get; set; }
        public ObservableCollection<WeatherData> WeatherForecast { get; set; }
        public PlotModel PlotModel { get; private set; }

        public DashboardView()
        {
            InitializeComponent();
            InitializeStatCards();
            InitializePlotModel();
            DataContext = this;
            FetchWeatherData();
        }

        private void InitializePlotModel()
        {
            PlotModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColor.FromRgb(200, 200, 200),
                TextColor = OxyColor.FromRgb(100, 100, 100),
                PlotMargins = new OxyThickness(60, 20, 20, 40),
                DefaultFont = "Segoe UI",
                IsLegendVisible = false,
                PlotAreaBorderThickness = new OxyThickness(1)
            };

            // Configure Y axis (Power)
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Leistung (kW)",
                TitleFontSize = 14,
                FontSize = 12,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(200, 200, 200),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(245, 245, 245),
                Minimum = 0,
                Maximum = 300,
                MajorStep = 50,
                MinorStep = 25
            });

            // Configure X axis (Time)
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Uhrzeit",
                TitleFontSize = 14,
                FontSize = 12,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(200, 200, 200),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(245, 245, 245),
                Minimum = 0,
                Maximum = 24,
                MajorStep = 3,
                MinorStep = 1,
                MajorGridlineThickness = 1,
                MinorGridlineThickness = 0.5,
                LabelFormatter = value => $"{(int)value:00}:00"
            });

            // Create series
            var areaSeries = new AreaSeries
            {
                Color = OxyColor.FromAColor(120, OxyColor.Parse("#2196F3")),
                Color2 = OxyColor.FromAColor(0, OxyColor.Parse("#2196F3")),
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
                InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                TrackerFormatString = "Zeit: {2:00}:00\nLeistung: {4:0.00} kW"
            };

            // Mock data points for a full day
            var mockData = new[]
            {
                // Night (00:00 - 06:00)
                new DataPoint(0, 0),    // 00:00
                new DataPoint(3, 0),    // 03:00
                new DataPoint(6, 15),   // 06:00
                
                // Morning (09:00 - 12:00)
                new DataPoint(9, 155),  // 09:00
                new DataPoint(12, 265), // 12:00
                
                // Afternoon (15:00 - 18:00)
                new DataPoint(15, 215), // 15:00
                new DataPoint(18, 65),  // 18:00
                
                // Evening/Night (21:00 - 24:00)
                new DataPoint(21, 0),   // 21:00
                new DataPoint(24, 0)    // 24:00
            };

            // Add the data points
            foreach (var point in mockData)
            {
                areaSeries.Points.Add(point);
                areaSeries.Points2.Add(new DataPoint(point.X, 0));
            }

            PlotModel.Series.Add(areaSeries);
        }

        private void InitializeStatCards()
        {
            StatCards = new ObservableCollection<StatCard>
            {
                new StatCard
                {
                    Title = "Anlagen Online",
                    Value = "42/45",
                    Subtitle = "93.3% Verfügbarkeit",
                    SubtitleColor = new SolidColorBrush(Color.FromRgb(76, 175, 80))
                },
                new StatCard
                {
                    Title = "Aktuelle Leistung",
                    Value = "284,5 kW",
                    Subtitle = "↑ 12% zum Vortag",
                    SubtitleColor = new SolidColorBrush(Color.FromRgb(76, 175, 80))
                },
                new StatCard
                {
                    Title = "Tagesproduktion",
                    Value = "1.842 kWh",
                    Subtitle = "Prognose: 2.450 kWh",
                    SubtitleColor = new SolidColorBrush(Color.FromRgb(158, 158, 158))
                },
                new StatCard
                {
                    Title = "CO₂ Einsparung (Heute)",
                    Value = "875 kg",
                    Subtitle = "≈ 5.400 km Autofahrt",
                    SubtitleColor = new SolidColorBrush(Color.FromRgb(158, 158, 158))
                },
                new StatCard
                {
                    Title = "Gesamtleistung",
                    Value = "12,5 MW",
                    Subtitle = "Installierte Leistung",
                    SubtitleColor = new SolidColorBrush(Color.FromRgb(158, 158, 158))
                }
            };
        }

        private async void FetchWeatherData()
        {
            WeatherForecast = new ObservableCollection<WeatherData>();
            string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=52.4265&longitude=7.0686&hourly=temperature_2m,weathercode";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject weatherJson = JObject.Parse(jsonResponse);
                    var hourlyTemperatures = weatherJson["hourly"]["temperature_2m"].Take(3).ToList();
                    var hourlyWeatherCodes = weatherJson["hourly"]["weathercode"].Take(3).ToList();

                    for (int i = 0; i < hourlyTemperatures.Count; i++)
                    {
                        string day = DateTime.Now.AddHours(i).ToString("dddd");
                        string temperature = hourlyTemperatures[i].ToString();
                        string weatherCode = hourlyWeatherCodes[i].ToString();
                        string description = GetWeatherDescription(weatherCode);
                        string icon = $"http://openweathermap.org/img/wn/{GetWeatherIcon(weatherCode)}.png"; // Placeholder icon

                        WeatherForecast.Add(new WeatherData
                        {
                            Day = day,
                            Temperature = $"{temperature}°C",
                            Description = description,
                            Icon = icon
                        });
                    }
                }
            }

            OnPropertyChanged(nameof(WeatherForecast));
        }

        private string GetWeatherDescription(string weatherCode)
        {
            switch (weatherCode)
            {
                case "0": return "Klarer Himmel";
                case "1": return "Überwiegend klar";
                case "2": return "Teilweise bewölkt";
                case "3": return "Bedeckt";
                case "45": return "Nebel";
                case "48": return "Ablagernder Reifnebel";
                case "51": return "Nieselregen: Leicht";
                case "53": return "Nieselregen: Mäßig";
                case "55": return "Nieselregen: Dichte Intensität";
                case "61": return "Regen: Leicht";
                case "63": return "Regen: Mäßig";
                case "65": return "Regen: Starke Intensität";
                case "71": return "Schneefall: Leicht";
                case "73": return "Schneefall: Mäßig";
                case "75": return "Schneefall: Starke Intensität";
                case "80": return "Regenschauer: Leicht";
                case "81": return "Regenschauer: Mäßig";
                case "82": return "Regenschauer: Heftig";
                case "95": return "Gewitter: Leicht oder mäßig";
                case "96": return "Gewitter: Mit leichtem Hagel";
                case "99": return "Gewitter: Mit starkem Hagel";
                default: return "Unbekannt";
            }
        }

        private string GetWeatherIcon(string weatherCode)
        {
            switch (weatherCode)
            {
                case "0": return "01d"; // Clear sky
                case "1": return "01d"; // Mainly clear
                case "2": return "02d"; // Partly cloudy
                case "3": return "03d"; // Overcast
                case "45": return "50d"; // Fog
                case "48": return "50d"; // Depositing rime fog
                case "51": return "09d"; // Drizzle: Light
                case "53": return "09d"; // Drizzle: Moderate
                case "55": return "09d"; // Drizzle: Dense intensity
                case "61": return "10d"; // Rain: Slight
                case "63": return "10d"; // Rain: Moderate
                case "65": return "10d"; // Rain: Heavy intensity
                case "71": return "13d"; // Snow fall: Slight
                case "73": return "13d"; // Snow fall: Moderate
                case "75": return "13d"; // Snow fall: Heavy intensity
                case "80": return "09d"; // Rain showers: Slight
                case "81": return "09d"; // Rain showers: Moderate
                case "82": return "09d"; // Rain showers: Violent
                case "95": return "11d"; // Thunderstorm: Slight or moderate
                case "96": return "11d"; // Thunderstorm: With slight hail
                case "99": return "11d"; // Thunderstorm: With heavy hail
                default: return "01d"; // Default icon
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class StatCard
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public Brush SubtitleColor { get; set; }
    }

    public class WeatherData
    {
        public string Day { get; set; }
        public string Temperature { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}