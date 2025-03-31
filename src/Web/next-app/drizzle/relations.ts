import { relations } from "drizzle-orm/relations";
import { user, kunde, solarmodul, leistung, solarmodultyp } from "./schema";

export const kundeRelations = relations(kunde, ({one, many}) => ({
	user: one(user, {
		fields: [kunde.userId],
		references: [user.userId]
	}),
	solarmoduls: many(solarmodul),
}));

export const userRelations = relations(user, ({many}) => ({
	kundes: many(kunde),
}));

export const leistungRelations = relations(leistung, ({one}) => ({
	solarmodul: one(solarmodul, {
		fields: [leistung.modulnummer],
		references: [solarmodul.modulnummer]
	}),
}));

export const solarmodulRelations = relations(solarmodul, ({one, many}) => ({
	leistungs: many(leistung),
	solarmodultyp: one(solarmodultyp, {
		fields: [solarmodul.solarmodultypnummer],
		references: [solarmodultyp.solarmodultypnummer]
	}),
	kunde: one(kunde, {
		fields: [solarmodul.kundennummer],
		references: [kunde.kundennummer]
	}),
}));

export const solarmodultypRelations = relations(solarmodultyp, ({many}) => ({
	solarmoduls: many(solarmodul),
}));