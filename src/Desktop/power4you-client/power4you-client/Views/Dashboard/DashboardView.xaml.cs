using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace power4you_client.Views
{
    public partial class DashboardView : UserControl
    {
        public ObservableCollection<StatCard> StatCards { get; set; }
        public PlotModel PlotModel { get; private set; }

        public DashboardView()
        {
            InitializeComponent();
            InitializeStatCards();
            InitializePlotModel();
            DataContext = this;
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
    }

    public class StatCard
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public Brush SubtitleColor { get; set; }
    }
} 