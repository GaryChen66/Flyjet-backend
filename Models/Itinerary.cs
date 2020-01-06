using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_Backend.Models
{
    public class Itinerary
    {
        public string departure_airport { get; set; }
        public string departure_city { get; set; }
        public string departure_country { get; set; }
        public string departure_date { get; set; }

        public string arrival_airport { get; set; }
        public string arrival_city { get; set; }
        public string arrival_date { get; set; }
        public string arrival_country { get; set; }

        public string aircraft { get; set; }
    }
}
