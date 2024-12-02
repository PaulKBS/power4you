using System;
using System.Collections.Generic;
using System.Linq;

namespace power4you_client.Models.Kunden
{
    public class Kunde
    {
        public int Id { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Strasse { get; set; }
        public string Hausnummer { get; set; }
        public string PLZ { get; set; }
        public string Ort { get; set; }
        public int AnlagenCount { get; set; }
        public DateTime ErstelltAm { get; set; }

        // Computed properties
        public string FullName => $"{Vorname} {Nachname}";
        public string FullAddress => $"{Strasse} {Hausnummer}, {PLZ} {Ort}";
        public string InitialLetters => $"{Vorname?.FirstOrDefault() ?? ' '}{Nachname?.FirstOrDefault() ?? ' '}";
    }
}
