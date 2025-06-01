import { NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { db } from '@/lib/db';
import { eq, and, desc } from 'drizzle-orm';
import { kunde, solarmodul, solarmodultyp, leistung } from '../../../../../drizzle/schema';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

export async function GET(
  request: Request,
  { params }: { params: Promise<{ modulId: string }> }
) {
  try {
    const modulId = parseInt((await params).modulId);
    
    if (isNaN(modulId)) {
      return NextResponse.json(
        { message: 'Invalid module ID' },
        { status: 400 }
      );
    }
    
    // Parse the cookie from the request headers
    const cookieHeader = request.headers.get('cookie') || '';
    const cookies = Object.fromEntries(
      cookieHeader.split('; ').map(c => {
        const [name, ...value] = c.split('=');
        return [name, value.join('=')];
      })
    );
    const token = cookies['auth-token'];
    
    if (!token) {
      return NextResponse.json(
        { message: 'Not authenticated' },
        { status: 401 }
      );
    }

    try {
      // Verify the token
      const { payload } = await jwtVerify(
        token,
        new TextEncoder().encode(JWT_SECRET)
      );
      
      // Get the user ID from the token and ensure it's a number
      const userId = Number(payload.userId);
      
      if (isNaN(userId)) {
        return NextResponse.json(
          { message: 'Invalid user ID' },
          { status: 400 }
        );
      }
      
      // First, get the customer data for the user
      const customerData = await db.select().from(kunde).where(eq(kunde.userId, userId));
      
      if (customerData.length === 0) {
        return NextResponse.json(
          { message: 'No customer data found for this user' },
          { status: 404 }
        );
      }
      
      const kundennummer = customerData[0].kundennummer;
      
      // Verify that the module belongs to this customer
      const moduleData = await db
        .select()
        .from(solarmodul)
        .where(
          and(
            eq(solarmodul.modulnummer, modulId),
            eq(solarmodul.kundennummer, kundennummer)
          )
        );
      
      if (moduleData.length === 0) {
        return NextResponse.json(
          { message: 'Module not found or does not belong to this customer' },
          { status: 404 }
        );
      }
      
      // Get the module type information
      const moduleWithType = await db
        .select({
          modulnummer: solarmodul.modulnummer,
          kundennummer: solarmodul.kundennummer,
          solarmodultypnummer: solarmodul.solarmodultypnummer,
          bezeichnung: solarmodultyp.bezeichnung,
          umpp: solarmodultyp.umpp,
          impp: solarmodultyp.impp,
          pmpp: solarmodultyp.pmpp
        })
        .from(solarmodul)
        .innerJoin(
          solarmodultyp,
          eq(solarmodul.solarmodultypnummer, solarmodultyp.solarmodultypnummer)
        )
        .where(eq(solarmodul.modulnummer, modulId));
      
      // Get recent performance data for this module
      const performanceData = await db
        .select()
        .from(leistung)
        .where(eq(leistung.modulnummer, modulId))
        .orderBy(desc(leistung.timestamp))
        .limit(24); // Get last 24 records
      
      // Format performance data for the chart
      const formattedPerformanceData = performanceData
        .map(entry => {
          const timestamp = new Date(entry.timestamp);
          return {
            time: `${timestamp.getHours()}:${timestamp.getMinutes().toString().padStart(2, '0')}`,
            power: entry.powerOut,
            timestamp: timestamp.getTime(), // Add raw timestamp for sorting
          };
        })
        .sort((a, b) => a.timestamp - b.timestamp) // Sort chronologically, oldest first
        .map(({ time, power }) => ({ time, power })); // Remove the timestamp property
      
      // Calculate some statistics
      const totalPower = performanceData.reduce((sum, entry) => sum + entry.powerOut, 0);
      const avgPower = performanceData.length > 0 ? totalPower / performanceData.length : 0;
      const maxPower = performanceData.length > 0 
        ? Math.max(...performanceData.map(entry => entry.powerOut)) 
        : 0;
      
      // Get the most recent power reading
      const currentPower = performanceData.length > 0 ? performanceData[0].powerOut : 0;
      
      return NextResponse.json({
        module: moduleWithType[0],
        performance: {
          data: formattedPerformanceData,
          statistics: {
            dailyYield: (totalPower / 1000).toFixed(2), // kWh
            currentPower,
            maxPower,
            avgPower: avgPower.toFixed(0),
            efficiency: Math.min(100, Math.round((currentPower / moduleWithType[0].pmpp) * 100)) || 0
          }
        }
      });
      
    } catch (jwtError) {
      console.error('JWT verification error:', jwtError);
      return NextResponse.json(
        { message: 'Invalid authentication token' },
        { status: 401 }
      );
    }
  } catch (error) {
    console.error('Error fetching module data:', error);
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    );
  }
} 