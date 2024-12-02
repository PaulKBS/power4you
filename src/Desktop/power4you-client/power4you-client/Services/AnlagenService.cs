using System;
using System.Collections.Generic;
using System.Linq;
using power4you_client.Models;

namespace power4you_client.Services
{
    public class AnlagenService
    {
        private static List<Anlage> _anlagen;
        private static int _nextId;

        static AnlagenService()
        {
            _anlagen = new List<Anlage>
            {
                new Anlage
                {
                    AnlageId = 1,
                    Name = "Anlage Peter",
                    KundenId = 1,
                    KundenName = "Peter Meyer",
                    Module = new List<SolarModulTyp>
                    {
                        new SolarModulTyp { Bezeichnung = "SunPower MAX3-400", Pmpp = 400.0f, Umpp = 65.8f, Impp = 6.08f },
                        new SolarModulTyp { Bezeichnung = "SunPower MAX3-400", Pmpp = 400.0f, Umpp = 65.8f, Impp = 6.08f }
                    },
                    InstallationsDatum = new DateTime(2023, 6, 15),
                    GesamtLeistung = 0.8,
                    Standort = "Kirchplatz 7, 34567 München"
                },
                new Anlage
                {
                    AnlageId = 2,
                    Name = "Anlage Schmidt",
                    KundenId = 2,
                    KundenName = "Anna Schmidt",
                    Module = new List<SolarModulTyp>
                    {
                        new SolarModulTyp { Bezeichnung = "LG NeON 2 355", Pmpp = 355.0f, Umpp = 59.4f, Impp = 5.97f },
                        new SolarModulTyp { Bezeichnung = "LG NeON 2 355", Pmpp = 355.0f, Umpp = 59.4f, Impp = 5.97f },
                        new SolarModulTyp { Bezeichnung = "LG NeON 2 355", Pmpp = 355.0f, Umpp = 59.4f, Impp = 5.97f }
                    },
                    InstallationsDatum = new DateTime(2023, 8, 22),
                    GesamtLeistung = 1.065,
                    Standort = "Hauptstraße 42, 23456 Hamburg"
                }
            };

            _nextId = _anlagen.Max(a => a.AnlageId) + 1;
        }

        public static List<Anlage> GetAllAnlagen()
        {
            return _anlagen;
        }

        public static void AddAnlage(Anlage anlage)
        {
            anlage.AnlageId = _nextId++;
            _anlagen.Add(anlage);
        }

        public static void UpdateAnlage(Anlage anlage)
        {
            var existingAnlage = _anlagen.FirstOrDefault(a => a.AnlageId == anlage.AnlageId);
            if (existingAnlage != null)
            {
                var index = _anlagen.IndexOf(existingAnlage);
                _anlagen[index] = anlage;
            }
        }

        public static void DeleteAnlage(Anlage anlage)
        {
            _anlagen.Remove(anlage);
        }
    }
} 