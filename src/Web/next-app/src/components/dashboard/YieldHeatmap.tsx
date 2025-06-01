'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface HeatmapDataPoint {
  date: string;
  yieldValue: number;
}

interface YieldHeatmapProps {
  data: HeatmapDataPoint[];
  isLoading?: boolean;
}

export default function YieldHeatmap({ data, isLoading }: YieldHeatmapProps) {
  const [hoveredCell, setHoveredCell] = useState<HeatmapDataPoint | null>(null);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });

  // Generate a grid of the last 30 days
  const generateDateGrid = () => {
    const grid: (HeatmapDataPoint | null)[][] = [];
    const today = new Date();
    const startDate = new Date(today);
    startDate.setDate(today.getDate() - 29); // Last 30 days

    // Create a map for quick lookup
    const dataMap = new Map(data.map(d => [d.date, d]));

    // Generate 6 rows (weeks) x 7 columns (days)
    for (let week = 0; week < 6; week++) {
      const weekRow: (HeatmapDataPoint | null)[] = [];
      for (let day = 0; day < 7; day++) {
        const currentDate = new Date(startDate);
        currentDate.setDate(startDate.getDate() + (week * 7) + day);
        
        if (currentDate <= today) {
          const dateStr = currentDate.toISOString().split('T')[0];
          const dataPoint = dataMap.get(dateStr);
          weekRow.push(dataPoint || { date: dateStr, yieldValue: 0 });
        } else {
          weekRow.push(null);
        }
      }
      grid.push(weekRow);
    }

    return grid;
  };

  // Get color intensity based on yieldValue
  const getColorIntensity = (yieldValue: number) => {
    if (yieldValue === 0) return 'bg-gray-100 dark:bg-gray-800';
    
    const maxYield = Math.max(...data.map(d => d.yieldValue));
    const intensity = yieldValue / maxYield;
    
    if (intensity <= 0.2) return 'bg-orange-200 dark:bg-orange-900';
    if (intensity <= 0.4) return 'bg-orange-300 dark:bg-orange-800';
    if (intensity <= 0.6) return 'bg-orange-400 dark:bg-orange-700';
    if (intensity <= 0.8) return 'bg-orange-500 dark:bg-orange-600';
    return 'bg-orange-600 dark:bg-orange-500';
  };

  const handleMouseEnter = (cell: HeatmapDataPoint, event: React.MouseEvent) => {
    setHoveredCell(cell);
    setMousePosition({ x: event.clientX, y: event.clientY });
  };

  const handleMouseLeave = () => {
    setHoveredCell(null);
  };

  const handleMouseMove = (event: React.MouseEvent) => {
    if (hoveredCell) {
      setMousePosition({ x: event.clientX, y: event.clientY });
    }
  };

  const dateGrid = generateDateGrid();
  const dayLabels = ['Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa', 'So'];
  const monthLabels = ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Tagesertrag Heatmap</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32">
            <div className="inline-block h-6 w-6 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Tagesertrag Heatmap (Letzte 30 Tage)</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="relative">
          {/* Month labels */}
          <div className="flex justify-between text-xs text-muted-foreground mb-2">
            {monthLabels.slice(new Date().getMonth() - 1, new Date().getMonth() + 2).map((month, index) => (
              <span key={index}>{month}</span>
            ))}
          </div>

          {/* Heatmap grid */}
          <div className="flex">
            {/* Day labels */}
            <div className="flex flex-col justify-between text-xs text-muted-foreground mr-2 py-1">
              {dayLabels.map((day, index) => (
                <span key={index} className="h-3 flex items-center">
                  {index % 2 === 0 ? day : ''}
                </span>
              ))}
            </div>

            {/* Grid */}
            <div className="grid grid-cols-7 gap-1">
              {dateGrid.flat().map((cell, index) => (
                <div
                  key={index}
                  className={`w-3 h-3 rounded-sm cursor-pointer transition-all duration-200 hover:scale-110 ${
                    cell ? getColorIntensity(cell.yieldValue) : 'bg-transparent'
                  }`}
                  onMouseEnter={cell ? (e) => handleMouseEnter(cell, e) : undefined}
                  onMouseLeave={handleMouseLeave}
                  onMouseMove={handleMouseMove}
                  title={cell ? `${new Date(cell.date).toLocaleDateString('de-DE')}: ${cell.yieldValue.toFixed(1)} kWh` : ''}
                />
              ))}
            </div>
          </div>

          {/* Legend */}
          <div className="flex items-center justify-between mt-4 text-xs text-muted-foreground">
            <span>Weniger</span>
            <div className="flex gap-1">
              <div className="w-3 h-3 bg-gray-100 dark:bg-gray-800 rounded-sm"></div>
              <div className="w-3 h-3 bg-orange-200 dark:bg-orange-900 rounded-sm"></div>
              <div className="w-3 h-3 bg-orange-300 dark:bg-orange-800 rounded-sm"></div>
              <div className="w-3 h-3 bg-orange-400 dark:bg-orange-700 rounded-sm"></div>
              <div className="w-3 h-3 bg-orange-500 dark:bg-orange-600 rounded-sm"></div>
              <div className="w-3 h-3 bg-orange-600 dark:bg-orange-500 rounded-sm"></div>
            </div>
            <span>Mehr</span>
          </div>

          {/* Statistics */}
          <div className="mt-4 grid grid-cols-3 gap-4 text-center">
            <div className="bg-muted p-3 rounded-md">
              <p className="text-muted-foreground text-xs">Gesamt (30 Tage)</p>
              <p className="font-medium text-lg">{data.reduce((sum, d) => sum + d.yieldValue, 0).toFixed(1)} kWh</p>
            </div>
            <div className="bg-muted p-3 rounded-md">
              <p className="text-muted-foreground text-xs">Durchschnitt/Tag</p>
              <p className="font-medium text-lg">{(data.reduce((sum, d) => sum + d.yieldValue, 0) / Math.max(data.length, 1)).toFixed(1)} kWh</p>
            </div>
            <div className="bg-muted p-3 rounded-md">
              <p className="text-muted-foreground text-xs">Bester Tag</p>
              <p className="font-medium text-lg">{Math.max(...data.map(d => d.yieldValue), 0).toFixed(1)} kWh</p>
            </div>
          </div>
        </div>

        {/* Tooltip */}
        {hoveredCell && (
          <div
            className="fixed z-50 bg-black text-white text-xs px-2 py-1 rounded shadow-lg pointer-events-none"
            style={{
              left: mousePosition.x + 10,
              top: mousePosition.y - 30,
            }}
          >
            <div>{new Date(hoveredCell.date).toLocaleDateString('de-DE')}</div>
            <div>{hoveredCell.yieldValue.toFixed(1)} kWh</div>
          </div>
        )}
      </CardContent>
    </Card>
  );
} 