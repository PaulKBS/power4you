import { drizzle } from 'drizzle-orm/mysql2';
import mysql from 'mysql2/promise';
import { sql } from 'drizzle-orm';

// Create a database connection
const connectionString = process.env.DATABASE_URL;

if (!connectionString) {
  throw new Error('DATABASE_URL not found in environment variables');
}

// Create a connection pool for MySQL
const pool = mysql.createPool({
  uri: connectionString,
  waitForConnections: true,
  queueLimit: 0,
  enableKeepAlive: true,
  keepAliveInitialDelay: 0,
  connectionLimit: 1,
});

// Initialize the Drizzle client
export const db = drizzle(pool);

// Simple SQL tag for raw queries
export { sql }; 