interface GeocodingResult {
  latitude: number;
  longitude: number;
}

/**
 * Converts an address to coordinates using OpenStreetMap Nominatim API
 * @param street Street name
 * @param houseNumber House number
 * @param postalCode Postal code
 * @param city City name
 * @param country Optional country name, defaults to Germany
 * @returns Promise with the geocoding result (latitude and longitude)
 */
export async function geocodeAddress(
  postalCode: string,
  city: string,
  country: string = 'Germany'
): Promise<GeocodingResult> {
  try {
    // Format the address for geocoding
    const addressQuery = encodeURIComponent(
      `${postalCode} ${city}, ${country}`
    );

    console.log(addressQuery);
    
    // Call the Nominatim API (OpenStreetMap)
    const response = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&q=${addressQuery}&limit=1`,
      {
        headers: {
          'User-Agent': 'power4you-app/1.0', // It's good practice to identify your app
        },
      }
    );
    
    if (!response.ok) {
      throw new Error('Geocoding service error');
    }
    
    const data = await response.json();
    
    if (!data || data.length === 0) {
      throw new Error('Address not found');
    }
    
    return {
      latitude: parseFloat(data[0].lat),
      longitude: parseFloat(data[0].lon),
    };
  } catch (error) {
    console.error('Geocoding error:', error);
    // Return default values if geocoding fails (can be adjusted)
    return {
      latitude: 52.43, // Default to Nordhorn, Germany
      longitude: 7.07,
    };
  }
} 