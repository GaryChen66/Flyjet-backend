using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Flight_Backend.Models
{
    public class FlightSearch
    {
        public int Id { get; set; }
        public Price TotalPrice { get; set; }
        public List<TravelerPrice> TravelerPrices { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public string GetOutput_TotalPrice
        {
            get
            {
                if (TotalPrice.Currency == "USD")
                    return "$" + TotalPrice.Value;
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
                    if (TotalPrice.Currency == "USD")
                        output += (i + 1) + ". " + TravelerPrices[i].Type + " "
                            + "$" + TravelerPrices[i].Value;
                    else
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
                        output += "\n\n";
                    output += Itineraries[i].departure_date + " - " + Itineraries[i].arrival_date
                         + "(" + Itineraries[i].departure_airport + ", " + Itineraries[i].departure_city + ", " + Itineraries[i].departure_country
                         + " - " + Itineraries[i].arrival_airport + ", " + Itineraries[i].arrival_city + ", " + Itineraries[i].arrival_country + ")"
                         + "\nAircraft: " + Itineraries[i].aircraft + " Airline: " + Itineraries[i].airline;
                }
                return output;
            }
        }
    }
}
