using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using power4you_admin.Services;
using power4you_admin.Models;
using power4you_admin.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace power4you_admin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private CustomersPage? _customersPage;
        private AnlagenPage? _anlagenPage;
        private bool _isDarkMode = false;

        public ObservableCollection<StatCardViewModel> StatCards { get; set; }
        public ObservableCollection<TopAnlageViewModel> TopAnlagen { get; set; }
        public ObservableCollection<WeatherForecastViewModel> WeatherForecasts { get; set; }
        
        private int _averageEfficiency;
        public int AverageEfficiency
        {
            get => _averageEfficiency;
            set { _averageEfficiency = value; OnPropertyChanged(nameof(AverageEfficiency)); OnPropertyChanged(nameof(AverageEfficiencyDisplay)); }
        }
        public string AverageEfficiencyDisplay => $"{AverageEfficiency}%";

        private int _networkStability;
        public int NetworkStability
        {
            get => _networkStability;
            set { _networkStability = value; OnPropertyChanged(nameof(NetworkStability)); OnPropertyChanged(nameof(NetworkStabilityDisplay)); }
        }
        public string NetworkStabilityDisplay => $"{NetworkStability}%";

        private string _chartGesamtLeistungText = "...";
        public string ChartGesamtLeistungText
        {
            get => _chartGesamtLeistungText;
            set { _chartGesamtLeistungText = value; OnPropertyChanged(nameof(ChartGesamtLeistungText)); }
        }

        private string _chartGesamtLeistungDescription = "(Live System Total)";
        public string ChartGesamtLeistungDescription
        {
            get => _chartGesamtLeistungDescription;
            set { _chartGesamtLeistungDescription = value; OnPropertyChanged(nameof(ChartGesamtLeistungDescription)); }
        }

        public MainWindow(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            
            StatCards = new ObservableCollection<StatCardViewModel>();
            TopAnlagen = new ObservableCollection<TopAnlageViewModel>();
            WeatherForecasts = new ObservableCollection<WeatherForecastViewModel>();
            
            DataContext = this;
            
            UpdateActiveNavigation(DashboardButton);
            
            InitializeTheme();
            
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                bool isConnected = await _databaseService.TestConnectionAsync();
                ConnectionStatus.Text = isConnected ? "✅ Verbunden" : "❌ Nicht verbunden";
                ConnectionIcon.Kind = isConnected ? PackIconKind.DatabaseCheck : PackIconKind.DatabaseAlert;

                if (isConnected)
                {
                    await LoadDashboardDataAsync();
                    UpdateLastUpdatedTime();
                }
                else
                {
                    MessageBox.Show("Verbindung zur Datenbank fehlgeschlagen. Bitte überprüfen Sie Ihre Verbindungseinstellungen.", 
                                    "Datenbankverbindungsfehler", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Initialisieren der Anwendung: {ex.Message}", 
                                "Initialisierungsfehler", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                ConnectionStatus.Text = "❌ Fehler";
                ConnectionIcon.Kind = PackIconKind.DatabaseAlert;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                StatCards.Clear();
                var totalCustomers = await _databaseService.GetTotalKundenCountAsync();
                StatCards.Add(new StatCardViewModel { 
                    Title = "Gesamtkunden", 
                    Value = totalCustomers.ToString(), 
                    Subtitle = "Aktive Konten", 
                    SubtitleColor = (Brush)FindResource("PrimaryHueMidBrush"), 
                    IconKind = PackIconKind.AccountGroupOutline 
                });

                var totalModules = await _databaseService.GetTotalSolarmoduleCountAsync();
                StatCards.Add(new StatCardViewModel { 
                    Title = "Solarmodule", 
                    Value = totalModules.ToString(), 
                    Subtitle = "Installierte Einheiten", 
                    SubtitleColor = (Brush)FindResource("SecondaryHueMidBrush"), 
                    IconKind = PackIconKind.SolarPanel 
                });

                var totalPower = await _databaseService.GetTotalPowerOutputAsync();
                StatCards.Add(new StatCardViewModel { 
                    Title = "Aktuelle Leistung", 
                    Value = $"{totalPower:F0} W", 
                    Subtitle = "Gesamtsystem Live", 
                    SubtitleColor = Brushes.OrangeRed, 
                    IconKind = PackIconKind.PowerPlugOutline 
                });
                
                var totalCapacity = await _databaseService.GetTotalCapacityKWpAsync();
                StatCards.Add(new StatCardViewModel { 
                    Title = "Gesamtkapazität", 
                    Value = $"{totalCapacity:F2} kWp", 
                    Subtitle = "Max. Potenzial", 
                    SubtitleColor = Brushes.DarkViolet, 
                    IconKind = PackIconKind.FlashOutline 
                });

                ChartGesamtLeistungText = $"{totalPower:F0} W";
                ChartGesamtLeistungDescription = "(Gesamtsystem Live)";

                TopAnlagen.Clear();
                var topModules = await _databaseService.GetTopPerformingModulesAsync(3); 
                if (topModules.Any())
                {
                    foreach (var module in topModules)
                    {
                        TopAnlagen.Add(new TopAnlageViewModel { 
                            Name = $"Mod #{module.ModuleNumber} (Kunde: {module.CustomerName})", 
                            Power = $"{module.PowerOutput:F0} W" 
                        });
                    }
                }
                else 
                {
                     TopAnlagen.Add(new TopAnlageViewModel { Name = "Keine Leistungsdaten verfügbar", Power = "N/V" });
                }

                AverageEfficiency = (int)await _databaseService.GetSystemEfficiencyAsync();
                NetworkStability = new Random().Next(90, 99);
                OnPropertyChanged(nameof(AverageEfficiencyDisplay));
                OnPropertyChanged(nameof(NetworkStabilityDisplay));
                OnPropertyChanged(nameof(ChartGesamtLeistungText));

                await LoadWeatherDataAsync();

                await LoadChartsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Dashboard-Daten: {ex.Message}", 
                                "Datenladefehler", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
            }
        }

        private async Task LoadChartsAsync(int days = 7)
        {
            try
            {
                var trendData = await _databaseService.GetPowerTrendDataAsync(days);
                PowerTrendChartContainer.Child = CreatePowerTrendVisualization(trendData);
                
                var moduleTypes = await _databaseService.GetModuleTypeDistributionAsync();
                if (ModuleTypesChartContainer != null)
                {
                    ModuleTypesChartContainer.Child = CreateModuleTypeVisualization(moduleTypes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Diagramme: {ex.Message}", 
                                "Diagramm-Ladefehler", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
            }
        }

        private async void TimePeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag?.ToString(), out int days))
                {
                    try
                    {
                        var loadingSpinner = CreateLoadingSpinner();
                        PowerTrendChartContainer.Child = loadingSpinner;
                        comboBox.IsEnabled = false;
                        
                        var trendData = await _databaseService.GetPowerTrendDataAsync(days);
                        PowerTrendChartContainer.Child = CreatePowerTrendVisualization(trendData);
                        
                        var totalPower = trendData.Any() ? trendData.Average(x => x.AvgPower) : 0;
                        ChartGesamtLeistungText = $"{totalPower:F0} W";
                        ChartGesamtLeistungDescription = $"(Ø Gesamt {days}T)"; // T for Tage (Days)
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Aktualisieren der Diagrammdaten: {ex.Message}", 
                                        "Diagramm-Aktualisierungsfehler", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Warning);
                    }
                    finally
                    {
                        comboBox.IsEnabled = true;
                    }
                }
            }
        }

        private UIElement CreateLoadingSpinner()
        {
            var grid = new Grid { Background = Brushes.Transparent };
            var loadingPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var spinner = new PackIcon { Kind = PackIconKind.Loading, Width = 32, Height = 32, Foreground = (Brush)FindResource("PrimaryHueMidBrush"), HorizontalAlignment = HorizontalAlignment.Center };
            var rotateTransform = new RotateTransform();
            spinner.RenderTransform = rotateTransform;
            spinner.RenderTransformOrigin = new Point(0.5, 0.5);
            var animation = new System.Windows.Media.Animation.DoubleAnimation { From = 0, To = 360, Duration = TimeSpan.FromSeconds(1), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever };
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
            var text = new TextBlock { Text = "Diagrammdaten werden geladen...", FontSize = 14, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 12, 0, 0) };
            loadingPanel.Children.Add(spinner);
            loadingPanel.Children.Add(text);
            grid.Children.Add(loadingPanel);
            return grid;
        }

        private UIElement CreatePowerTrendVisualization(List<(DateTime Date, double AvgPower)> trendData)
        {
            var canvas = new Canvas { Background = Brushes.Transparent }; 
            if (!trendData.Any() || trendData.Count < 2)
            {
                var noDataPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                var icon = new PackIcon { Kind = PackIconKind.ChartTimelineVariant, Width = 48, Height = 48, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12) };
                var text = new TextBlock { Text = trendData.Any() ? "Nicht genügend Daten für Trendanalyse." : "Keine Leistungsdaten verfügbar.", FontSize = 14, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center };
                noDataPanel.Children.Add(icon);
                noDataPanel.Children.Add(text);
                canvas.Children.Add(noDataPanel); 
                canvas.SizeChanged += (s, e) => {
                    if (noDataPanel.Parent == canvas && trendData.Count < 2) {
                        Canvas.SetLeft(noDataPanel, (e.NewSize.Width - noDataPanel.ActualWidth) / 2);
                        Canvas.SetTop(noDataPanel, (e.NewSize.Height - noDataPanel.ActualHeight) / 2);
                    }
                };
                return canvas;
            }
            var dataPoints = trendData.OrderBy(d => d.Date).ToList();
            double maxPower = dataPoints.Max(p => p.AvgPower);
            double minPower = dataPoints.Min(p => p.AvgPower);
            double range = maxPower - minPower;
            if (range < 0.1) { maxPower = minPower + 10; }
            range = maxPower - minPower;
            double padding = range * 0.15; 
            maxPower += padding;
            minPower = Math.Max(0, minPower - padding);
            if (minPower < 0) minPower = 0; 

            canvas.SizeChanged += (s, e) => 
            {
                if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0) return;
                DrawChartContent(canvas, dataPoints, maxPower, minPower, e.NewSize.Width, e.NewSize.Height);
            };
            return canvas;
        }

        private void DrawChartContent(Canvas canvas, List<(DateTime Date, double AvgPower)> dataPoints, double maxPower, double minPower, double actualWidth, double actualHeight)
        {
            canvas.Children.Clear();
            double plotMarginLeft = 65; double plotMarginTop = 30; double plotMarginRight = 30; double plotMarginBottom = 50;
            double plotWidth = actualWidth - plotMarginLeft - plotMarginRight;
            double plotHeight = actualHeight - plotMarginTop - plotMarginBottom;
            if (plotWidth <= 0 || plotHeight <= 0) return;
            var gridLinesBrush = (Brush)FindResource("MaterialDesignDivider");
            int yGridLines = 5;
            for (int i = 0; i <= yGridLines; i++)
            {
                double yPos = plotMarginTop + (i * plotHeight / yGridLines);
                var line = new Line { X1 = plotMarginLeft, Y1 = yPos, X2 = plotMarginLeft + plotWidth, Y2 = yPos, Stroke = gridLinesBrush, StrokeThickness = 0.5, StrokeDashArray = new DoubleCollection { 2, 2 } };
                canvas.Children.Add(line);
                double value = maxPower - (i * (maxPower - minPower) / yGridLines);
                var labelText = new TextBlock { Text = $"{value:F0} W", FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("MaterialDesignBody"), Padding = new Thickness(4,2,4,2)};
                var labelBorder = new Border { Child = labelText, CornerRadius = new CornerRadius(3), Background=(Brush)FindResource("MaterialDesignPaper"), Margin= new Thickness(0,0,5,0)}; 
                canvas.Children.Add(labelBorder);
                labelBorder.UpdateLayout(); 
                Canvas.SetLeft(labelBorder, plotMarginLeft - labelBorder.ActualWidth - 5); 
                Canvas.SetTop(labelBorder, yPos - (labelBorder.ActualHeight / 2));
            }
            var linePoints = new PointCollection();
            var fillPoints = new PointCollection();
            fillPoints.Add(new Point(plotMarginLeft, plotMarginTop + plotHeight)); 
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double x = plotMarginLeft + (dataPoints.Count == 1 ? plotWidth / 2 : (i * plotWidth / (dataPoints.Count - 1)));
                double normalizedY = (maxPower - minPower) == 0 ? 0.5 : (dataPoints[i].AvgPower - minPower) / (maxPower - minPower);
                double y = plotMarginTop + plotHeight - (normalizedY * plotHeight);
                linePoints.Add(new Point(x, y)); fillPoints.Add(new Point(x,y));
            }
            if (dataPoints.Any()) fillPoints.Add(new Point(plotMarginLeft + plotWidth, plotMarginTop + plotHeight)); 
            var gradientBrush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            var primaryColor = ((SolidColorBrush)FindResource("PrimaryHueMidBrush")).Color;
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(80, primaryColor.R, primaryColor.G, primaryColor.B), 0));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(10, primaryColor.R, primaryColor.G, primaryColor.B), 1));
            var fillPolygon = new Polygon { Points = fillPoints, Fill = gradientBrush };
            if (dataPoints.Count > 1) canvas.Children.Add(fillPolygon);
            var polyline = new Polyline { Points = linePoints, Stroke = (Brush)FindResource("PrimaryHueMidBrush"), StrokeThickness = 2.5, StrokeLineJoin = PenLineJoin.Round };
            canvas.Children.Add(polyline);
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double x = plotMarginLeft + (dataPoints.Count == 1 ? plotWidth / 2 : (i * plotWidth / (dataPoints.Count - 1)));
                double normalizedY = (maxPower - minPower) == 0 ? 0.5 : (dataPoints[i].AvgPower - minPower) / (maxPower - minPower);
                double y = plotMarginTop + plotHeight - (normalizedY * plotHeight);
                var outerCircle = new Ellipse { Width = 9, Height = 9, Fill = (Brush)FindResource("MaterialDesignPaper"), Stroke = (Brush)FindResource("PrimaryHueMidBrush"), StrokeThickness = 2.5 };
                Canvas.SetLeft(outerCircle, x - outerCircle.Width/2); Canvas.SetTop(outerCircle, y - outerCircle.Height/2);
                canvas.Children.Add(outerCircle);
                var valueLabel = new Border { Background = (Brush)FindResource("MaterialDesignPaper"), BorderBrush = (Brush)FindResource("PrimaryHueMidBrush"), BorderThickness = new Thickness(1.5), CornerRadius = new CornerRadius(5), Padding = new Thickness(10, 6, 10, 6), Visibility = Visibility.Collapsed, Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 7, ShadowDepth = 1.5, Opacity = 0.25, Color = Colors.Black } };
                var tooltipStack = new StackPanel();
                tooltipStack.Children.Add(new TextBlock { Text = dataPoints[i].Date.ToString("MMM dd, yyyy"), FontSize = 11, FontWeight = FontWeights.Medium, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center });
                tooltipStack.Children.Add(new TextBlock { Text = $"{dataPoints[i].AvgPower:F0} W", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("PrimaryHueMidBrush"), HorizontalAlignment = HorizontalAlignment.Center, Margin=new Thickness(0,2,0,2) });
                valueLabel.Child = tooltipStack;
                canvas.Children.Add(valueLabel);
                outerCircle.MouseEnter += (s, e) => { valueLabel.Visibility = Visibility.Visible; Canvas.SetLeft(valueLabel, x - (valueLabel.ActualWidth/2)); Canvas.SetTop(valueLabel, y - valueLabel.ActualHeight - 10); outerCircle.RenderTransform = new ScaleTransform(1.4, 1.4, outerCircle.Width/2, outerCircle.Height/2); };
                outerCircle.MouseLeave += (s, e) => { valueLabel.Visibility = Visibility.Collapsed; outerCircle.RenderTransform = null; };
                if (dataPoints.Count <= 10 || i % Math.Max(1, dataPoints.Count / 7) == 0 || i == dataPoints.Count - 1)
                {
                    var dateLabel = new TextBlock { Text = dataPoints[i].Date.ToString("MMM dd"), FontSize = 11, FontWeight = FontWeights.Medium, Foreground = (Brush)FindResource("MaterialDesignBody") };
                    canvas.Children.Add(dateLabel);
                    dateLabel.UpdateLayout();
                    Canvas.SetLeft(dateLabel, x - (dateLabel.ActualWidth/2));
                    Canvas.SetTop(dateLabel, actualHeight - plotMarginBottom + 8); 
                }
            }
            var xAxis = new Line { X1 = plotMarginLeft, Y1 = plotMarginTop + plotHeight, X2 = plotMarginLeft + plotWidth, Y2 = plotMarginTop + plotHeight, Stroke = (Brush)FindResource("MaterialDesignDivider"), StrokeThickness = 1 };
            canvas.Children.Add(xAxis);
            var yAxis = new Line { X1 = plotMarginLeft, Y1 = plotMarginTop, X2 = plotMarginLeft, Y2 = plotMarginTop + plotHeight, Stroke = (Brush)FindResource("MaterialDesignDivider"), StrokeThickness = 1 };
            canvas.Children.Add(yAxis);
        }

        private UIElement CreateModuleTypeVisualization(List<(string ModuleType, int Count)> moduleTypes)
        {
            var grid = new Grid();
            if (!moduleTypes.Any())
            {
                var noDataPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                var icon = new PackIcon { Kind = PackIconKind.SolarPanel, Width = 48, Height = 48, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12) };
                var text = new TextBlock { Text = "Keine Modultypdaten verfügbar", FontSize = 14, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center };
                noDataPanel.Children.Add(icon);
                noDataPanel.Children.Add(text);
                grid.Children.Add(noDataPanel);
                return grid;
            }
            var total = moduleTypes.Sum(x => x.Count);
            if (total == 0) 
            {
                grid.Children.Add(new TextBlock { Text = "Keine Module installiert", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), FontSize = 14 });
                return grid;
            }
            var mainStack = new StackPanel { Margin = new Thickness(0) };
            var colors = new[] { "#1E88E5", "#43A047", "#FB8C00", "#8E24AA", "#E53935", "#00ACC1", "#FDD835" };
            var sortedTypes = moduleTypes.OrderByDescending(m => m.Count).ToList();
            var chartGrid = new Grid { Height = 160, Margin = new Thickness(0, 0, 0, 20) };
            var donutCanvas = new Canvas { Width = 160, Height = 160, HorizontalAlignment = HorizontalAlignment.Center };
            double centerX = 80; double centerY = 80; double outerRadius = 70; double innerRadius = 40; double startAngle = -90;
            for (int i = 0; i < sortedTypes.Count && i < colors.Length; i++)
            {
                var moduleType = sortedTypes[i];
                var percentage = (double)moduleType.Count / total;
                var sweepAngle = percentage * 360;
                var path = CreateDonutSegment(centerX, centerY, outerRadius, innerRadius, startAngle, sweepAngle);
                path.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));
                path.Stroke = (Brush)FindResource("MaterialDesignPaper"); path.StrokeThickness = 2;
                path.MouseEnter += (s, e) => { path.RenderTransform = new ScaleTransform(1.05, 1.05, centerX, centerY); path.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.3 }; };
                path.MouseLeave += (s, e) => { path.RenderTransform = null; path.Effect = null; };
                donutCanvas.Children.Add(path);
                startAngle += sweepAngle;
            }
            var centerPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var totalText = new TextBlock { Text = total.ToString(), FontSize = 24, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
            var totalLabel = new TextBlock { Text = "Gesamt", FontSize = 12, Foreground = (Brush)FindResource("MaterialDesignBodyLight"), HorizontalAlignment = HorizontalAlignment.Center };
            centerPanel.Children.Add(totalText); centerPanel.Children.Add(totalLabel);
            chartGrid.Children.Add(donutCanvas); chartGrid.Children.Add(centerPanel);
            mainStack.Children.Add(chartGrid);
            var legendGrid = new Grid();
            legendGrid.ColumnDefinitions.Add(new ColumnDefinition()); legendGrid.ColumnDefinitions.Add(new ColumnDefinition());
            int row = 0; int col = 0;
            for (int i = 0; i < sortedTypes.Count && i < colors.Length; i++)
            {
                if (i > 0 && i % 2 == 0) { row++; col = 0; }
                var moduleType = sortedTypes[i];
                var percentage = (double)moduleType.Count / total * 100;
                var legendItem = new Border { Margin = new Thickness(0, 4, 8, 4), Padding = new Thickness(12, 8, 12, 8), Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)), CornerRadius = new CornerRadius(6) };
                var itemStack = new StackPanel();
                var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
                var colorBox = new Rectangle { Width = 12, Height = 12, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i])), RadiusX = 2, RadiusY = 2, VerticalAlignment = VerticalAlignment.Center };
                var typeText = new TextBlock { Text = moduleType.ModuleType, FontWeight = FontWeights.Medium, FontSize = 13, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                header.Children.Add(colorBox); header.Children.Add(typeText);
                var stats = new StackPanel { Orientation = Orientation.Horizontal };
                var countText = new TextBlock { Text = $"{moduleType.Count} Einheiten", FontSize = 12, Foreground = (Brush)FindResource("MaterialDesignBody") };
                var percentText = new TextBlock { Text = $" • {percentage:F1}%", FontSize = 12, FontWeight = FontWeights.Medium, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i])) };
                stats.Children.Add(countText); stats.Children.Add(percentText);
                itemStack.Children.Add(header); itemStack.Children.Add(stats);
                legendItem.Child = itemStack;
                if (legendGrid.RowDefinitions.Count <= row) legendGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(legendItem, row); Grid.SetColumn(legendItem, col);
                legendGrid.Children.Add(legendItem);
                col++;
            }
            mainStack.Children.Add(legendGrid);
            grid.Children.Add(mainStack);
            return grid;
        }

        private Path CreateDonutSegment(double centerX, double centerY, double outerRadius, double innerRadius, double startAngle, double sweepAngle)
        {
            var path = new Path(); var pathGeometry = new PathGeometry(); var pathFigure = new PathFigure();
            double startRad = startAngle * Math.PI / 180; double endRad = (startAngle + sweepAngle) * Math.PI / 180;
            double outerStartX = centerX + outerRadius * Math.Cos(startRad); double outerStartY = centerY + outerRadius * Math.Sin(startRad);
            double outerEndX = centerX + outerRadius * Math.Cos(endRad); double outerEndY = centerY + outerRadius * Math.Sin(endRad);
            double innerStartX = centerX + innerRadius * Math.Cos(startRad); double innerStartY = centerY + innerRadius * Math.Sin(startRad);
            double innerEndX = centerX + innerRadius * Math.Cos(endRad); double innerEndY = centerY + innerRadius * Math.Sin(endRad);
            pathFigure.StartPoint = new Point(outerStartX, outerStartY);
            var outerArc = new ArcSegment { Point = new Point(outerEndX, outerEndY), Size = new Size(outerRadius, outerRadius), IsLargeArc = sweepAngle > 180, SweepDirection = SweepDirection.Clockwise };
            pathFigure.Segments.Add(outerArc);
            pathFigure.Segments.Add(new LineSegment(new Point(innerEndX, innerEndY), true));
            var innerArc = new ArcSegment { Point = new Point(innerStartX, innerStartY), Size = new Size(innerRadius, innerRadius), IsLargeArc = sweepAngle > 180, SweepDirection = SweepDirection.Counterclockwise };
            pathFigure.Segments.Add(innerArc);
            pathFigure.IsClosed = true; pathGeometry.Figures.Add(pathFigure); path.Data = pathGeometry;
            return path;
        }

        private void UpdateLastUpdatedTime()
        {
            LastUpdatedText.Text = $"Zuletzt aktualisiert: {DateTime.Now:HH:mm:ss}";
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshButton.IsEnabled = false;
                RefreshButton.Content = "AKTUALISIERE...";
                await InitializeAsync();
            }
            finally
            {
                RefreshButton.IsEnabled = true;
                RefreshButton.Content = "DATEN AKTUALISIEREN";
            }
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string page)
            {
                NavigateToPage(page);
                UpdateActiveNavigation(button);
            }
        }

        private void NavigateToPage(string page)
        {
            UserControl? pageToNavigate = null;
            switch (page)
            {
                case "Dashboard":
                    PageTitle.Text = "Dashboard";
                    PageSubtitle.Text = "Systemübersicht und Statistiken";
                    ContentArea.Content = DashboardContent;
                    break;
                case "Customers":
                    PageTitle.Text = "Kundenverwaltung";
                    PageSubtitle.Text = "Kunden und deren Informationen verwalten";
                    if (_customersPage == null) { _customersPage = new CustomersPage(_databaseService); }
                    pageToNavigate = _customersPage;
                    break;
                case "Modules":
                    SetPageTitle("Solarmodultypen", "Solarmodultypen und deren Spezifikationen verwalten");
                    pageToNavigate = new SolarmodultypPage();
                    break;
                case "Performance":
                    SetPageTitle("Leistungsanalyse", "Energieertrag und Systemleistung analysieren");
                    pageToNavigate = CreatePlaceholderPage("Leistungsanalyse", "Leistungsanalysen in Kürze verfügbar...");
                    break;
                case "Anlagen":
                    SetPageTitle("Anlagenübersicht", "Alle Solaranlagen und deren Status einsehen und verwalten");
                    if (_anlagenPage == null) _anlagenPage = ((App)Application.Current).ServiceProvider.GetRequiredService<AnlagenPage>();
                    pageToNavigate = _anlagenPage;
                    break;
            }
            if (pageToNavigate != null)
            {
                ContentArea.Content = pageToNavigate;
                UpdateLastUpdatedTime();
            }
        }

        private void UpdateActiveNavigation(Button activeButton)
        {
            ResetNavigationButtonStyle(DashboardButton);
            ResetNavigationButtonStyle(CustomersButton);
            ResetNavigationButtonStyle(AnlagenButton);
            ResetNavigationButtonStyle(ModulesButton);
            SetActiveNavigationButtonStyle(activeButton);
        }

        private void ResetNavigationButtonStyle(Button button)
        {
            button.Background = Brushes.Transparent;
            button.Opacity = 0.8;
        }

        private void SetActiveNavigationButtonStyle(Button button)
        {
            try
            {
                button.Background = (Brush)FindResource("PrimaryHueMidBrush");
                button.Opacity = 1.0;
            }
            catch
            {
                button.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                button.Opacity = 1.0;
            }
        }

        private UserControl CreatePlaceholderPage(string title, string message)
        {
            var userControl = new UserControl();
            var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var icon = new PackIcon { Kind = PackIconKind.Creation, Width = 48, Height = 48, HorizontalAlignment = HorizontalAlignment.Center, Foreground = (Brush)FindResource("MaterialDesignBodyLight") };
            var titleBlock = new TextBlock { Text = title, FontSize = 22, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 12, 0, 6), Foreground = (Brush)FindResource("MaterialDesignBody") };
            var messageBlock = new TextBlock { Text = message, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, Foreground = (Brush)FindResource("MaterialDesignBodyLight") };
            stackPanel.Children.Add(icon); stackPanel.Children.Add(titleBlock); stackPanel.Children.Add(messageBlock);
            userControl.Content = stackPanel;
            return userControl;
        }

        private void InitializeTheme()
        {
            DarkModeToggle.IsChecked = _isDarkMode;
            ApplyTheme(_isDarkMode);
        }

        private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = DarkModeToggle.IsChecked ?? false;
            ApplyTheme(_isDarkMode);
        }

        private void ApplyTheme(bool isDarkMode)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            
            if (isDarkMode)
            {
                theme.SetBaseTheme(Theme.Dark);
                theme.Paper = Color.FromRgb(0x00, 0x00, 0x00);
                theme.CardBackground = Color.FromRgb(0x11, 0x11, 0x11);
                theme.ToolBarBackground = Color.FromRgb(0x0A, 0x0A, 0x0A);
                theme.Body = Color.FromRgb(0xF0, 0xF0, 0xF0);
                theme.BodyLight = Color.FromRgb(0xA8, 0xA8, 0xA8);
                theme.ColumnHeader = Color.FromRgb(0x1A, 0x1A, 0x1A);
                theme.Selection = Color.FromRgb(0x2A, 0x2A, 0x2A);
                theme.Divider = Color.FromRgb(0x2E, 0x2E, 0x2E);
                
                theme.PrimaryMid = Color.FromRgb(0x00, 0x7A, 0xFF);
                theme.PrimaryLight = Color.FromRgb(0x33, 0x99, 0xFF);
                theme.PrimaryDark = Color.FromRgb(0x00, 0x5C, 0xC8);

                theme.SecondaryMid = Color.FromRgb(0x00, 0x7A, 0xFF);
                theme.SecondaryLight = Color.FromRgb(0x33, 0x99, 0xFF);
                theme.SecondaryDark = Color.FromRgb(0x00, 0x5C, 0xC8);
            }
            else
            {
                theme.SetBaseTheme(Theme.Light);
                theme.Paper = Colors.White;
                theme.CardBackground = Colors.White;
                theme.ToolBarBackground = Color.FromRgb(0xF5, 0xF5, 0xF5);
                theme.Body = Color.FromRgb(0x21, 0x21, 0x21);
                theme.BodyLight = Color.FromRgb(0x75, 0x75, 0x75);
                theme.ColumnHeader = Color.FromRgb(0xF5, 0xF5, 0xF5);
                theme.Selection = Color.FromRgb(0xE0, 0xE0, 0xE0);
                theme.Divider = Color.FromRgb(0xDB, 0xDB, 0xDB);

                theme.PrimaryMid = Color.FromRgb(0x4C, 0xAF, 0x50);
                theme.PrimaryLight = Color.FromRgb(0x81, 0xC7, 0x84);
                theme.PrimaryDark = Color.FromRgb(0x38, 0x8E, 0x3C);

                theme.SecondaryMid = Color.FromRgb(0x03, 0xA9, 0xF4);
                theme.SecondaryLight = Color.FromRgb(0x81, 0xD4, 0xFA);
                theme.SecondaryDark = Color.FromRgb(0x02, 0x88, 0xD1);
            }
            
            paletteHelper.SetTheme(theme);
        }

        private void ViewReportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Detailed module analysis/reporting feature placeholder.", "Feature Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"PowerData_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportDashboardDataToCsv(saveFileDialog.FileName);
                    MessageBox.Show($"Dashboard data exported successfully to {saveFileDialog.FileName}", 
                                  "Export Successful", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", 
                              "Export Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private async void ExportDashboardDataToCsv(string filePath)
        {
            var recentMeasurements = await _databaseService.GetRecentLeistungenAsync(1000);
            
            using var writer = new System.IO.StreamWriter(filePath);
            
            writer.WriteLine("Timestamp,Module Number,Power Output (W),Customer Name,Module Type");
            
            foreach (var measurement in recentMeasurements)
            {
                writer.WriteLine($"{measurement.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                               $"{measurement.Modulnummer}," +
                               $"{measurement.PowerOut}," +
                               $"{measurement.Solarmodul?.Kunde?.Nachname ?? "Unknown"}," +
                               $"{measurement.Solarmodul?.Solarmodultyp?.Bezeichnung ?? "Unknown"}");
            }
        }

        private async Task LoadWeatherDataAsync()
        {
            try
            {
                WeatherForecasts.Clear();
                string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=52.4265&longitude=7.0686&hourly=temperature_2m,weathercode&forecast_days=3&current_weather=true&timezone=Europe/Berlin";
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl);
                    var weatherData = JsonDocument.Parse(response);
                    var hourlyTemps = weatherData.RootElement.GetProperty("hourly").GetProperty("temperature_2m").EnumerateArray().Take(72).ToArray();
                    var hourlyCodes = weatherData.RootElement.GetProperty("hourly").GetProperty("weathercode").EnumerateArray().Take(72).ToArray();
                    for (int day = 0; day < 3; day++)
                    {
                        var dayStart = day * 24;
                        var dayTemps = hourlyTemps.Skip(dayStart).Take(24).Select(t => t.GetDouble()).ToArray();
                        var dayCodes = hourlyCodes.Skip(dayStart).Take(24).Select(c => c.GetInt32()).ToArray();
                        if (dayTemps.Any())
                        {
                            var avgTemp = dayTemps.Average();
                            var mostCommonCode = dayCodes.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key;
                            var dayName = day == 0 ? "Heute" : day == 1 ? "Morgen" : "Übermorgen";
                            var description = GetWeatherDescription(mostCommonCode.ToString());
                            var iconKind = GetWeatherIcon(mostCommonCode.ToString());
                            WeatherForecasts.Add(new WeatherForecastViewModel { Day = dayName, Description = description, Temperature = $"{avgTemp:F0}°C", IconKind = iconKind });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WeatherForecasts.Clear();
                WeatherForecasts.Add(new WeatherForecastViewModel { Day="Heute", Description="Sonnige Abschnitte", Temperature="22°C", IconKind = PackIconKind.WeatherPartlyCloudy });
                WeatherForecasts.Add(new WeatherForecastViewModel { Day="Morgen", Description="Meist sonnig", Temperature="24°C", IconKind = PackIconKind.WeatherSunny });
                WeatherForecasts.Add(new WeatherForecastViewModel { Day="Übermorgen", Description="Schauer erwartet", Temperature="19°C", IconKind = PackIconKind.WeatherRainy });
            }
            OnPropertyChanged(nameof(WeatherForecasts));
        }

        private string GetWeatherDescription(string weatherCode)
        {
            return weatherCode switch
            {
                "0" => "Klarer Himmel", "1" => "Überwiegend klar", "2" => "Teilweise bewölkt", "3" => "Bedeckt",
                "45" => "Nebel", "48" => "Reifnebel",
                "51" => "Leichter Nieselregen", "53" => "Mäßiger Nieselregen", "55" => "Starker Nieselregen",
                "61" => "Leichter Regen", "63" => "Mäßiger Regen", "65" => "Starker Regen",
                "71" => "Leichter Schneefall", "73" => "Mäßiger Schneefall", "75" => "Starker Schneefall",
                "80" => "Leichte Regenschauer", "81" => "Mäßige Regenschauer", "82" => "Heftige Regenschauer",
                "95" => "Gewitter", "96" => "Gewitter mit leichtem Hagel", "99" => "Gewitter mit starkem Hagel",
                _ => "Unbekannt"
            };
        }

        private PackIconKind GetWeatherIcon(string weatherCode)
        {
            return weatherCode switch
            {
                "0" => PackIconKind.WeatherSunny,
                "1" => PackIconKind.WeatherSunny,
                "2" => PackIconKind.WeatherPartlyCloudy,
                "3" => PackIconKind.WeatherCloudy,
                "45" => PackIconKind.WeatherFog,
                "48" => PackIconKind.WeatherFog,
                "51" => PackIconKind.WeatherRainy,
                "53" => PackIconKind.WeatherRainy,
                "55" => PackIconKind.WeatherRainy,
                "61" => PackIconKind.WeatherRainy,
                "63" => PackIconKind.WeatherRainy,
                "65" => PackIconKind.WeatherPouring,
                "71" => PackIconKind.WeatherSnowy,
                "73" => PackIconKind.WeatherSnowy,
                "75" => PackIconKind.WeatherSnowyHeavy,
                "80" => PackIconKind.WeatherRainy,
                "81" => PackIconKind.WeatherRainy,
                "82" => PackIconKind.WeatherPouring,
                "95" => PackIconKind.WeatherLightning,
                "96" => PackIconKind.WeatherLightning,
                "99" => PackIconKind.WeatherLightningRainy,
                _ => PackIconKind.WeatherPartlyCloudy
            };
        }

        private void ViewAllMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View all measurements placeholder. This could navigate to a new page/view.", "Feature Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetPageTitle(string title, string subtitle)
        {
            PageTitle.Text = title;
            PageSubtitle.Text = subtitle;
        }
    }

    // Simple ViewModel for Stat Cards (as per user's example)
    public class StatCardViewModel
    {
        public string? Title { get; set; }
        public string? Value { get; set; }
        public string? Subtitle { get; set; }
        public Brush? SubtitleColor { get; set; }
        public PackIconKind IconKind { get; set; }
    }

    // Simple ViewModel for Top Anlagen
    public class TopAnlageViewModel
    {
        public string? Name { get; set; }
        public string? Power { get; set; }
    }

    // Simple ViewModel for Weather Forecast (dummy)
    public class WeatherForecastViewModel
    {
        public string? Day { get; set; }
        public string? Description { get; set; }
        public string? Temperature { get; set; }
        public PackIconKind IconKind { get; set; }
    }
}