'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { SolarPanelAnimation } from '@/components/ui/SolarPanelAnimation';

// Types
interface SolarModule {
  modulnummer: number;
  kundennummer: number;
  solarmodultypnummer: number;
  bezeichnung: string;
  umpp: number;
  impp: number;
  pmpp: number;
}

interface Customer {
  kundennummer: number;
  userId: number;
  vorname: string;
  nachname: string;
  strasse: string;
  hausnummer: string;
  postleitzahl: string;
  ort: string;
  email: string;
  telefonnummer: string;
}

export default function AnlagenPage() {
  const [modules, setModules] = useState<SolarModule[]>([]);
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!loading && !user) {
      router.push('/login');
      return;
    }

    async function fetchAnlagen() {
      try {
        setIsLoading(true);
        const response = await fetch('/api/anlagen');
        
        if (!response.ok) {
          if (response.status === 401) {
            router.push('/login');
            return;
          }
          throw new Error(`Error fetching anlagen: ${response.statusText}`);
        }
        
        const data = await response.json();
        setModules(data.modules);
        setCustomer(data.customer);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch anlagen:', err);
        setError('Fehler beim Laden der Anlagendaten');
      } finally {
        setIsLoading(false);
      }
    }
    
    if (user) {
      fetchAnlagen();
    }
  }, [user, loading, router]);

  if (loading || isLoading) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          <p className="mt-4 text-gray-600">Lade Anlagendaten...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="bg-red-50 border border-red-200 text-red-800 rounded-md p-4 mb-6">
          <p>{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto w-full mb-12">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Meine Solaranlagen</h1>
        {customer && (
          <div className="text-sm text-gray-500">
            Kundennummer: {customer.kundennummer}
          </div>
        )}
      </div>
      
      {modules.length === 0 ? (
        <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 rounded-md p-4 mb-6">
          <p>Sie haben noch keine Solarmodule registriert.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {modules.map((module) => (
            <Card key={module.modulnummer} className="overflow-hidden">
              <div className="h-32 bg-gradient-to-r from-blue-500 to-blue-700 flex items-center justify-center">
                <SolarPanelAnimation />
              </div>
              <CardHeader>
                <CardTitle className="flex justify-between items-center">
                  <span>Modul #{module.modulnummer}</span>
                  <span className="text-sm bg-green-100 text-green-800 px-2 py-1 rounded-full">Aktiv</span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-gray-500">Modultyp</p>
                    <p className="font-medium">{module.bezeichnung}</p>
                  </div>
                  
                  <div className="grid grid-cols-3 gap-2">
                    <div>
                      <p className="text-xs text-gray-500">UMPP</p>
                      <p className="font-medium">{module.umpp} V</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">IMPP</p>
                      <p className="font-medium">{module.impp} A</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">PMPP</p>
                      <p className="font-medium">{module.pmpp} W</p>
                    </div>
                  </div>
                  
                  <Button 
                    variant="outline" 
                    className="w-full mt-2"
                    onClick={() => router.push(`/anlagen/${module.modulnummer}`)}
                  >
                    Details anzeigen
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
} 