import { NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { kunde, user } from '../../../../../drizzle/schema';
import { verifyApiKey } from '@/lib/auth';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

export async function GET(request: Request) {
  try {
    // Check for API key authentication first
    const apiKey = request.headers.get('x-api-key');
    
    if (apiKey) {
      // Verify API key and get user ID
      const userId = await verifyApiKey(apiKey);
      
      if (!userId) {
        return NextResponse.json(
          { message: 'Invalid API key' },
          { status: 401 }
        );
      }
      
      // Get user information
      const userData = await db.select({
        userId: user.userId,
        username: user.username,
        apiKey: user.apiKey
      })
      .from(user)
      .where(eq(user.userId, userId))
      .limit(1);
      
      if (userData.length === 0) {
        return NextResponse.json(
          { message: 'User not found' },
          { status: 404 }
        );
      }
      
      // Query kunde table to get user's vorname and nachname
      const kundeData = await db.select({
        vorname: kunde.vorname,
        nachname: kunde.nachname
      }).from(kunde).where(eq(kunde.userId, userId));
      
      return NextResponse.json({
        userId: userData[0].userId,
        username: userData[0].username,
        apiKey: userData[0].apiKey,
        kunde: kundeData.length > 0 ? kundeData[0] : undefined
      });
    }
    
    // If no API key, fall back to JWT token authentication
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