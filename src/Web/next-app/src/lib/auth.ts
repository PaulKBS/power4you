import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { user } from '../../drizzle/schema';
import { jwtVerify } from 'jose';
import { NextResponse } from 'next/server';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

/**
 * Verify API key against the database
 * @param apiKey - The API key to verify
 * @returns User ID if valid, null if invalid
 */
export async function verifyApiKey(apiKey: string): Promise<number | null> {
  try {
    // Find user with matching API key
    const result = await db.select({
      userId: user.userId,
      username: user.username,
      apiKey: user.apiKey
    })
    .from(user)
    .where(eq(user.apiKey, apiKey))
    .limit(1);

    if (result.length === 0) {
      return null;
    }

    return result[0].userId;
  } catch (error) {
    console.error('Error verifying API key:', error);
    return null;
  }
}

/**
 * Extract user ID from request using either API key or JWT token
 * @param request - The incoming request
 * @returns User ID and authentication result
 */
export async function getUserIdFromRequest(request: Request): Promise<{ userId: number | null; error: NextResponse | null }> {
  try {
    // Check for API key authentication
    const apiKey = request.headers.get('x-api-key');
    
    if (apiKey) {
      // Verify API key and get user ID
      const userId = await verifyApiKey(apiKey);
      
      if (!userId) {
        return {
          userId: null,
          error: NextResponse.json(
            { message: 'Invalid API key' },
            { status: 401 }
          )
        };
      }
      
      return { userId, error: null };
    }
    
    // No API key, fall back to JWT token authentication
    const cookieHeader = request.headers.get('cookie') || '';
    const cookies = Object.fromEntries(
      cookieHeader.split('; ').map(c => {
        const [name, ...value] = c.split('=');
        return [name, value.join('=')];
      })
    );
    const token = cookies['auth-token'];
    
    if (!token) {
      return {
        userId: null,
        error: NextResponse.json(
          { message: 'Not authenticated' },
          { status: 401 }
        )
      };
    }

    try {
      // Verify the token
      const { payload } = await jwtVerify(
        token,
        new TextEncoder().encode(JWT_SECRET)
      );
      
      // Get userId from payload
      const userId = Number(payload.userId);
      
      if (isNaN(userId)) {
        return {
          userId: null,
          error: NextResponse.json(
            { message: 'Invalid user ID' },
            { status: 400 }
          )
        };
      }
      
      return { userId, error: null };
    } catch (jwtError) {
      console.error('JWT verification error:', jwtError);
      return {
        userId: null,
        error: NextResponse.json(
          { message: 'Invalid authentication token' },
          { status: 401 }
        )
      };
    }
  } catch (error) {
    console.error('Error authenticating request:', error);
    return {
      userId: null,
      error: NextResponse.json(
        { message: 'Internal server error' },
        { status: 500 }
      )
    };
  }
} 