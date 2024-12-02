namespace power4you_client.Models
{
    public class SolarModulTyp
    {
        public int ModulId { get; set; }
        public string Bezeichnung { get; set; }
        public float Pmpp { get; set; }  // Peak power in Watts
        public float Umpp { get; set; }  // Voltage at maximum power point
        public float Impp { get; set; }  // Current at maximum power point
    }
} 