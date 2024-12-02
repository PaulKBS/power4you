using System;
using System.Collections.Generic;

namespace power4you_client.Models
{
    public class Anlage
    {
        public int AnlageId { get; set; }
        public string Name { get; set; }
        public int KundenId { get; set; }
        public string KundenName { get; set; }
        public List<SolarModulTyp> Module { get; set; }
        public DateTime InstallationsDatum { get; set; }
        public double GesamtLeistung { get; set; }
        public string Standort { get; set; }
    }
} 