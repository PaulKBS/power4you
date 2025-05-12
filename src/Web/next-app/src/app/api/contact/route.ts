import { NextRequest, NextResponse } from 'next/server';
import nodemailer from 'nodemailer';

export async function POST(request: NextRequest) {
  try {
    // Parse the request body
    const body = await request.json();
    const { name, email, subject, message } = body;

    // Validate inputs
    if (!name || !email || !subject || !message) {
      return NextResponse.json(
        { message: 'Alle Felder müssen ausgefüllt sein' },
        { status: 400 }
      );
    }

    // Get SMTP URL from environment variables
    const smtpUrl = process.env.SMTP_URL;
    if (!smtpUrl) {
      console.error('SMTP_URL not configured in environment variables');
      return NextResponse.json(
        { message: 'E-Mail-Konfiguration fehlt auf dem Server' },
        { status: 500 }
      );
    }

    // Create email transporter from SMTP URL
    const transporter = nodemailer.createTransport(smtpUrl);

    // Setup email data
    const mailOptions = {
      from: `"Power4You Kontaktformular" <power4you@gbs-labor.de>`,
      to: 'power4you@gbs-labor.de',
      replyTo: email,
      subject: `Kontaktformular: ${subject}`,
      text: `
Name: ${name}
E-Mail: ${email}
Betreff: ${subject}

Nachricht:
${message}
      `,
      html: `
<h2>Neue Anfrage über das Kontaktformular</h2>
<p><strong>Name:</strong> ${name}</p>
<p><strong>E-Mail:</strong> ${email}</p>
<p><strong>Betreff:</strong> ${subject}</p>
<h3>Nachricht:</h3>
<p>${message.replace(/\n/g, '<br>')}</p>
      `,
    };

    // Send email
    await transporter.sendMail(mailOptions);

    // Return success response
    return NextResponse.json(
      { message: 'E-Mail erfolgreich gesendet' },
      { status: 200 }
    );
  } catch (error) {
    console.error('Error sending email:', error);
    return NextResponse.json(
      { message: 'Fehler beim Senden der E-Mail' },
      { status: 500 }
    );
  }
} 