using System.Windows;
using System.Windows.Controls;
using power4you_admin.Models;
using power4you_admin.Services;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.ComponentModel;
using System.Windows.Input;

namespace power4you_admin.Views;

public partial class AnlagePerformanceDialog : Window, INotifyPropertyChanged
{
    private readonly Anlage _anlage;
    private readonly AnlageService _anlageService;
    private List<Leistung> _currentPerformanceData = new List<Leistung>();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ObservableCollection<ISeries> _series = new ObservableCollection<ISeries>();
    public ObservableCollection<ISeries> Series
    {
        get => _series;
        set
        {
            _series = value;
            OnPropertyChanged(nameof(Series));
        }
    }

    private ObservableCollection<Axis> _xAxes = new ObservableCollection<Axis>
    {
        new Axis
        {
            Name = "Zeitstempel",
            Labeler = value =>
            {
                // Check if the value is a valid long and within DateTime's tick range
                if (double.IsNaN(value) || double.IsInfinity(value))
                    return string.Empty; // Handle non-finite doubles
                try
                {
                    long ticks = Convert.ToInt64(value);
                    if (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
                        return new DateTime(ticks).ToString("g"); // Default short date/time
                    return string.Empty; // Ticks out of range
                }
                catch (OverflowException)
                {
                    return string.Empty; // Value too large/small for Int64
                }
            },
            UnitWidth = TimeSpan.FromDays(1).Ticks, 
            MinStep = TimeSpan.FromHours(1).Ticks 
        }
    };
    public ObservableCollection<Axis> XAxes
    {
        get => _xAxes;
        set
        {
            _xAxes = value;
            OnPropertyChanged(nameof(XAxes));
        }
    }

    private ObservableCollection<Axis> _yAxes = new ObservableCollection<Axis>
    {
        new Axis
        {
            Name = "Gesamtleistung (W)",
            MinLimit = 0
        }
    };
    public ObservableCollection<Axis> YAxes
    {
        get => _yAxes;
        set
        {
            _yAxes = value;
            OnPropertyChanged(nameof(YAxes));
        }
    }

    public AnlagePerformanceDialog(Anlage anlage, AnlageService anlageService)
    {
        InitializeComponent();
        _anlage = anlage;
        _anlageService = anlageService;
        DataContext = this; // Set DataContext for LiveCharts bindings
        
        Loaded += async (s, e) => await LoadPerformanceDataAsync();
        PerformanceChart.PreviewMouseWheel += PerformanceChart_PreviewMouseWheel; // Attach handler
    }

    private void PerformanceChart_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // When the mouse is over the chart, handle the mouse wheel event for chart zooming/panning
        // and mark it as handled to prevent the parent ScrollViewer from also scrolling.
        if (sender is CartesianChart chart && chart.IsMouseOver)
        {
            // The chart handles its own zooming via mouse wheel if ZoomMode is not None.
            // We just need to ensure the event doesn't propagate further.
            e.Handled = true;
        }
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        TimeRangeComboBox.IsEnabled = !isLoading;
        ExportButton.IsEnabled = !isLoading;
        PerformanceDataGrid.IsEnabled = !isLoading;
        PerformanceChart.IsEnabled = !isLoading; // Disable chart during load too
    }

    private async Task LoadPerformanceDataAsync()
    {
        if (_anlage == null) 
        {
            return;
        }

        if (TimeRangeComboBox.SelectedItem is not ComboBoxItem selectedTimeItem || selectedTimeItem.Tag is not string timeRangeTag)
        {
            return; 
        }

        ShowLoading(true);
        PerformanceDataGrid.ItemsSource = null;
        Series.Clear(); // Clear previous chart series
        AvgPowerText.Text = "-";
        MaxPowerText.Text = "-";
        TotalMeasurementsText.Text = "-";

        DateTime startDate;
        DateTime endDate = DateTime.Now;
        string timeRangeDescription;

        switch (timeRangeTag)
        {
            case "24H":
                startDate = DateTime.Now.AddHours(-24);
                timeRangeDescription = "letzten 24 Stunden";
                XAxes[0].UnitWidth = TimeSpan.FromHours(1).Ticks;
                XAxes[0].MinStep = TimeSpan.FromMinutes(30).Ticks;
                XAxes[0].Labeler = value => 
                {
                    if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
                    try { long ticks = Convert.ToInt64(value); return (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks).ToString("HH:mm") : string.Empty; }
                    catch (OverflowException) { return string.Empty; }
                };
                break;
            case "7D":
                startDate = DateTime.Now.AddDays(-7).Date; 
                timeRangeDescription = "letzten 7 Tage";
                XAxes[0].UnitWidth = TimeSpan.FromDays(1).Ticks;
                XAxes[0].MinStep = TimeSpan.FromHours(12).Ticks;
                XAxes[0].Labeler = value => 
                {
                    if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
                    try { long ticks = Convert.ToInt64(value); return (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks).ToString("dd.MM HH:mm") : string.Empty; }
                    catch (OverflowException) { return string.Empty; }
                };
                break;
            case "90D":
                startDate = DateTime.Now.AddDays(-90).Date;
                timeRangeDescription = "letzten 90 Tage";
                XAxes[0].UnitWidth = TimeSpan.FromDays(7).Ticks; // Weekly ticks
                XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;
                XAxes[0].Labeler = value => 
                {
                    if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
                    try { long ticks = Convert.ToInt64(value); return (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks).ToString("dd.MM.yy") : string.Empty; }
                    catch (OverflowException) { return string.Empty; }
                };
                break;
            case "All":
                startDate = DateTime.MinValue; 
                timeRangeDescription = "gesamten Zeitraum";
                XAxes[0].UnitWidth = TimeSpan.FromDays(30).Ticks; // Monthly ticks or adjust dynamically
                XAxes[0].MinStep = TimeSpan.FromDays(7).Ticks;
                XAxes[0].Labeler = value => 
                {
                    if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
                    try { long ticks = Convert.ToInt64(value); return (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks).ToString("MMM yyyy") : string.Empty; }
                    catch (OverflowException) { return string.Empty; }
                };
                break;
            case "30D":
            default:
                startDate = DateTime.Now.AddDays(-30).Date;
                timeRangeDescription = "letzten 30 Tage";
                XAxes[0].UnitWidth = TimeSpan.FromDays(1).Ticks;
                XAxes[0].MinStep = TimeSpan.FromHours(6).Ticks;
                XAxes[0].Labeler = value => 
                {
                    if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
                    try { long ticks = Convert.ToInt64(value); return (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks).ToString("dd.MM") : string.Empty; }
                    catch (OverflowException) { return string.Empty; }
                };
                break;
        }
        OnPropertyChanged(nameof(XAxes)); // Notify that XAxes configuration changed

        try
        {
            TitleText.Text = $"Leistungsanalyse - {_anlage.FormattedName}";
            SubtitleText.Text = $"Daten für {timeRangeDescription} für {(_anlage.Kunde?.Vorname ?? "N/A")} {(_anlage.Kunde?.Nachname ?? "N/A")}";
            
            var statisticsTask = _anlageService.GetAnlageStatisticsAsync(_anlage.Kundennummer, startDate, endDate);
            // Fetch history for chart (total power) and grid (sampled individual modules)
            var performanceHistoryTask = _anlageService.GetPerformanceHistoryAsync(_anlage.Kundennummer, startDate, endDate); 

            await Task.WhenAll(statisticsTask, performanceHistoryTask);

            var statistics = await statisticsTask;
            _currentPerformanceData = await performanceHistoryTask;
            
            AvgPowerText.Text = $"{statistics.DurchschnittlichePowerLetzten30Tage:F1} W";
            MaxPowerText.Text = $"{statistics.MaximalePowerLetzten30Tage:F1} W";
            TotalMeasurementsText.Text = statistics.GesamtMessungen.ToString(); // This count is now based on the selected period in GetAnlageStatisticsAsync
            
            // Prepare data for the DataGrid (sampled per module)
            PerformanceDataGrid.ItemsSource = _currentPerformanceData.OrderByDescending(p => p.Timestamp).Take(500);

            // Prepare data for the Chart (sum of PowerOut per Timestamp)
            var chartData = _currentPerformanceData
                .GroupBy(p => p.Timestamp) // Group by the sampled timestamp
                .Select(g => new DateTimePoint(g.Key, g.Sum(p => p.PowerOut))) // DateTimePoint for time series
                .OrderBy(dp => dp.DateTime) // Ensure chronological order for the chart
                .ToList();

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    Name = "Gesamtleistung",
                    Values = chartData,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue.WithAlpha(90)),
                    Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.CornflowerBlue),
                    GeometryStroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                    LineSmoothness = 0.65
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Laden der Leistungsdaten: {ex.Message}", 
                          "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            AvgPowerText.Text = "Fehler";
            MaxPowerText.Text = "Fehler";
            TotalMeasurementsText.Text = "Fehler";
            Series.Clear();
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private async void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) 
        {
            await LoadPerformanceDataAsync();
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentPerformanceData.Any())
        {
            MessageBox.Show("Keine Daten zum Exportieren vorhanden.", "Exportieren", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Leistung_{_anlage.FormattedName?.Replace(" ", "_")}_{((ComboBoxItem)TimeRangeComboBox.SelectedItem).Tag}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Export _currentPerformanceData which is already sampled
                ExportPerformanceDataToCsv(saveFileDialog.FileName, _currentPerformanceData);
                MessageBox.Show($"Leistungsdaten erfolgreich nach {saveFileDialog.FileName} exportiert.", 
                              "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Exportieren der Leistungsdaten: {ex.Message}", 
                          "Exportfehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ExportPerformanceDataToCsv(string filePath, IEnumerable<Leistung> performanceData)
    {
        using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        
        writer.WriteLine("Zeitstempel;Modul Nr.;Leistung (W);Modultyp;Modultyp Spannung (Umpp);Modultyp Strom (Impp);Modultyp Leistung (Pmpp)");
        
        foreach (var data in performanceData)
        {
            var line = string.Format("\"{0:yyyy-MM-dd HH:mm:ss}\";{1};{2};\"{3}\";{4};{5};{6}",
                                 data.Timestamp,
                                 data.Modulnummer,
                                 data.PowerOut,
                                 data.Solarmodul?.Solarmodultyp?.Bezeichnung?.Replace("\"", "\"\"") ?? "Unbekannt",
                                 data.Solarmodul?.Solarmodultyp?.Umpp.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "N/A",
                                 data.Solarmodul?.Solarmodultyp?.Impp.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "N/A",
                                 data.Solarmodul?.Solarmodultyp?.Pmpp.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "N/A"
                                 );
            writer.WriteLine(line);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 