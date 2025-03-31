import { mysqlTable, mysqlSchema, AnyMySqlColumn, foreignKey, int, varchar, text, index } from "drizzle-orm/mysql-core"
import { sql } from "drizzle-orm"

export const rezepte = mysqlTable("rezepte", {
	rezeptId: int("rezept_id").autoincrement().notNull(),
	userId: int("user_id").notNull().references(() => user.userId, { onDelete: "cascade", onUpdate: "cascade" } ),
	title: varchar({ length: 255 }).notNull(),
	image: varchar({ length: 255 }).notNull(),
	text: text().notNull(),
});

export const user = mysqlTable("user", {
	userId: int("user_id").autoincrement().notNull(),
	username: varchar({ length: 255 }).notNull(),
	password: varchar({ length: 255 }).notNull(),
});

export const zutaten = mysqlTable("zutaten", {
	zutatId: int("zutat_id").autoincrement().notNull(),
	rezeptId: int("rezept_id").notNull().references(() => rezepte.rezeptId, { onDelete: "cascade", onUpdate: "cascade" } ),
	zutat: varchar({ length: 255 }).notNull(),
},
(table) => [
	index("rezept_id").on(table.rezeptId),
]);
