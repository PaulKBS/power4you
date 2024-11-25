using power4you_client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace power4you_client.Services
{
    internal class DemoKundenService : IKundenService
    {
        private readonly List<Kunde> _kunden;

        public DemoKundenService()
        {
            _kunden = new List<Kunde>
            {
                new Kunde
                {
                    KundenID = 1,
                    Vorname = "Max",
                    Nachname = "Mustermann",
                    Email = "max.mustermann@example.com",
                    Telefon = "+49 123 4567890",
                    PLZ = "12345",
                    Ort = "Berlin",
                    Geburtsdatum = new DateTime(1985, 5, 15)
                },
                new Kunde
                {
                    KundenID = 2,
                    Vorname = "Anna",
                    Nachname = "Schmidt",
                    Email = "anna.schmidt@example.com",
                    Telefon = "+49 176 98765432",
                    PLZ = "80333",
                    Ort = "München",
                    Geburtsdatum = new DateTime(1992, 8, 23)
                },
                new Kunde
                {
                    KundenID = 3,
                    Vorname = "Thomas",
                    Nachname = "Weber",
                    Email = "thomas.weber@example.com",
                    Telefon = "+49 151 11223344",
                    PLZ = "70174",
                    Ort = "Stuttgart",
                    Geburtsdatum = new DateTime(1978, 12, 3)
                }
            };
        }

        public async Task<List<Kunde>> GetAllKundenAsync()
        {
            return await Task.FromResult(_kunden);
        }

        public async Task<Kunde> GetKundeByIdAsync(int id)
        {
            return await Task.FromResult(_kunden.FirstOrDefault(k => k.KundenID == id));
        }

        public async Task AddKundeAsync(Kunde kunde)
        {
            kunde.KundenID = _kunden.Max(k => k.KundenID) + 1;
            _kunden.Add(kunde);
            await Task.CompletedTask;
        }

        public async Task UpdateKundeAsync(Kunde kunde)
        {
            var existingKunde = _kunden.FirstOrDefault(k => k.KundenID == kunde.KundenID);
            if (existingKunde != null)
            {
                var index = _kunden.IndexOf(existingKunde);
                _kunden[index] = kunde;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteKundeAsync(int id)
        {
            var kunde = _kunden.FirstOrDefault(k => k.KundenID == id);
            if (kunde != null)
            {
                _kunden.Remove(kunde);
            }
            await Task.CompletedTask;
        }

        Task<Kunde> IKundenService.AddKundeAsync(Kunde kunde)
        {
            throw new NotImplementedException();
        }

        Task<Kunde> IKundenService.UpdateKundeAsync(Kunde kunde)
        {
            throw new NotImplementedException();
        }
    }
}