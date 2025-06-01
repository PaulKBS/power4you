import { NextResponse } from 'next/server';
import { db } from '@/lib/db';
import { eq } from 'drizzle-orm';
import { kunde } from '../../../../drizzle/schema';
import { getUserIdFromRequest } from '@/lib/auth';

/**
 * @swagger
 * /api/customer:
 *   get:
 *     summary: Get customer data for the authenticated user
 *     description: Retrieves the profile data for the currently authenticated customer
 *     tags:
 *       - Customer
 *     security:
 *       - cookieAuth: []
 *       - apiKey: []
 *     responses:
 *       200:
 *         description: Customer data successfully retrieved
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 id:
 *                   type: integer
 *                 userId:
 *                   type: integer
 *                 vorname:
 *                   type: string
 *                 nachname:
 *                   type: string
 *                 strasse:
 *                   type: string
 *                 hausnummer:
 *                   type: string
 *                 postleitzahl:
 *                   type: string
 *                 ort:
 *                   type: string
 *                 email:
 *                   type: string
 *                   format: email
 *                 telefonnummer:
 *                   type: string
 *       401:
 *         description: Not authenticated or invalid token
 *       404:
 *         description: No customer data found for this user
 *       500:
 *         description: Internal server error
 */
export async function GET(request: Request) {
  try {
    // Authenticate request and get userId
    const { userId, error } = await getUserIdFromRequest(request);
    
    if (error) {
      return error;
    }
    
    // Fetch customer data based on user ID
    const customerData = await db.select().from(kunde).where(eq(kunde.userId, userId!));
    
    if (customerData.length === 0) {
      return NextResponse.json(
        { message: 'No customer data found for this user' },
        { status: 404 }
      );
    }
    
    return NextResponse.json(customerData[0]);
    
  } catch (error) {
    console.error('Error fetching customer data:', error);
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    );
  }
}

/**
 * @swagger
 * /api/customer:
 *   put:
 *     summary: Update customer data
 *     description: Updates the profile data for the currently authenticated customer
 *     tags:
 *       - Customer
 *     security:
 *       - cookieAuth: []
 *       - apiKey: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - vorname
 *               - nachname
 *               - strasse
 *               - hausnummer
 *               - postleitzahl
 *               - ort
 *               - email
 *               - telefonnummer
 *             properties:
 *               vorname:
 *                 type: string
 *                 description: First name
 *                 example: Max
 *               nachname:
 *                 type: string
 *                 description: Last name
 *                 example: Mustermann
 *               strasse:
 *                 type: string
 *                 description: Street name
 *                 example: Musterstraße
 *               hausnummer:
 *                 type: string
 *                 description: House number
 *                 example: 123
 *               postleitzahl:
 *                 type: string
 *                 description: Postal code
 *                 example: 12345
 *               ort:
 *                 type: string
 *                 description: City
 *                 example: Berlin
 *               email:
 *                 type: string
 *                 format: email
 *                 description: Email address
 *                 example: max.mustermann@example.com
 *               telefonnummer:
 *                 type: string
 *                 description: Phone number
 *                 example: 030123456789
 *     responses:
 *       200:
 *         description: Customer data updated successfully
 *       400:
 *         description: Invalid user ID
 *       401:
 *         description: Not authenticated or invalid token
 *       500:
 *         description: Internal server error
 */
export async function PUT(request: Request) {
  try {
    // Authenticate request and get userId
    const { userId, error } = await getUserIdFromRequest(request);
    
    if (error) {
      return error;
    }

    // Get the updated data from the request body
    const updatedData = await request.json();
    
    // Update customer data
    await db.update(kunde)
      .set({
        vorname: updatedData.vorname,
        nachname: updatedData.nachname,
        strasse: updatedData.strasse,
        hausnummer: updatedData.hausnummer,
        postleitzahl: updatedData.postleitzahl,
        ort: updatedData.ort,
        email: updatedData.email,
        telefonnummer: updatedData.telefonnummer,
      })
      .where(eq(kunde.userId, userId!));
    
    return NextResponse.json({ message: 'Customer data updated successfully' });
    
  } catch (error) {
    console.error('Error updating customer data:', error);
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    );
  }
} 