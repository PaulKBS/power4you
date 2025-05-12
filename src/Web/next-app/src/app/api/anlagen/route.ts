import { NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { kunde, solarmodul, solarmodultyp } from '../../../../drizzle/schema';

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
      
      // First, get the customer data for the user
      const customerData = await db.select().from(kunde).where(eq(kunde.userId, userId));
      
      if (customerData.length === 0) {
        return NextResponse.json(
          { message: 'No customer data found for this user' },
          { status: 404 }
        );
      }
      
      const kundennummer = customerData[0].kundennummer;
      
      // Now fetch all solar modules for this customer with their type information
      const modules = await db
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
        .where(eq(solarmodul.kundennummer, kundennummer));
      
      return NextResponse.json({ 
        customer: customerData[0],
        modules: modules 
      });
      
    } catch (jwtError) {
      console.error('JWT verification error:', jwtError);
      return NextResponse.json(
        { message: 'Invalid authentication token' },
        { status: 401 }
      );
    }
  } catch (error) {
    console.error('Error fetching solar modules:', error);
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    );
  }
} 