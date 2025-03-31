interface WeatherDay {
  day: string;
  icon: string;
  temperature: string;
  iconColor: string;
}

export default function WeatherForecast() {
  const weatherData: WeatherDay[] = [
    { day: 'Heute', icon: 'fa-sun', temperature: '22°C', iconColor: 'text-yellow-500' },
    { day: 'Mo', icon: 'fa-cloud-sun', temperature: '19°C', iconColor: 'text-gray-500' },
    { day: 'Di', icon: 'fa-cloud', temperature: '18°C', iconColor: 'text-gray-500' },
    { day: 'Mi', icon: 'fa-sun', temperature: '23°C', iconColor: 'text-yellow-500' },
  ];

  return (
    <div className="bg-white p-4 rounded-lg shadow-sm">
      <h3 className="text-lg font-medium text-gray-900 mb-4">Wettervorhersage</h3>
      <div className="grid grid-cols-4 gap-4">
        {weatherData.map((day, index) => (
          <div key={index} className="text-center">
            <p className="text-gray-500">{day.day}</p>
            <i className={`fas ${day.icon} text-3xl ${day.iconColor} my-2`}></i>
            <p className="text-sm">{day.temperature}</p>
          </div>
        ))}
      </div>
    </div>
  );
} 