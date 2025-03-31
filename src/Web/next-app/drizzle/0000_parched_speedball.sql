-- Current sql file was generated after introspecting the database
-- If you want to run this migration please uncomment this code before executing migrations
/*
CREATE TABLE `Kunde` (
	`Kundennummer` int(11) AUTO_INCREMENT NOT NULL,
	`User_ID` int(11) NOT NULL,
	`Vorname` varchar(255) NOT NULL,
	`Nachname` varchar(255) NOT NULL,
	`Strasse` varchar(255) NOT NULL,
	`Hausnummer` varchar(255) NOT NULL,
	`Postleitzahl` varchar(255) NOT NULL,
	`Ort` varchar(255) NOT NULL,
	`Email` varchar(255) NOT NULL,
	`Telefonnummer` varchar(255) NOT NULL,
	CONSTRAINT `User_ID` UNIQUE(`User_ID`)
);
--> statement-breakpoint
CREATE TABLE `Leistung` (
	`Timestamp` timestamp NOT NULL DEFAULT 'current_timestamp()',
	`Modulnummer` int(11) NOT NULL,
	`Power_Out` int(11) NOT NULL
);
--> statement-breakpoint
CREATE TABLE `Solarmodul` (
	`Modulnummer` int(11) AUTO_INCREMENT NOT NULL,
	`Solarmodultypnummer` int(11) NOT NULL,
	`Kundennummer` int(11) NOT NULL
);
--> statement-breakpoint
CREATE TABLE `Solarmodultyp` (
	`Solarmodultypnummer` int(11) AUTO_INCREMENT NOT NULL,
	`Bezeichnung` varchar(64) NOT NULL,
	`Umpp` float NOT NULL,
	`Impp` float NOT NULL,
	`Pmpp` float NOT NULL
);
--> statement-breakpoint
CREATE TABLE `User` (
	`User_ID` int(11) AUTO_INCREMENT NOT NULL,
	`Username` varchar(255) NOT NULL,
	`Password` varchar(255) NOT NULL,
	`Api_key` varchar(255) NOT NULL
);
--> statement-breakpoint
ALTER TABLE `Kunde` ADD CONSTRAINT `Kunde_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `User`(`User_ID`) ON DELETE restrict ON UPDATE restrict;--> statement-breakpoint
ALTER TABLE `Leistung` ADD CONSTRAINT `Leistung_ibfk_1` FOREIGN KEY (`Modulnummer`) REFERENCES `Solarmodul`(`Modulnummer`) ON DELETE restrict ON UPDATE restrict;--> statement-breakpoint
ALTER TABLE `Solarmodul` ADD CONSTRAINT `Solarmodul_ibfk_2` FOREIGN KEY (`Solarmodultypnummer`) REFERENCES `Solarmodultyp`(`Solarmodultypnummer`) ON DELETE restrict ON UPDATE restrict;--> statement-breakpoint
ALTER TABLE `Solarmodul` ADD CONSTRAINT `Solarmodul_ibfk_3` FOREIGN KEY (`Kundennummer`) REFERENCES `Kunde`(`Kundennummer`) ON DELETE restrict ON UPDATE restrict;--> statement-breakpoint
CREATE INDEX `Modulnummer_FK` ON `Leistung` (`Modulnummer`);--> statement-breakpoint
CREATE INDEX `Kundennummer` ON `Solarmodul` (`Kundennummer`);--> statement-breakpoint
CREATE INDEX `Solarmodultypnummer` ON `Solarmodul` (`Solarmodultypnummer`);
*/