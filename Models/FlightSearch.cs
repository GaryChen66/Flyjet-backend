using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Flight_Backend.Models
{
    public class FlightSearch
    {
        public Price TotalPrice { get; set; }
        public List<TravelerPrice> TravelerPrices { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public string AirCraft { get; set; }
        public string GetOutput_TotalPrice
        {
            get
            {
                return TotalPrice.Value + TotalPrice.Currency;
            }
        }
        public string GetOutput_TravelPrices
        {
            get
            {
                int i = 0;
                string output = "";
                for(i = 0; i < TravelerPrices.Count; i++)
                {
                    if (i > 0)
                        output += "\n";
                    output += (i + 1) + ". " + TravelerPrices[i].Type + " "
                        + TravelerPrices[i].Value + TravelerPrices[i].Currency;
                }
                return output;
            }
        }
        public string GetOutput_Itineraries
        {
            get
            {
                string output = "";
                for (int i = 0; i < Itineraries.Count; i++)
                {
                    if (i > 0)
                        output += "\n";
                    output += get_date(Itineraries[i].departure_date)
                         + " - " + get_date(Itineraries[i].arrival_date)
                         + "(" + Itineraries[i].departure_airport + ", " + Itineraries[i].departure_city
                         + " - " + Itineraries[i].arrival_airport + ", " + Itineraries[i].arrival_city + ")";
                }
                return output;
            }
        }
        public string get_date(string origindate)
        {
            return origindate.Split(' ')[1] + " " + origindate.Split(' ')[2];
        }
    }
}
