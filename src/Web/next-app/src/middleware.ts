import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { jwtVerify } from 'jose';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

export async function middleware(request: NextRequest) {
  // Handle API routes with API key authentication
  if (request.nextUrl.pathname.startsWith('/api/')) {
    // Skip authentication for login route
    if (request.nextUrl.pathname === '/api/auth/login') {
      return NextResponse.next();
    }

    // Check for API key in headers
    const apiKey = request.headers.get('x-api-key');
    
    // If API key exists, validate it
    if (apiKey) {
      // Skip JWT cookie authentication for API key requests
      // The actual API endpoint will verify the API key against the database
      request.headers.set('x-api-authenticated', 'true');
      return NextResponse.next();
    }
    
    // No API key, fall back to JWT token authentication
    const token = request.cookies.get('auth-token')?.value;
    
    if (!token) {
      return NextResponse.json(
        { message: 'Not authenticated' },
        { status: 401 }
      );
    }

    try {
      // Verify the token
      await jwtVerify(
        token,
        new TextEncoder().encode(JWT_SECRET)
      );
      
      return NextResponse.next();
    } catch (error) {
      // Token is invalid or expired
      console.error('Token verification failed:', error);
      return NextResponse.json(
        { message: 'Invalid authentication token' },
        { status: 401 }
      );
    }
  }
  
  // Non-API routes
  if (request.nextUrl.pathname === '/login') {
    return NextResponse.next();
  }

  const token = request.cookies.get('auth-token')?.value;

  if (!token) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  try {
    // Verify the token
    await jwtVerify(
      token,
      new TextEncoder().encode(JWT_SECRET)
    );
    
    return NextResponse.next();
  } catch (error) {
    // Token is invalid or expired
    console.error('Token verification failed:', error);
    return NextResponse.redirect(new URL('/login', request.url));
  }
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    '/((?!_next/static|_next/image|favicon.ico).*)',
  ],
}; 