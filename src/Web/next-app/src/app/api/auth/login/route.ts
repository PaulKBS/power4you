import { NextResponse } from 'next/server';
import jwt from 'jsonwebtoken';
import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { user } from '../../../../../drizzle/schema';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

/**
 * @swagger
 * /api/auth/login:
 *   post:
 *     summary: Authenticate user
 *     description: Authenticates a user with username and password
 *     tags:
 *       - Authentication
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - username
 *               - password
 *             properties:
 *               username:
 *                 type: string
 *                 description: User's username
 *                 example: user123
 *               password:
 *                 type: string
 *                 description: User's password
 *                 format: password
 *                 example: '*******'
 *     responses:
 *       200:
 *         description: Login successful, returns user data and sets auth cookie
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: Login successful
 *                 user:
 *                   type: object
 *                   properties:
 *                     userId:
 *                       type: integer
 *                       example: 123
 *                     username:
 *                       type: string
 *                       example: user123
 *       400:
 *         description: Missing username or password
 *       401:
 *         description: Invalid credentials
 *       500:
 *         description: Internal server error
 *       503:
 *         description: Database connection error
 */
export async function POST(request: Request) {
  try {
    const { username, password } = await request.json();

    if (!username || !password) {
      return NextResponse.json(
        { message: 'Benutzername und Passwort sind erforderlich' },
        { status: 400 }
      );
    }

    // Query the database for the user
    try {
      const result = await db.select().from(user).where(eq(user.username, username)).limit(1);
      
      if (result.length === 0) {
        return NextResponse.json(
          { message: 'Ungültige Anmeldeinformationen' },
          { status: 401 }
        );
      }

      const foundUser = result[0];
      const passwordMatch = password === foundUser.password;

      if (!passwordMatch) {
        return NextResponse.json(
          { message: 'Ungültige Anmeldeinformationen' },
          { status: 401 }
        );
      }

      // Generate JWT token
      const token = jwt.sign(
        { 
          userId: foundUser.userId,
          username: foundUser.username,
          apiKey: foundUser.apiKey
        },
        JWT_SECRET,
        { expiresIn: '24h' }
      );

      // Set HTTP-only cookie
      const response = NextResponse.json(
        { message: 'Login successful', user: { userId: foundUser.userId, username: foundUser.username } },
        { status: 200 }
      );

      // Update cookie settings to ensure it persists
      response.cookies.set('auth-token', token, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        path: '/',
        sameSite: 'lax', // Changed from 'strict' to 'lax' to allow the cookie to be sent with navigation
        maxAge: 60 * 60 * 24 // 24 hours
      });

      return response;
    } catch (dbError) {
      console.error('Database error during login:', dbError);
      return NextResponse.json(
        { message: 'Datenbankverbindungsfehler, bitte versuchen Sie es später erneut' },
        { status: 503 }
      );
    }
  } catch (error) {
    console.error('Login error:', error);
    return NextResponse.json(
      { message: 'Interner Serverfehler' },
      { status: 500 }
    );
  }
} 