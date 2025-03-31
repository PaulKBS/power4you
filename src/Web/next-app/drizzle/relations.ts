import { relations } from "drizzle-orm/relations";
import { user, rezepte, zutaten } from "./schema";

export const rezepteRelations = relations(rezepte, ({one, many}) => ({
	user: one(user, {
		fields: [rezepte.userId],
		references: [user.userId]
	}),
	zutatens: many(zutaten),
}));

export const userRelations = relations(user, ({many}) => ({
	rezeptes: many(rezepte),
}));

export const zutatenRelations = relations(zutaten, ({one}) => ({
	rezepte: one(rezepte, {
		fields: [zutaten.rezeptId],
		references: [rezepte.rezeptId]
	}),
}));