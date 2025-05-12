import { mysqlTable, unique, int, varchar, index, timestamp, float } from "drizzle-orm/mysql-core"

export const kunde = mysqlTable("Kunde", {
	kundennummer: int("Kundennummer").autoincrement().notNull(),
	userId: int("User_ID").notNull().references(() => user.userId, { onDelete: "restrict", onUpdate: "restrict" } ),
	vorname: varchar("Vorname", { length: 255 }).notNull(),
	nachname: varchar("Nachname", { length: 255 }).notNull(),
	strasse: varchar("Strasse", { length: 255 }).notNull(),
	hausnummer: varchar("Hausnummer", { length: 255 }).notNull(),
	postleitzahl: varchar("Postleitzahl", { length: 255 }).notNull(),
	ort: varchar("Ort", { length: 255 }).notNull(),
	email: varchar("Email", { length: 255 }).notNull(),
	telefonnummer: varchar("Telefonnummer", { length: 255 }).notNull(),
},
(table) => [
	unique("User_ID").on(table.userId),
]);

export const leistung = mysqlTable("Leistung", {
	timestamp: timestamp("Timestamp", { mode: 'string' }).default('current_timestamp()').notNull(),
	modulnummer: int("Modulnummer").notNull().references(() => solarmodul.modulnummer, { onDelete: "restrict", onUpdate: "restrict" } ),
	powerOut: int("Power_Out").notNull(),
},
(table) => [
	index("Modulnummer_FK").on(table.modulnummer),
]);

export const solarmodul = mysqlTable("Solarmodul", {
	modulnummer: int("Modulnummer").autoincrement().notNull(),
	solarmodultypnummer: int("Solarmodultypnummer").notNull().references(() => solarmodultyp.solarmodultypnummer, { onDelete: "restrict", onUpdate: "restrict" } ),
	kundennummer: int("Kundennummer").notNull().references(() => kunde.kundennummer, { onDelete: "restrict", onUpdate: "restrict" } ),
},
(table) => [
	index("Kundennummer").on(table.kundennummer),
	index("Solarmodultypnummer").on(table.solarmodultypnummer),
]);

export const solarmodultyp = mysqlTable("Solarmodultyp", {
	solarmodultypnummer: int("Solarmodultypnummer").autoincrement().notNull(),
	bezeichnung: varchar("Bezeichnung", { length: 64 }).notNull(),
	umpp: float("Umpp").notNull(),
	impp: float("Impp").notNull(),
	pmpp: float("Pmpp").notNull(),
});

export const user = mysqlTable("User", {
	userId: int("User_ID").autoincrement().notNull(),
	username: varchar("Username", { length: 255 }).notNull(),
	password: varchar("Password", { length: 255 }).notNull(),
	apiKey: varchar("Api_key", { length: 255 }).notNull(),
});
