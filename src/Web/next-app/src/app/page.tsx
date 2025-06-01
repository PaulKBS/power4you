'use client';

import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import StatusCard from '@/components/dashboard/StatusCard';
import WeatherForecast from '@/components/dashboard/WeatherForecast';
import DateRangePicker from '@/components/dashboard/DateRangePicker';
import YieldHeatmap from '@/components/dashboard/YieldHeatmap';

// Define types for the chart data
interface LineChartDataPoint {
  time: string;
  production: number;
  consumption: number;
  feedIn: number;
}

interface PieChartDataPoint {
  name: string;
  value: number;
}

interface HeatmapDataPoint {
  date: string;
  yieldValue: number;
}

const COLORS = ['#f97316', '#22c55e', '#3b82f6'];

export default function Home() {
  const [lineChartData, setLineChartData] = useState<LineChartDataPoint[]>([]);
  const [pieChartData, setPieChartData] = useState<PieChartDataPoint[]>([]);
  const [heatmapData, setHeatmapData] = useState<HeatmapDataPoint[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isHeatmapLoading, setIsHeatmapLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentView, setCurrentView] = useState<'hourly' | 'daily'>('hourly');
  const [currentDateRange, setCurrentDateRange] = useState<{start: string, end: string} | null>(null);

  // Fetch energy data based on parameters
  const fetchEnergyData = async (startDate?: string, endDate?: string, view: 'hourly' | 'daily' = 'hourly') => {
    try {
      setIsLoading(true);
      
      const params = new URLSearchParams();
      if (startDate) params.append('startDate', startDate);
      if (endDate) params.append('endDate', endDate);
      params.append('view', view);

      console.log('Fetching energy data with params:', { startDate, endDate, view });
      const response = await fetch(`/api/energy?${params.toString()}`);

      if (!response.ok) {
        if (response.status === 401) {
          console.log('User not authenticated');
          setError('Nicht authentifiziert');
          setIsLoading(false);
          return;
        }
        throw new Error(`Error fetching energy data: ${response.statusText}`);
      }

      const data = await response.json();
      console.log('Received data:', data);

      if (data.lineChartData && data.lineChartData.length > 0) {
        // For hourly view, filter out future times only if it's today's data
        if (view === 'hourly' && (!startDate || startDate === new Date().toISOString().split('T')[0])) {
          const now = new Date();
          const currentHour = now.getHours();
          const currentMinute = now.getMinutes();

          const filteredLineChartData = data.lineChartData.filter((point: LineChartDataPoint) => {
            const [hour, minute] = point.time.split(':').map(Number);
            return hour < currentHour || (hour === currentHour && minute <= currentMinute);
          });

          setLineChartData(filteredLineChartData);
        } else {
          setLineChartData(data.lineChartData);
        }
      }

      if (data.pieChartData && data.pieChartData.length > 0) {
        setPieChartData(data.pieChartData);
      }

      setCurrentView(view);
      setCurrentDateRange(startDate && endDate ? { start: startDate, end: endDate } : null);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch energy data:', err);
      setError('Fehler beim Laden der Energiedaten');
    } finally {
      setIsLoading(false);
    }
  };

  // Fetch heatmap data
  const fetchHeatmapData = async () => {
    try {
      setIsHeatmapLoading(true);
      const response = await fetch('/api/energy?view=heatmap');

      if (!response.ok) {
        throw new Error(`Error fetching heatmap data: ${response.statusText}`);
      }

      const data = await response.json();
      
      if (data.heatmapData) {
        setHeatmapData(data.heatmapData);
      }
    } catch (err) {
      console.error('Failed to fetch heatmap data:', err);
    } finally {
      setIsHeatmapLoading(false);
    }
  };

  // Handle date range changes from the picker
  const handleDateRangeChange = (startDate: string, endDate: string, view: 'hourly' | 'daily') => {
    fetchEnergyData(startDate, endDate, view);
  };

  // Initial data fetch
  useEffect(() => {
    // Load today's data (same as "Heute" button) instead of last 24 hours
    const today = new Date().toISOString().split('T')[0];
    fetchEnergyData(today, today, 'hourly'); // Load today's hourly data from 00:00 to 23:59
    fetchHeatmapData(); // Load heatmap data
  }, []);

  // Loading spinner
  if (isLoading && lineChartData.length === 0) {
    return (
      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          <p className="mt-4 text-muted-foreground">Lade Energiedaten...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
        <div className="bg-red-50 border border-red-200 text-red-800 dark:bg-red-900 dark:border-red-800 dark:text-red-100 rounded-md p-4 mb-6">
          <p>{error}</p>
        </div>
      </div>
    );
  }

  // Calculate current power and daily yield values from the line chart data
  const currentPower = lineChartData.length > 0 ? (() => {
    if (currentView === 'daily') {
      // For daily view, show the latest day's production
      const latestDay = lineChartData[lineChartData.length - 1];
      return `${latestDay.production.toFixed(1)} kWh`;
    } else {
      // For hourly view, find the closest time entry
      const now = new Date();
      const currentHour = now.getHours();

      const closestEntry = lineChartData.reduce((closest, entry) => {
        const entryHour = parseInt(entry.time.split(':')[0]);
        const currentDiff = Math.abs(currentHour - entryHour);
        const closestDiff = Math.abs(currentHour - parseInt(closest.time.split(':')[0]));

        return currentDiff < closestDiff ? entry : closest;
      }, lineChartData[0]);

      return `${closestEntry.production.toFixed(1)} kW`;
    }
  })() : '0.0 kW';

  const dailyYieldValue = lineChartData.length > 0 ?
    lineChartData.reduce((sum, data) => sum + data.production, 0) : 0;

  const dailyYield = currentView === 'daily' ? 
    `${dailyYieldValue.toFixed(1)} kWh` : 
    `${dailyYieldValue.toFixed(1)} kWh`;

  // Calculate CO2 savings based on daily yield
  const co2SavingsDaily = dailyYieldValue * 0.42; // kg CO2
  const co2Savings = co2SavingsDaily >= 1000 ?
    `${(co2SavingsDaily / 1000).toFixed(2)}t` :
    `${co2SavingsDaily.toFixed(1)}kg`;

  // Battery charge level (in percentage)
  const batteryCharge = 70;

  // Calculate self-sufficiency rate (Autarkie)
  const totalConsumption = lineChartData.length > 0 ?
    lineChartData.reduce((sum, data) => sum + data.consumption, 0) : 0;
  const selfSufficiencyRate = totalConsumption > 0 ?
    Math.min(100, Math.round((dailyYieldValue / totalConsumption) * 100)) : 0;

  // Battery color based on charge level
  const getBatteryColor = () => {
    if (batteryCharge <= 20) return 'bg-red-500';
    if (batteryCharge <= 40) return 'bg-yellow-500';
    return 'bg-green-500';
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
      {/* Date Range Picker */}
      <DateRangePicker onDateRangeChange={handleDateRangeChange} isLoading={isLoading} />

      {/* Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatusCard
          title={currentView === 'daily' ? 'Letzte Tagesproduktion' : 'Aktuelle Leistung'}
          value={currentPower}
          icon="fa-sun"
          iconColor="text-orange-500"
        />
        <StatusCard
          title={currentView === 'daily' ? 'Gesamtertrag (Zeitraum)' : 'Tagesertrag'}
          value={dailyYield}
          icon="fa-chart-line"
          iconColor="text-orange-500"
        />
        <StatusCard
          title="Autarkie"
          value={`${selfSufficiencyRate}%`}
          icon="fa-house-circle-check"
          iconColor="text-blue-500"
        />
        <StatusCard
          title="CO₂ Einsparung"
          value={co2Savings}
          icon="fa-leaf"
          iconColor="text-green-500"
        />
      </div>

      {/* Main Chart Section */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="col-span-1 md:col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>
                Energieproduktion & Verbrauch
                {currentDateRange && (
                  <span className="text-sm font-normal text-muted-foreground ml-2">
                    ({new Date(currentDateRange.start).toLocaleDateString('de-DE')} - {new Date(currentDateRange.end).toLocaleDateString('de-DE')})
                  </span>
                )}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="h-[400px]">
                {lineChartData.length > 0 ? (
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={lineChartData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis 
                        dataKey="time" 
                        stroke="var(--muted-foreground)"
                        tick={{ fontSize: 12 }}
                      />
                      <YAxis 
                        stroke="var(--muted-foreground)"
                        label={{ 
                          value: currentView === 'daily' ? 'Energie (kWh)' : 'Leistung (kW)', 
                          angle: -90, 
                          position: 'insideLeft' 
                        }}
                      />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: 'var(--card)',
                          color: 'var(--card-foreground)',
                          borderRadius: '4px',
                          padding: '8px',
                          border: '1px solid var(--border)'
                        }}
                        itemStyle={{
                          color: 'var(--card-foreground)'
                        }}
                        labelStyle={{
                          color: 'var(--card-foreground)'
                        }}
                        formatter={(value, name) => [
                          `${value} ${currentView === 'daily' ? 'kWh' : 'kW'}`,
                          name === 'production' ? 'Produktion' : 
                          name === 'consumption' ? 'Verbrauch' : 'Einspeisung'
                        ]}
                      />
                      <Line type="monotone" dataKey="production" stroke="#f97316" strokeWidth={2} />
                      <Line type="monotone" dataKey="consumption" stroke="#3b82f6" strokeWidth={2} />
                      <Line type="monotone" dataKey="feedIn" stroke="#22c55e" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                ) : (
                  <div className="flex items-center justify-center h-full text-muted-foreground">
                    {isLoading ? (
                      <div className="flex items-center space-x-2">
                        <div className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-solid border-orange-500 border-r-transparent"></div>
                        <span>Lade Daten...</span>
                      </div>
                    ) : (
                      'Keine Daten verfügbar'
                    )}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
        <div className="col-span-1">
          <Card>
            <CardHeader>
              <CardTitle>Legende</CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="mt-4 space-y-3">
                <li className="flex items-center space-x-3">
                  <span className="w-4 h-4 bg-orange-500 rounded-full"></span>
                  <span>Produktion</span>
                </li>
                <li className="flex items-center space-x-3">
                  <span className="w-4 h-4 bg-blue-500 rounded-full"></span>
                  <span>Verbrauch</span>
                </li>
                <li className="flex items-center space-x-3">
                  <span className="w-4 h-4 bg-green-500 rounded-full"></span>
                  <span>Einspeisung</span>
                </li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mt-4">
            <CardHeader>
              <CardTitle>Batteriestatus</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-col items-center">
                <div className="w-20 h-36 border-2 border-gray-300 rounded-md relative mb-2">
                  <div className={`absolute bottom-0 left-0 right-0 ${getBatteryColor()} rounded-sm`} style={{ height: `${batteryCharge}%` }}></div>
                  <div className="absolute w-8 h-3 bg-gray-300 left-1/2 -translate-x-1/2 -top-3 rounded-sm"></div>
                </div>
                <span className="text-xl font-bold">{batteryCharge}%</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Heatmap and Energy Distribution */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <YieldHeatmap data={heatmapData} isLoading={isHeatmapLoading} />
        <Card>
          <CardHeader>
            <CardTitle>Energieverteilung</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-[200px]">
              {pieChartData.length > 0 ? (
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={pieChartData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {pieChartData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      formatter={(value, name) => [`${value}%`, name]}
                      contentStyle={{
                        backgroundColor: 'var(--card)',
                        color: 'var(--card-foreground)',
                        borderRadius: '4px',
                        padding: '8px',
                        border: '1px solid var(--border)'
                      }}
                      itemStyle={{
                        color: 'var(--card-foreground)'
                      }}
                      labelStyle={{
                        color: 'var(--card-foreground)'
                      }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <div className="flex items-center justify-center h-full text-muted-foreground">
                  Keine Daten verfügbar
                </div>
              )}
            </div>
            {pieChartData.length > 0 && (
              <div className="mt-6 space-y-2">
                <ul className="grid grid-cols-1 gap-2">
                  {pieChartData.map((entry, index) => (
                    <li key={index} className="flex items-center justify-between">
                      <div className="flex items-center space-x-2">
                        <span className="w-3 h-3 rounded-full" style={{ backgroundColor: COLORS[index % COLORS.length] }}></span>
                        <span>{entry.name}</span>
                      </div>
                      <span className="font-medium">{entry.value}%</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Weather Forecast */}
      <div className="grid grid-cols-1 gap-4">
        <WeatherForecast />
      </div>
    </div>
  );
}
