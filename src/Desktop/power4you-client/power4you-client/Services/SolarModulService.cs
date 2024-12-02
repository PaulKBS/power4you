using System.Collections.Generic;
using System.Linq;
using power4you_client.Models;

namespace power4you_client.Services
{
    public class SolarModulService
    {
        private static List<SolarModulTyp> _modulTypen;
        private static int _nextId;

        static SolarModulService()
        {
            _modulTypen = new List<SolarModulTyp>
            {
                new SolarModulTyp 
                { 
                    Solarmodultypnummer = 1,
                    Bezeichnung = "SunPower MAX3-400", 
                    Pmpp = 400.0f, 
                    Umpp = 65.8f, 
                    Impp = 6.08f 
                },
                new SolarModulTyp 
                { 
                    Solarmodultypnummer = 2,
                    Bezeichnung = "LG NeON 2 355", 
                    Pmpp = 355.0f, 
                    Umpp = 59.4f, 
                    Impp = 5.97f 
                },
                new SolarModulTyp 
                { 
                    Solarmodultypnummer = 3,
                    Bezeichnung = "JinkoSolar Tiger 390", 
                    Pmpp = 390.0f, 
                    Umpp = 63.2f, 
                    Impp = 6.17f 
                }
            };

            _nextId = _modulTypen.Max(m => m.Solarmodultypnummer) + 1;
        }

        public static List<SolarModulTyp> GetAllModulTypen()
        {
            return _modulTypen;
        }

        public static void AddModulTyp(SolarModulTyp modul)
        {
            modul.Solarmodultypnummer = _nextId++;
            _modulTypen.Add(modul);
        }

        public static void UpdateModulTyp(SolarModulTyp modul)
        {
            var existingModul = _modulTypen.FirstOrDefault(m => m.Solarmodultypnummer == modul.Solarmodultypnummer);
            if (existingModul != null)
            {
                var index = _modulTypen.IndexOf(existingModul);
                _modulTypen[index] = modul;
            }
        }

        public static void DeleteModulTyp(SolarModulTyp modul)
        {
            _modulTypen.Remove(modul);
        }
    }
} 