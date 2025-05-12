import { useState, useEffect } from 'react';
import { geocodeAddress } from '@/lib/geocoding';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

interface WeatherDay {
  day: string;
  date: string; // ISO date string
  icon: string;
  temperature: string;
  iconColor: string;
  weather_code: number;
  description: string;
  humidity?: number;
  wind_speed?: number;
  precipitation?: number;
  feels_like?: string;
}

interface HourlyData {
  time: string;
  temperature: number;
  precipitation: number;
  humidity: number;
  wind_speed: number;
}

interface HourlyWeatherData {
  time: string[];
  temperature_2m: number[];
  precipitation: number[];
  relative_humidity_2m: number[];
  windspeed_10m: number[];
}

interface WeatherCode {
  code: number;
  description: string;
  icon: React.ReactNode;
}

interface CustomerData {
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

export default function WeatherForecast() {
  const [weatherData, setWeatherData] = useState<WeatherDay[]>([]);
  const [hourlyData, setHourlyData] = useState<HourlyData[]>([]);
  const [rawHourlyData, setRawHourlyData] = useState<HourlyWeatherData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [locationName, setLocationName] = useState<string>('');
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [selectedDayData, setSelectedDayData] = useState<WeatherDay | null>(null);
  const [forecastDays, setForecastDays] = useState(4);
  const [activeHourlyChart, setActiveHourlyChart] = useState<'temperature' | 'precipitation'>('temperature');

  useEffect(() => {
    // Fetch customer data first
    const fetchCustomerData = async () => {
      try {
        const response = await fetch('/api/customer');
        if (!response.ok) {
          if (response.status === 404) {
            // If no customer data found, use default location
            fetchWeatherForLocation(52.43, 7.07, 'Nordhorn');
            return;
          }
          throw new Error('Failed to fetch customer data');
        }
        
        const data: CustomerData = await response.json();
        
        // Get coordinates from customer address
        const { latitude, longitude } = await geocodeAddress(
          data.strasse,
          data.hausnummer,
          data.postleitzahl,
          data.ort
        );
        
        // Fetch weather using customer coordinates
        fetchWeatherForLocation(latitude, longitude, data.ort);
      } catch (err) {
        console.error('Error fetching customer data:', err);
        // Fallback to default location
        fetchWeatherForLocation(52.43, 7.07, 'Nordhorn');
      }
    };
    
    fetchCustomerData();
  }, []);
  
  const fetchWeatherForLocation = async (latitude: number, longitude: number, location: string) => {
    try {
      setLocationName(location);
      
      const response = await fetch(
        `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}&daily=weathercode,temperature_2m_max,temperature_2m_min,apparent_temperature_max,precipitation_sum,windspeed_10m_max,relative_humidity_2m_max&hourly=temperature_2m,precipitation,relative_humidity_2m,windspeed_10m&timezone=Europe/Berlin&forecast_days=${forecastDays}`
      );
      
      if (!response.ok) {
        throw new Error('Failed to fetch weather data');
      }
      
      const data = await response.json();
      
      // Store raw hourly data
      setRawHourlyData(data.hourly);
      
      // Process the daily data
      const processedData: WeatherDay[] = [];
      const daysOfWeek = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
      
      // Map weather codes to icons and descriptions
      for (let i = 0; i < data.daily.time.length; i++) {
        const maxTemp = Math.round(data.daily.temperature_2m_max[i]);
        const date = new Date(data.daily.time[i]);
        const dayLabel = i === 0 ? 'Heute' : daysOfWeek[date.getDay()];
        const weatherCode = data.daily.weathercode[i];
        const feelsLike = Math.round(data.daily.apparent_temperature_max[i]);
        const precipitation = data.daily.precipitation_sum[i];
        
        processedData.push({
          day: dayLabel,
          date: data.daily.time[i],
          icon: getIconForWeatherCode(weatherCode),
          temperature: `${maxTemp}°C`,
          iconColor: '',
          weather_code: weatherCode,
          description: getWeatherDescription(weatherCode),
          humidity: data.daily.relative_humidity_2m_max[i],
          wind_speed: data.daily.windspeed_10m_max[i],
          precipitation: precipitation,
          feels_like: `${feelsLike}°C`,
        });
      }
      
      setWeatherData(processedData);
      
      // Set the first day as selected by default
      if (processedData.length > 0) {
        setSelectedDate(processedData[0].date);
        setSelectedDayData(processedData[0]);
        // Process hourly data for the first day
        processHourlyData(data.hourly, processedData[0].date);
      }
      setLoading(false);
    } catch (err) {
      console.error('Error fetching weather data:', err);
      setError('Wetterdaten konnten nicht geladen werden');
      setLoading(false);
    }
  };
  
  const processHourlyData = (hourlyData: HourlyWeatherData, selectedDate: string) => {
    const startDate = new Date(selectedDate);
    const endDate = new Date(selectedDate);
    endDate.setDate(endDate.getDate() + 1);

    const filteredData = hourlyData.time
      .map((time: string, index: number) => {
        const currentDate = new Date(time);
        if (currentDate >= startDate && currentDate < endDate) {
          return {
            time: new Date(time).toLocaleTimeString('de-DE', { hour: '2-digit', minute: '2-digit' }),
            temperature: Math.round(hourlyData.temperature_2m[index]),
            precipitation: hourlyData.precipitation[index],
            humidity: hourlyData.relative_humidity_2m[index],
            wind_speed: hourlyData.windspeed_10m[index]
          };
        }
        return null;
      })
      .filter((item: HourlyData | null) => item !== null);

    setHourlyData(filteredData as HourlyData[]);
  };

  // Map WMO weather codes to icons
  function getIconForWeatherCode(code: number): string {
    // Clear
    if (code === 0) {
      return 'sunny';
    }
    // Mainly clear, partly cloudy
    else if (code === 1 || code === 2) {
      return 'partly-cloudy';
    }
    // Overcast
    else if (code === 3) {
      return 'cloudy';
    }
    // Fog
    else if (code >= 45 && code <= 48) {
      return 'fog';
    }
    // Drizzle
    else if (code >= 51 && code <= 57) {
      return 'light-rain';
    }
    // Rain
    else if ((code >= 61 && code <= 67) || (code >= 80 && code <= 82)) {
      return 'rain';
    }
    // Snow
    else if ((code >= 71 && code <= 77) || (code >= 85 && code <= 86)) {
      return 'snow';
    }
    // Thunderstorm
    else if (code >= 95 && code <= 99) {
      return 'thunderstorm';
    }
    // Default
    else {
      return 'cloudy';
    }
  }

  // Get weather description based on WMO weather code
  function getWeatherDescription(code: number): string {
    if (code === 0) return 'Klarer Himmel';
    if (code === 1) return 'Überwiegend klar';
    if (code === 2) return 'Teilweise bewölkt';
    if (code === 3) return 'Bedeckt';
    if (code >= 45 && code <= 48) return 'Nebel';
    if (code >= 51 && code <= 57) return 'Nieselregen';
    if (code >= 61 && code <= 67) return 'Regen';
    if (code >= 71 && code <= 77) return 'Schneefall';
    if (code >= 80 && code <= 82) return 'Regenschauer';
    if (code >= 85 && code <= 86) return 'Schneeschauer';
    if (code >= 95 && code <= 99) return 'Gewitter';
    return 'Unbekannt';
  }
  
  // Map WMO weather codes to FontAwesome icons and colors
  function getWeatherInfo(code: number): WeatherCode {
    // Clear
    if (code === 0) {
      return { 
        code,
        description: 'Clear sky',
        icon: <SunnyIcon />
      };
    }
    // Mainly clear, partly cloudy
    else if (code === 1 || code === 2) {
      return { 
        code,
        description: 'Partly cloudy',
        icon: <PartlyCloudyIcon />
      };
    }
    // Overcast
    else if (code === 3) {
      return { 
        code,
        description: 'Overcast',
        icon: <CloudyIcon />
      };
    }
    // Fog
    else if (code >= 45 && code <= 48) {
      return { 
        code,
        description: 'Fog',
        icon: <FogIcon />
      };
    }
    // Drizzle
    else if (code >= 51 && code <= 57) {
      return { 
        code,
        description: 'Drizzle',
        icon: <LightRainIcon />
      };
    }
    // Rain
    else if ((code >= 61 && code <= 67) || (code >= 80 && code <= 82)) {
      return { 
        code,
        description: 'Rain',
        icon: <RainIcon />
      };
    }
    // Snow
    else if ((code >= 71 && code <= 77) || (code >= 85 && code <= 86)) {
      return { 
        code,
        description: 'Snow',
        icon: <SnowIcon />
      };
    }
    // Thunderstorm
    else if (code >= 95 && code <= 99) {
      return { 
        code,
        description: 'Thunderstorm',
        icon: <ThunderstormIcon />
      };
    }
    // Default
    else {
      return { 
        code,
        description: 'Unknown',
        icon: <CloudyIcon />
      };
    }
  }

  const handleDateSelect = (date: string) => {
    setSelectedDate(date);
    const dayData = weatherData.find(day => day.date === date);
    if (dayData && rawHourlyData) {
      setSelectedDayData(dayData);
      processHourlyData(rawHourlyData, date);
    }
  };

  const loadMoreDays = async () => {
    const newDays = forecastDays + 3;
    setForecastDays(newDays);
    
    // Store current selected date to maintain selection
    const currentSelectedDate = selectedDate;
    
    // Re-fetch with more days
    if (weatherData.length > 0) {
      try {
        const response = await fetch(
          `https://api.open-meteo.com/v1/forecast?latitude=52.43&longitude=7.07&daily=weathercode,temperature_2m_max,temperature_2m_min,apparent_temperature_max,precipitation_sum,windspeed_10m_max,relative_humidity_2m_max&hourly=temperature_2m,precipitation,relative_humidity_2m,windspeed_10m&timezone=Europe/Berlin&forecast_days=${newDays}`
        );
        
        if (!response.ok) {
          throw new Error('Failed to fetch weather data');
        }
        
        const data = await response.json();
        
        // Store raw hourly data
        setRawHourlyData(data.hourly);
        
        // Process the daily data
        const processedData: WeatherDay[] = [];
        const daysOfWeek = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
        
        for (let i = 0; i < data.daily.time.length; i++) {
          const maxTemp = Math.round(data.daily.temperature_2m_max[i]);
          const date = new Date(data.daily.time[i]);
          const dayLabel = i === 0 ? 'Heute' : daysOfWeek[date.getDay()];
          const weatherCode = data.daily.weathercode[i];
          const feelsLike = Math.round(data.daily.apparent_temperature_max[i]);
          const precipitation = data.daily.precipitation_sum[i];
          
          processedData.push({
            day: dayLabel,
            date: data.daily.time[i],
            icon: getIconForWeatherCode(weatherCode),
            temperature: `${maxTemp}°C`,
            iconColor: '',
            weather_code: weatherCode,
            description: getWeatherDescription(weatherCode),
            humidity: data.daily.relative_humidity_2m_max[i],
            wind_speed: data.daily.windspeed_10m_max[i],
            precipitation: precipitation,
            feels_like: `${feelsLike}°C`,
          });
        }
        
        setWeatherData(processedData);
        
        // Maintain the selected date if it exists in the new data
        if (currentSelectedDate) {
          const dayData = processedData.find(day => day.date === currentSelectedDate);
          if (dayData) {
            setSelectedDayData(dayData);
            processHourlyData(data.hourly, currentSelectedDate);
          }
        }
      } catch (err) {
        console.error('Error fetching additional weather data:', err);
        setError('Zusätzliche Wetterdaten konnten nicht geladen werden');
      }
    }
  };

  // SVG Weather Icons Components
  const SunnyIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-10 h-10 text-yellow-500">
      <circle cx="12" cy="12" r="5" />
      <line x1="12" y1="3" x2="12" y2="1" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="12" y1="23" x2="12" y2="21" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="1" y1="12" x2="3" y2="12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="21" y1="12" x2="23" y2="12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );

  const PartlyCloudyIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-gray-600 dark:text-gray-300">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <circle cx="8" cy="9" r="3" fill="#fbbf24" stroke="none" />
    </svg>
  );

  const CloudyIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-gray-500 dark:text-gray-300">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
    </svg>
  );

  const FogIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-gray-500 dark:text-gray-300">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <line x1="5" y1="12" x2="19" y2="12" strokeWidth="1" strokeDasharray="1 3" />
      <line x1="3" y1="15" x2="21" y2="15" strokeWidth="1" strokeDasharray="1 3" />
      <line x1="7" y1="18" x2="17" y2="18" strokeWidth="1" strokeDasharray="1 3" />
    </svg>
  );

  const LightRainIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-blue-400">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <line x1="8" y1="16" x2="7" y2="20" strokeWidth="1" />
      <line x1="12" y1="16" x2="11" y2="20" strokeWidth="1" />
      <line x1="16" y1="16" x2="15" y2="20" strokeWidth="1" />
    </svg>
  );

  const RainIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-blue-500">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <line x1="8" y1="16" x2="6" y2="22" strokeWidth="1.5" />
      <line x1="12" y1="16" x2="10" y2="22" strokeWidth="1.5" />
      <line x1="16" y1="16" x2="14" y2="22" strokeWidth="1.5" />
    </svg>
  );

  const SnowIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-blue-200 dark:text-blue-300">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <circle cx="8" cy="18" r="1" fill="currentColor" />
      <circle cx="12" cy="18" r="1" fill="currentColor" />
      <circle cx="16" cy="18" r="1" fill="currentColor" />
      <circle cx="10" cy="21" r="1" fill="currentColor" />
      <circle cx="14" cy="21" r="1" fill="currentColor" />
    </svg>
  );

  const ThunderstormIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="w-10 h-10 text-purple-500">
      <path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" fill="var(--muted)" strokeWidth="1" />
      <path d="M12 14l-2 4h4l-2 4" fill="#a855f7" stroke="#a855f7" strokeWidth="0.5" />
    </svg>
  );

  if (loading) {
    return (
      <div className="bg-card text-card-foreground p-4 rounded-lg shadow-sm">
        <h3 className="text-lg font-medium mb-4">Wettervorhersage</h3>
        <div className="flex justify-center items-center h-24">
          <p>Lade Wetterdaten...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-card text-card-foreground p-4 rounded-lg shadow-sm">
        <h3 className="text-lg font-medium mb-4">Wettervorhersage</h3>
        <div className="flex justify-center items-center h-24">
          <p className="text-red-500">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-card text-card-foreground p-4 rounded-lg shadow-sm">
      <h3 className="text-lg font-medium mb-4">Wettervorhersage für {locationName}</h3>
      
      {/* Weather day selection */}
      <div className="grid grid-cols-4 gap-4 mb-6">
        {weatherData.map((day, index) => (
          <div 
            key={index} 
            className={`text-center p-2 rounded-lg cursor-pointer transition-colors ${
              selectedDate === day.date ? 'bg-blue-100 border border-blue-300 dark:bg-blue-900 dark:border-blue-700' : 'hover:bg-muted'
            }`}
            onClick={() => handleDateSelect(day.date)}
          >
            <p className="text-muted-foreground">{day.day}</p>
            <div className="flex justify-center my-2">
              {getWeatherInfo(day.weather_code).icon}
            </div>
            <p className="text-sm">{day.temperature}</p>
          </div>
        ))}
      </div>

      {/* Show more days button */}
      {forecastDays < 10 && (
        <div className="flex justify-center mb-4">
          <button 
            onClick={loadMoreDays}
            className="text-sm text-blue-600 hover:underline dark:text-blue-400"
          >
            Mehr Tage anzeigen
          </button>
        </div>
      )}
      
      {/* Detailed weather information */}
      {selectedDayData && (
        <div className="border-t pt-4 mt-2">
          <h4 className="text-md font-medium mb-3">Details für {selectedDayData.day} ({new Date(selectedDayData.date).toLocaleDateString('de-DE')})</h4>
          
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-muted p-3 rounded-lg">
              <p className="text-xs text-muted-foreground">Beschreibung</p>
              <p className="font-medium">{selectedDayData.description}</p>
            </div>
            
            <div className="bg-muted p-3 rounded-lg">
              <p className="text-xs text-muted-foreground">Gefühlte Temperatur</p>
              <p className="font-medium">{selectedDayData.feels_like}</p>
            </div>
            
            <div className="bg-muted p-3 rounded-lg">
              <p className="text-xs text-muted-foreground">Luftfeuchtigkeit</p>
              <p className="font-medium">{selectedDayData.humidity}%</p>
            </div>
            
            <div className="bg-muted p-3 rounded-lg">
              <p className="text-xs text-muted-foreground">Windgeschwindigkeit</p>
              <p className="font-medium">{selectedDayData.wind_speed} km/h</p>
            </div>
            
            <div className="bg-muted p-3 rounded-lg">
              <p className="text-xs text-muted-foreground">Niederschlag</p>
              <p className="font-medium">{selectedDayData.precipitation} mm</p>
            </div>
          </div>

          {/* Hourly weather charts */}
          <div className="mt-6">
            <div className="flex justify-between items-center mb-4">
              <h4 className="text-md font-medium">Stündliche Vorhersage</h4>
              <div className="flex space-x-2">
                <button 
                  className={`px-3 py-1 rounded-md text-sm ${activeHourlyChart === 'temperature' ? 'bg-blue-500 text-white' : 'bg-muted text-muted-foreground'}`}
                  onClick={() => setActiveHourlyChart('temperature')}
                >
                  Temperatur
                </button>
                <button 
                  className={`px-3 py-1 rounded-md text-sm ${activeHourlyChart === 'precipitation' ? 'bg-blue-500 text-white' : 'bg-muted text-muted-foreground'}`}
                  onClick={() => setActiveHourlyChart('precipitation')}
                >
                  Niederschlag
                </button>
              </div>
            </div>
            
            <div className="h-64">
              {activeHourlyChart === 'temperature' ? (
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart
                    data={hourlyData}
                    margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                    <XAxis dataKey="time" stroke="var(--muted-foreground)" />
                    <YAxis unit="°C" stroke="var(--muted-foreground)" />
                    <Tooltip 
                      contentStyle={{
                        backgroundColor: 'var(--card)',
                        color: 'var(--card-foreground)',
                        borderRadius: '4px',
                        padding: '8px',
                        border: '1px solid var(--border)'
                      }}
                      formatter={(value) => [`${value}°C`]} 
                    />
                    <Legend />
                    <Line 
                      type="monotone" 
                      dataKey="temperature" 
                      name="Temperatur" 
                      stroke="#f97316" 
                      activeDot={{ r: 8 }} 
                    />
                  </LineChart>
                </ResponsiveContainer>
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={hourlyData}
                    margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                    <XAxis dataKey="time" stroke="var(--muted-foreground)" />
                    <YAxis unit="mm" stroke="var(--muted-foreground)" />
                    <Tooltip 
                      contentStyle={{
                        backgroundColor: 'var(--card)',
                        color: 'var(--card-foreground)',
                        borderRadius: '4px',
                        padding: '8px',
                        border: '1px solid var(--border)'
                      }}
                      formatter={(value) => [`${value} mm`]} 
                    />
                    <Legend />
                    <Bar 
                      dataKey="precipitation" 
                      name="Niederschlag" 
                      fill="#3b82f6" 
                    />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
} 