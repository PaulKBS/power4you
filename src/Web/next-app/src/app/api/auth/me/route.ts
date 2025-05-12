import { NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { kunde } from '../../../../../drizzle/schema';

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
      console.log('No auth token found in cookies');
      return NextResponse.json(
        { message: 'Not authenticated' },
        { status: 401 }
      );
    }

    try {
      const { payload } = await jwtVerify(
        token,
        new TextEncoder().encode(JWT_SECRET)
      );

      // Get userId from payload
      const userId = Number(payload.userId);
      
      // Query kunde table to get user's vorname and nachname
      const kundeData = await db.select({
        vorname: kunde.vorname,
        nachname: kunde.nachname
      }).from(kunde).where(eq(kunde.userId, userId));
      
      return NextResponse.json({
        userId: payload.userId,
        username: payload.username,
        apiKey: payload.apiKey,
        kunde: kundeData.length > 0 ? kundeData[0] : undefined
      });
    } catch (jwtError) {
      console.error('JWT verification error:', jwtError);
      return NextResponse.json(
        { message: 'Invalid authentication token' },
        { status: 401 }
      );
    }
  } catch (error) {
    console.error('Authentication error:', error);
    return NextResponse.json(
      { message: 'Not authenticated' },
      { status: 401 }
    );
  }
} 