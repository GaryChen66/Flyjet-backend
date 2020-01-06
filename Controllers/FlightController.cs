using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

using Flight_Backend.Models;
using Newtonsoft.Json;

namespace Flight_Backend.Controllers
{
    public class FlightController : Controller
    {
        private static readonly HttpClient client;
        private static List<Airport> airport_list;
        private static List<City> city_list;
        static FlightController(){
            client = new HttpClient();
            string json = System.IO.File.ReadAllText("Database/airport.json");
            airport_list = JsonConvert.DeserializeObject<List<Airport>>(json);

            json = System.IO.File.ReadAllText("Database/cities.json");
            city_list = JsonConvert.DeserializeObject<List<City>>(json);
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string flight_type, string charter_aircraft,
            string charter_seat, string commercial_seat,
            string departure, string arrival,
            string from, string to, int adults, int children, int infants
            )
        {
            string parameter_string = "Flight Type:" + flight_type + "\n"
                + "charter aircraft:" + charter_aircraft + "\n"
                + "charter seat:" + charter_seat + "\n"
                + "departure:" + departure + "\n"
                + "arrivalt:" + arrival + "\n"
                + "from:" + from + "\n"
                + "to:" + to + "\n";

            //Set Access Token Header
            var access_token = await GetAccessToken();
            client.DefaultRequestHeaders.Authorization
                         = new AuthenticationHeaderValue("Bearer", access_token);

            //Get Flight Search Result
            var json = await GetFlightSearchResult(flight_type, charter_aircraft,
                charter_seat, commercial_seat, departure, arrival, from, to,
                adults, children, infants);

            List<FlightSearch> searchResult = new List<FlightSearch>();

            if (json["errors"] == null) {
                JArray data = (JArray)json["data"];
                int length = data.Count;

                for(int i = 0; i < length; i++)
                {
                    FlightSearch searchItem = new FlightSearch();
                    JObject dataItem = (JObject)data[i];
                    searchItem.TotalPrice = new Price {
                        Value = dataItem["price"]["grandTotal"].ToString(),
                        Currency = dataItem["price"]["currency"].ToString(),
                    };
                    searchItem.TravelerPrices = new List<TravelerPrice>();
                    
                    JArray travelerPricing = (JArray)dataItem["travelerPricings"];
                    for(int j = 0; j < travelerPricing.Count; j ++)
                    {
                        JObject travelerItem = (JObject)travelerPricing[j];
                        searchItem.TravelerPrices.Add(
                            new TravelerPrice {
                                Type = travelerItem["travelerType"].ToString(),
                                Value = travelerItem["price"]["total"].ToString(),
                                Currency = travelerItem["price"]["currency"].ToString()
                            }
                        );
                    }

                    searchItem.Itineraries = new List<Itinerary>();
                    JArray itin_arr = (JArray)dataItem["itineraries"];
                    for(int j = 0; j < itin_arr.Count; j++)
                    {
                        JObject itin = (JObject)itin_arr[j];
                        JArray segment_arr = (JArray)itin["segments"];
                        for(int k = 0; k < segment_arr.Count; k++)
                        {
                            JObject segment = (JObject)segment_arr[k];
                            Itinerary itinItem = new Itinerary();
                            Airport_Info departure_airport_info = GetAirport_Info(segment["departure"]["iataCode"].ToString());
                            itinItem.departure_airport = departure_airport_info.airport;
                            itinItem.departure_city = departure_airport_info.city;
                            itinItem.departure_country = departure_airport_info.country;
                            itinItem.departure_date = segment["departure"]["at"].ToString();

                            Airport_Info arrival_airport_info = GetAirport_Info(segment["arrival"]["iataCode"].ToString());
                            itinItem.arrival_airport = arrival_airport_info.airport;
                            itinItem.arrival_city = arrival_airport_info.city;
                            itinItem.arrival_country = arrival_airport_info.country;
                            itinItem.arrival_date = segment["arrival"]["at"].ToString();
                            searchItem.Itineraries.Add(itinItem);
                        }
                    }

                    searchResult.Add(searchItem);
                }
                //contentString = json.ToString();
            }
            return View(searchResult);
        }

        public async Task<string> GetAccessToken()
        {
            var auth_values = new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" },
                { "grant_type", "client_credentials" },
                { "client_id", "aaLwnYEJlUHXTeaFCVhhf5ZaeNCql9qr" },
                { "client_secret", "RVFcHRRPZxIJdtW6" }
            };

            var body_content = new FormUrlEncodedContent(auth_values);
            var auth_header = new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            };

            var response = await client.PostAsync("https://test.api.amadeus.com/v1/security/oauth2/token",
                body_content);

            var responseString = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseString);
            
            return json["access_token"].ToString();
        }

        public async Task<JObject> GetFlightSearchResult(string flight_type, string charter_aircraft,
            string charter_seat, string commercial_seat,
            string departure, string arrival,
            string from, string to, int adults, int children, int infants)
        {
            string originLocationCode, destinationLocationCode, departureDate, returnDate,
                   adults_cnt, children_cnt, infants_cnt, travelClass, includedAirlineCodes,
                   excludedAirlineCodes, nonStop, currencyCode, max;
            originLocationCode = departure;
            destinationLocationCode = arrival;
            departureDate = from;
            returnDate = to;
            adults_cnt = adults.ToString(); children_cnt = children.ToString();
            infants_cnt = infants.ToString();
            max = "10";

            var search_url = "https://test.api.amadeus.com/v2/shopping/flight-offers?"
                + "originLocationCode=" + originLocationCode
                + "&destinationLocationCode=" + destinationLocationCode
                + "&departureDate=" + departureDate
                + "&returnDate=" + returnDate
                + "&adults=" + adults_cnt
                + "&children=" + children_cnt
                + "&infants=" + infants_cnt
                + "&includedAirlineCodes=" + "TG"
                + "&max=" + max;
            var response = await client.GetAsync(search_url);
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }
        public static Airport_Info GetAirport_Info(string code)
        {
            Airport_Info airport_Info = new Airport_Info();

            var foundAirport = airport_list.SingleOrDefault(item => item.codeIataAirport == code);
            var city = foundAirport.codeIataCity;

            var foundCity = city_list.SingleOrDefault(item => item.codeIataCity == city);
            airport_Info.airport = code;
            airport_Info.city = foundCity.nameCity;
            airport_Info.country = foundAirport.nameCountry;
            return airport_Info;
        }
    }
}