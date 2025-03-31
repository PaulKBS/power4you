'use client';

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import StatusCard from '@/components/dashboard/StatusCard';
import WeatherForecast from '@/components/dashboard/WeatherForecast';

const lineChartData = [
  { time: '06:00', production: 0.5, consumption: 1.2, feedIn: 0 },
  { time: '08:00', production: 2.1, consumption: 1.8, feedIn: 0.3 },
  { time: '10:00', production: 4.3, consumption: 2.1, feedIn: 2.2 },
  { time: '12:00', production: 5.2, consumption: 2.4, feedIn: 2.8 },
  { time: '14:00', production: 4.8, consumption: 2.8, feedIn: 2.0 },
  { time: '16:00', production: 3.1, consumption: 3.1, feedIn: 0 },
  { time: '18:00', production: 1.2, consumption: 3.5, feedIn: 0 },
  { time: '20:00', production: 0.1, consumption: 2.2, feedIn: 0 },
];

const pieChartData = [
  { name: 'Eigenverbrauch', value: 45 },
  { name: 'Einspeisung', value: 35 },
  { name: 'Batterieladung', value: 20 },
];

const COLORS = ['#f97316', '#22c55e', '#3b82f6'];

export default function Home() {
  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
      {/* Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatusCard
          title="Aktuelle Leistung"
          value="4.2 kW"
          icon="fa-sun"
          iconColor="text-orange-500"
        />
        <StatusCard
          title="Tagesertrag"
          value="28.5 kWh"
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
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={lineChartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="time" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="production" stroke="#f97316" strokeWidth={2} />
                    <Line type="monotone" dataKey="consumption" stroke="#3b82f6" strokeWidth={2} />
                    <Line type="monotone" dataKey="feedIn" stroke="#22c55e" strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
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
                  <span className="text-gray-700">Produktion</span>
                </li>
                <li className="flex items-center space-x-3">
                  <span className="w-4 h-4 bg-blue-500 rounded-full"></span>
                  <span className="text-gray-700">Verbrauch</span>
                </li>
                <li className="flex items-center space-x-3">
                  <span className="w-4 h-4 bg-green-500 rounded-full"></span>
                  <span className="text-gray-700">Einspeisung</span>
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
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
