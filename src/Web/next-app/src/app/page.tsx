'use client';

import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import StatusCard from '@/components/dashboard/StatusCard';
import WeatherForecast from '@/components/dashboard/WeatherForecast';

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

const COLORS = ['#f97316', '#22c55e', '#3b82f6'];

export default function Home() {
  const [lineChartData, setLineChartData] = useState<LineChartDataPoint[]>([]);
  const [pieChartData, setPieChartData] = useState<PieChartDataPoint[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchEnergyData() {
      try {
        setIsLoading(true);
        const response = await fetch('/api/energy');
        
        if (!response.ok) {
          // If not authenticated, handle accordingly
          if (response.status === 401) {
            console.log('User not authenticated');
            setError('Nicht authentifiziert');
            setIsLoading(false);
            return;
          }
          
          throw new Error(`Error fetching energy data: ${response.statusText}`);
        }
        
        const data = await response.json();
        
        // If we have valid data, update the states
        if (data.lineChartData && data.lineChartData.length > 0) {
          // Filter out future times
          const now = new Date();
          const currentHour = now.getHours();
          const currentMinute = now.getMinutes();
          
          const filteredLineChartData = data.lineChartData.filter((point: LineChartDataPoint) => {
            const [hour, minute] = point.time.split(':').map(Number);
            return hour < currentHour || (hour === currentHour && minute <= currentMinute);
          });
          
          setLineChartData(filteredLineChartData);
        }
        
        if (data.pieChartData && data.pieChartData.length > 0) {
          setPieChartData(data.pieChartData);
        }
        
        setError(null);
      } catch (err) {
        console.error('Failed to fetch energy data:', err);
        setError('Fehler beim Laden der Energiedaten');
      } finally {
        setIsLoading(false);
      }
    }
    
    fetchEnergyData();
  }, []);

  // Loading spinner
  if (isLoading) {
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
    // Get current hour
    const now = new Date();
    const currentHour = now.getHours();
    
    // Find the closest time entry in the data
    const closestEntry = lineChartData.reduce((closest, entry) => {
      const entryHour = parseInt(entry.time.split(':')[0]);
      const currentDiff = Math.abs(currentHour - entryHour);
      const closestDiff = Math.abs(currentHour - parseInt(closest.time.split(':')[0]));
      
      return currentDiff < closestDiff ? entry : closest;
    }, lineChartData[0]);
    
    return `${closestEntry.production.toFixed(1)} kW`;
  })() : '0.0 kW';
  
  const dailyYield = lineChartData.length > 0 ? 
    `${lineChartData.reduce((sum, data) => sum + data.production, 0).toFixed(1)} kWh` : '0.0 kWh';

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
      {/* Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatusCard
          title="Aktuelle Leistung"
          value={currentPower}
          icon="fa-sun"
          iconColor="text-orange-500"
        />
        <StatusCard
          title="Tagesertrag"
          value={dailyYield}
          icon="fa-chart-line"
          iconColor="text-orange-500"
        />
        <StatusCard
          title="Batterie"
          value="85%"
          icon="fa-battery-three-quarters"
          iconColor="text-green-500"
        />
        <StatusCard
          title="CO₂ Einsparung"
          value="2.4t"
          icon="fa-leaf"
          iconColor="text-green-500"
        />
      </div>

      {/* Main Chart Section */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="col-span-1 md:col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Energieproduktion & Verbrauch</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="h-[400px]">
                {lineChartData.length > 0 ? (
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={lineChartData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis dataKey="time" stroke="var(--muted-foreground)" />
                      <YAxis stroke="var(--muted-foreground)" />
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
                      />
                      <Line type="monotone" dataKey="production" stroke="#f97316" strokeWidth={2} />
                      <Line type="monotone" dataKey="consumption" stroke="#3b82f6" strokeWidth={2} />
                      <Line type="monotone" dataKey="feedIn" stroke="#22c55e" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                ) : (
                  <div className="flex items-center justify-center h-full text-muted-foreground">
                    Keine Daten verfügbar
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
        </div>
      </div>

      {/* Weather and Forecast */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <WeatherForecast />
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
    </div>
  );
}
