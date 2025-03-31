-- Current sql file was generated after introspecting the database
-- If you want to run this migration please uncomment this code before executing migrations
/*
CREATE TABLE `rezepte` (
	`rezept_id` int(11) AUTO_INCREMENT NOT NULL,
	`user_id` int(11) NOT NULL,
	`title` varchar(255) NOT NULL,
	`image` varchar(255) NOT NULL,
	`text` text NOT NULL
);
--> statement-breakpoint
CREATE TABLE `user` (
	`user_id` int(11) AUTO_INCREMENT NOT NULL,
	`username` varchar(255) NOT NULL,
	`password` varchar(255) NOT NULL
);
--> statement-breakpoint
CREATE TABLE `zutaten` (
	`zutat_id` int(11) AUTO_INCREMENT NOT NULL,
	`rezept_id` int(11) NOT NULL,
	`zutat` varchar(255) NOT NULL
);
--> statement-breakpoint
ALTER TABLE `rezepte` ADD CONSTRAINT `user_id_fk` FOREIGN KEY (`user_id`) REFERENCES `user`(`user_id`) ON DELETE cascade ON UPDATE cascade;--> statement-breakpoint
ALTER TABLE `zutaten` ADD CONSTRAINT `zutaten_ibfk_1` FOREIGN KEY (`rezept_id`) REFERENCES `rezepte`(`rezept_id`) ON DELETE cascade ON UPDATE cascade;--> statement-breakpoint
CREATE INDEX `rezept_id` ON `zutaten` (`rezept_id`);
*/