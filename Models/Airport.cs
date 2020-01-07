namespace Flight_Backend.Models
{
    public class Airport
    {
        public string codeIataAirport { get; set; }
        public string codeIataCity { get; set; }
        public string nameCountry { get; set; }
    }
    public class City
    {
        public string codeIataCity { get; set; }
        public string nameCity { get; set; }
        public string timezone { get; set; }
    }
    public class Country
    {
        public string codeIso2Country { get; set; }
        public string codeIso3Country { get; set; }
        public string nameCountry { get; set; }
    }
    public class Airport_Info
    {
        public string airport { get; set; }
        public string city { get; set; }
        public string country { get; set; }
    }
}
