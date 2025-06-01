'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine } from 'recharts';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import Image from 'next/image';

// Types for the module and performance data
interface SolarModule {
  modulnummer: number;
  kundennummer: number;
  solarmodultypnummer: number;
  bezeichnung: string;
  umpp: number;
  impp: number;
  pmpp: number;
}

interface PerformanceDataPoint {
  time: string;
  power: number;
}

interface PerformanceStatistics {
  dailyYield: string;
  currentPower: number;
  maxPower: number;
  avgPower: string;
  efficiency: number;
}

interface ModuleData {
  module: SolarModule;
  performance: {
    data: PerformanceDataPoint[];
    statistics: PerformanceStatistics;
  };
}

interface ClientModulePageProps {
  modulId: string;
}

export default function ClientModulePage({ modulId }: ClientModulePageProps) {
  const [moduleData, setModuleData] = useState<ModuleData | null>(null);
  const [performanceData, setPerformanceData] = useState<PerformanceDataPoint[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [moduleImageOpen, setModuleImageOpen] = useState(false);
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!loading && !user) {
      router.push('/login');
      return;
    }

    async function fetchModuleDetails() {
      try {
        setIsLoading(true);
        
        // Fetch module data from specific module API endpoint
        const response = await fetch(`/api/anlagen/${modulId}`);
        
        if (!response.ok) {
          if (response.status === 401) {
            router.push('/login');
            return;
          }
          throw new Error(`Error fetching module details: ${response.statusText}`);
        }
        
        const data = await response.json();
        setModuleData(data);
        
        // Update performance data for the chart
        if (data.performance && data.performance.data.length > 0) {
          console.log('Performance data points:', data.performance.data.length);
          setPerformanceData(data.performance.data);
          
          // If only one data point, duplicate it with a different time to ensure the chart displays properly
          if (data.performance.data.length === 1) {
            const point = data.performance.data[0];
            const fakePoint = { 
              ...point, 
              time: point.time.includes(':30') ? 
                point.time.replace(':30', ':00') : 
                point.time.replace(':00', ':30') 
            };
            setPerformanceData([point, fakePoint]);
          }
        }
        
        setError(null);
      } catch (err) {
        console.error('Failed to fetch module details:', err);
        setError(err instanceof Error ? err.message : 'Fehler beim Laden der Moduldaten');
      } finally {
        setIsLoading(false);
      }
    }
    
    if (user) {
      fetchModuleDetails();
    }
  }, [user, loading, router, modulId]);

  const handleBack = () => {
    router.push('/anlagen');
  };

  if (loading || isLoading) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          <p className="mt-4 text-muted-foreground">Lade Moduldaten...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="bg-red-50 border border-red-200 text-red-800 dark:bg-red-900 dark:border-red-800 dark:text-red-100 rounded-md p-4 mb-6">
          <p>{error}</p>
        </div>
        <Button variant="outline" onClick={handleBack}>
          Zurück zur Übersicht
        </Button>
      </div>
    );
  }

  if (!moduleData || !moduleData.module) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 dark:bg-yellow-900 dark:border-yellow-800 dark:text-yellow-100 rounded-md p-4 mb-6">
          <p>Modul nicht gefunden</p>
        </div>
        <Button variant="outline" onClick={handleBack}>
          Zurück zur Übersicht
        </Button>
      </div>
    );
  }

  const { module, performance } = moduleData;
  const { statistics } = performance || { statistics: { 
    dailyYield: '0', 
    currentPower: 0, 
    maxPower: 0, 
    avgPower: '0', 
    efficiency: 0 
  }};

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Modul #{module.modulnummer}</h1>
        <Button variant="outline" onClick={handleBack}>
          Zurück zur Übersicht
        </Button>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Module Information */}
        <Card className="md:col-span-1">
          <CardHeader>
            <CardTitle>Modulinformationen</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">Modultyp</p>
                <button
                  onClick={() => setModuleImageOpen(true)}
                  className="font-medium text-primary hover:underline focus:outline-none"
                >
                  {module.bezeichnung}
                </button>
              </div>
              
              <div>
                <p className="text-sm text-muted-foreground">Technische Spezifikationen</p>
                <div className="mt-2 space-y-2">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">UMPP:</span>
                    <span className="font-medium">{module.umpp} V</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">IMPP:</span>
                    <span className="font-medium">{module.impp} A</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">PMPP:</span>
                    <span className="font-medium">{module.pmpp} W</span>
                  </div>
                </div>
              </div>
              
              <div>
                <p className="text-sm text-muted-foreground">Status</p>
                <div className="mt-1">
                  <span className="text-sm bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100 px-2 py-1 rounded-full">
                    Aktiv
                  </span>
                </div>
              </div>
              
              <div>
                <p className="text-sm text-muted-foreground">Installation</p>
                <p className="font-medium">20. Mai 2023</p>
              </div>
            </div>
          </CardContent>
        </Card>
        
        {/* Performance Chart */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Aktuelle Leistung</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-[300px]">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={performanceData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis 
                    dataKey="time" 
                    padding={{ left: 10, right: 10 }}
                    tickMargin={10}
                    label={undefined}
                    stroke="var(--muted-foreground)"
                  />
                  <YAxis 
                    label={{ value: 'Leistung (W)', angle: -90, position: 'insideLeft', style: { textAnchor: 'middle' } }}
                    domain={[0, 'auto']}
                    stroke="var(--muted-foreground)"
                  />
                  <Tooltip 
                    contentStyle={{
                      backgroundColor: 'var(--card)',
                      color: 'var(--card-foreground)',
                      borderRadius: '4px',
                      padding: '8px',
                      border: '1px solid var(--border)'
                    }} 
                    formatter={(value) => [`${value} W`, 'Leistung']}
                    labelFormatter={(label) => `Zeit: ${label}`}
                  />
                  {performanceData.length > 0 && statistics.currentPower > 0 && (
                    <ReferenceLine 
                      y={statistics.currentPower} 
                      stroke="#f59e0b" 
                      strokeDasharray="3 3"
                      label={{ 
                        value: 'Aktuelle Leistung', 
                        position: 'right',
                        fill: 'var(--primary)',
                        fontSize: 12
                      }} 
                    />
                  )}
                  <Line 
                    type="monotone" 
                    dataKey="power" 
                    stroke="#f97316" 
                    strokeWidth={2}
                    dot={{ r: 4 }}
                    activeDot={{ r: 6 }}
                    connectNulls
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
            <div className="mt-4 grid grid-cols-3 gap-4 text-center">
              <div className="bg-muted p-3 rounded-md">
                <p className="text-muted-foreground text-xs">Tagesertrag</p>
                <p className="font-medium text-lg">{statistics.dailyYield} kWh</p>
              </div>
              <div className="bg-muted p-3 rounded-md">
                <p className="text-muted-foreground text-xs">Aktuelle Leistung</p>
                <p className="font-medium text-lg">{statistics.currentPower} W</p>
              </div>
              <div className="bg-muted p-3 rounded-md">
                <p className="text-muted-foreground text-xs">Effizienz</p>
                <p className="font-medium text-lg">{statistics.efficiency}%</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Solar Module Image Dialog */}
      <Dialog open={moduleImageOpen} onOpenChange={setModuleImageOpen}>
        <DialogContent className="sm:max-w-4xl max-h-[90vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>{module.bezeichnung}</DialogTitle>
          </DialogHeader>
          <div className="relative w-full min-h-[400px] max-h-[70vh] overflow-hidden rounded-lg">
            <Image
              src={`/images/solar-modules/${module.solarmodultypnummer}.png`}
              alt={`Foto von ${module.bezeichnung}`}
              fill
              className="object-contain"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = "/images/solar-modules/default.png";
              }}
            />
          </div>
          <div className="text-sm text-muted-foreground mt-2">
            <p>Modellnummer: {module.solarmodultypnummer}</p>
            <p>Nennleistung: {module.pmpp} W</p>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
} 