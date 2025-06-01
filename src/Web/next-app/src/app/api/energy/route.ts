import { NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { db } from '@/lib/db';
import { eq, sql } from 'drizzle-orm';
import { leistung, solarmodul, kunde } from '../../../../drizzle/schema';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

export async function GET(request: Request) {
  try {
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

      // Parse query parameters for date range
      const url = new URL(request.url);
      const startDate = url.searchParams.get('startDate');
      const endDate = url.searchParams.get('endDate');
      const view = url.searchParams.get('view') || 'hourly'; // hourly, daily, heatmap

      // Get modules for the current customer
      const customerModules = await db
        .select({
          modulnummer: solarmodul.modulnummer
        })
        .from(solarmodul)
        .innerJoin(kunde, eq(solarmodul.kundennummer, kunde.kundennummer))
        .where(eq(kunde.userId, userId));

      if (customerModules.length === 0) {
        return NextResponse.json(
          { message: 'No solar modules found for this user' },
          { status: 404 }
        );
      }

      // Get moduleIds for the query
      const moduleIds = customerModules.map(module => module.modulnummer);

      // Default to last 24 hours if no date range specified
      let dateCondition;
      if (startDate && endDate) {
        // If it's the same date (like "heute"), we need to include the entire day
        if (startDate === endDate) {
          dateCondition = sql`DATE(${leistung.timestamp}) = ${startDate}`;
        } else {
          dateCondition = sql`${leistung.timestamp} >= ${startDate} AND ${leistung.timestamp} <= DATE_ADD(${endDate}, INTERVAL 1 DAY)`;
        }
      } else {
        dateCondition = sql`${leistung.timestamp} >= DATE_SUB(NOW(), INTERVAL 24 HOUR)`;
      }

      if (view === 'heatmap') {
        // Get daily yield data for heatmap (last 30 days or specified range)
        const heatmapStartDate = startDate || sql`DATE_SUB(NOW(), INTERVAL 30 DAY)`;
        const heatmapEndDate = endDate || sql`NOW()`;
        
        const dailyYieldData = await db
          .select({
            date: sql`DATE(${leistung.timestamp})`,
            yield: sql`SUM(${leistung.powerOut}) / 1000`, // Convert to kWh
          })
          .from(leistung)
          .where(sql`${leistung.modulnummer} IN (${moduleIds.join(',')}) 
                  AND ${leistung.timestamp} >= ${heatmapStartDate}
                  AND ${leistung.timestamp} <= ${heatmapEndDate}`)
          .groupBy(sql`DATE(${leistung.timestamp})`)
          .orderBy(sql`DATE(${leistung.timestamp})`);

        const formattedHeatmapData = dailyYieldData.map(day => ({
          date: day.date,
          yieldValue: parseFloat(Number(day.yield).toFixed(2))
        }));

        return NextResponse.json({
          heatmapData: formattedHeatmapData
        });
      }

      if (view === 'daily') {
        // Get daily aggregated data
        const dailyData = await db
          .select({
            date: sql`DATE(${leistung.timestamp})`,
            production: sql`SUM(${leistung.powerOut}) / 1000`, // Convert to kWh
          })
          .from(leistung)
          .where(sql`${leistung.modulnummer} IN (${moduleIds.join(',')}) 
                  AND ${dateCondition}`)
          .groupBy(sql`DATE(${leistung.timestamp})`)
          .orderBy(sql`DATE(${leistung.timestamp})`);

        const formattedDailyData = dailyData.map(day => {
          const production = Number(day.production);
          const consumption = production * (0.7 + Math.random() * 0.1);
          const feedIn = Math.max(0, production - consumption);
          
          return {
            time: day.date,
            production: parseFloat(production.toFixed(1)),
            consumption: parseFloat(consumption.toFixed(1)),
            feedIn: parseFloat(feedIn.toFixed(1))
          };
        });

        // Calculate pie chart data for the period
        const totalProduction = formattedDailyData.reduce((sum, day) => sum + day.production, 0);
        const totalConsumption = formattedDailyData.reduce((sum, day) => sum + day.consumption, 0);
        const totalFeedIn = formattedDailyData.reduce((sum, day) => sum + day.feedIn, 0);
        const batteryCharge = Math.max(0, totalProduction * 0.1);

        const pieData = [
          { name: 'Eigenverbrauch', value: Math.round((totalConsumption / totalProduction) * 100) || 0 },
          { name: 'Einspeisung', value: Math.round((totalFeedIn / totalProduction) * 100) || 0 },
          { name: 'Batterieladung', value: Math.round((batteryCharge / totalProduction) * 100) || 0 }
        ];

        return NextResponse.json({
          lineChartData: formattedDailyData,
          pieChartData: pieData
        });
      }

      // Default hourly view
      const hourlyData = await db
        .select({
          time: sql`DATE_FORMAT(${leistung.timestamp}, '%H:00')`,
          production: sql`SUM(${leistung.powerOut}) / 1000`, // Convert to kW
        })
        .from(leistung)
        .where(sql`${leistung.modulnummer} IN (${moduleIds.join(',')}) 
                AND ${dateCondition}`)
        .groupBy(sql`DATE_FORMAT(${leistung.timestamp}, '%H:00')`)
        .orderBy(sql`DATE_FORMAT(${leistung.timestamp}, '%H:00')`);

      // Add consumption and feed-in data (simulated for now)
      const formattedData = hourlyData.map(hour => {
        const production = Number(hour.production);
        const consumption = production * (0.7 + Math.random() * 0.1);
        const feedIn = Math.max(0, production - consumption);
        
        return {
          time: hour.time,
          production: parseFloat(production.toFixed(1)),
          consumption: parseFloat(consumption.toFixed(1)),
          feedIn: parseFloat(feedIn.toFixed(1))
        };
      });

      // Calculate pie chart data
      const totalProduction = formattedData.reduce((sum, hour) => sum + hour.production, 0);
      const totalConsumption = formattedData.reduce((sum, hour) => sum + hour.consumption, 0);
      const totalFeedIn = formattedData.reduce((sum, hour) => sum + hour.feedIn, 0);
      const batteryCharge = Math.max(0, totalProduction * 0.1);

      const pieData = [
        { name: 'Eigenverbrauch', value: Math.round((totalConsumption / totalProduction) * 100) || 0 },
        { name: 'Einspeisung', value: Math.round((totalFeedIn / totalProduction) * 100) || 0 },
        { name: 'Batterieladung', value: Math.round((batteryCharge / totalProduction) * 100) || 0 }
      ];

      return NextResponse.json({
        lineChartData: formattedData,
        pieChartData: pieData
      });
      
    } catch (jwtError) {
      console.error('JWT verification error:', jwtError);
      return NextResponse.json(
        { message: 'Invalid authentication token' },
        { status: 401 }
      );
    }
  } catch (error) {
    console.error('Error fetching energy data:', error);
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    );
  }
} 