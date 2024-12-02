using System;
using System.Collections.Generic;
using System.Linq;
using power4you_client.Models.Kunden;

namespace power4you_client.Services
{
    public class KundenService
    {
        private static List<Kunde> _kunden;
        private static int _nextId;

        // Static constructor to initialize mock data
        static KundenService()
        {
            _kunden = new List<Kunde>
            {
                new Kunde
                {
                    Id = 1,
                    Vorname = "Max",
                    Nachname = "Mustermann",
                    Email = "max.mustermann@example.com",
                    Telefon = "+49 (🇩🇪) 123 456789",
                    Strasse = "Sonnenallee",
                    Hausnummer = "42",
                    PLZ = "12345",
                    Ort = "Berlin",
                    AnlagenCount = 2,
                    ErstelltAm = DateTime.Now.AddMonths(-3)
                },
                new Kunde
                {
                    Id = 2,
                    Vorname = "Maria",
                    Nachname = "Schmidt",
                    Email = "m.schmidt@example.com",
                    Telefon = "+49 (🇩🇪) 987 654321",
                    Strasse = "Solarweg",
                    Hausnummer = "15",
                    PLZ = "54321",
                    Ort = "München",
                    AnlagenCount = 1,
                    ErstelltAm = DateTime.Now.AddMonths(-2)
                },
                new Kunde
                {
                    Id = 3,
                    Vorname = "Thomas",
                    Nachname = "Weber",
                    Email = "t.weber@example.com",
                    Telefon = "+49 (🇩🇪) 456 789123",
                    Strasse = "Energiestraße",
                    Hausnummer = "7a",
                    PLZ = "67890",
                    Ort = "Hamburg",
                    AnlagenCount = 3,
                    ErstelltAm = DateTime.Now.AddMonths(-1)
                },
                new Kunde
                {
                    Id = 4,
                    Vorname = "Sarah",
                    Nachname = "Becker",
                    Email = "s.becker@example.com",
                    Telefon = "+49 (🇩🇪) 234 567890",
                    Strasse = "Photovoltaikplatz",
                    Hausnummer = "23",
                    PLZ = "89012",
                    Ort = "Stuttgart",
                    AnlagenCount = 2,
                    ErstelltAm = DateTime.Now.AddMonths(-1)
                }
            };

            _nextId = _kunden.Max(k => k.Id) + 1;
        }

        public static List<Kunde> GetAllKunden()
        {
            return _kunden;
        }

        public static void AddKunde(Kunde kunde)
        {
            kunde.Id = _nextId++;
            kunde.ErstelltAm = DateTime.Now;
            _kunden.Add(kunde);
        }

        public static void UpdateKunde(Kunde kunde)
        {
            var existingKunde = _kunden.FirstOrDefault(k => k.Id == kunde.Id);
            if (existingKunde != null)
            {
                var index = _kunden.IndexOf(existingKunde);
                _kunden[index] = kunde;
            }
        }

        public static void DeleteKunde(Kunde kunde)
        {
            _kunden.Remove(kunde);
        }
    }
} 