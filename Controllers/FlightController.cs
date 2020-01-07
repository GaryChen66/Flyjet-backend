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
using System.Text;

namespace Flight_Backend.Controllers
{
    public class FlightController : Controller
    {
        private static readonly HttpClient client;
        private static List<City> city_list;
        private static List<Country> country_list;
        private static List<FlightSearch> currentSearchResult { get; set; }
        private static bool tokenGet { get; set; }
        private static JArray flightData;
        static FlightController(){
            client = new HttpClient();
            string json = System.IO.File.ReadAllText("Database/cities.json");
            city_list = JsonConvert.DeserializeObject<List<City>>(json);

            json = System.IO.File.ReadAllText("Database/countries.json");
            country_list = JsonConvert.DeserializeObject<List<Country>>(json);

            currentSearchResult = new List<FlightSearch>();
            flightData = new JArray();
            tokenGet = false;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Confirm(int? id) {
            var flightDataItem = flightData.SingleOrDefault(item => item["id"].ToString() == id.ToString());
            var priceObject = new JObject();
            priceObject["data"] = new JObject();
            priceObject["data"]["type"] = new string("flight-offers-pricing");
            priceObject["data"]["flightOffers"] = new JArray();

            var tempArray = new JArray();
            tempArray.Add(flightDataItem);
            priceObject["data"]["flightOffers"] = tempArray;

            var bodyContent = new StringContent(JsonConvert.SerializeObject(priceObject),
                Encoding.UTF8, "application/json");

            var bookItem = currentSearchResult.SingleOrDefault(item => item.Id == id);
            var response = await client.PostAsync("https://test.api.amadeus.com/v1/shopping/flight-offers/pricing",
                bodyContent);
            var responseString = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseString);
            if (json["data"] != null)
            {
                return View(bookItem);
            }
            else
            {
                return NotFound();
            }
        }
        
        public async Task<IActionResult> Search(string flight_type,
            string departure, string arrival,
            string from, string to, int adults, int children, int infants,
            string travelClass, bool noneStop
            )
        {
            //Set Access Token Header
            if (!tokenGet)
            {
                var access_token = await GetAccessToken();
                client.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("Bearer", access_token);
                tokenGet = true;
            }
            if (departure == null)
            {
                return View(currentSearchResult);
            }
            //Get Flight Search Result
            var json = await GetFlightSearchResult(flight_type, departure, arrival, from, to,
                adults, children, infants, travelClass, noneStop);

            List<FlightSearch> searchResult = new List<FlightSearch>();

            if (json["errors"] == null) {
                //Get All Data
                JArray data = (JArray)json["data"];
                int length = data.Count;
                flightData = data;

                //Get Dictionary
                JObject dictionary = (JObject)json["dictionaries"];

                for(int i = 0; i < length; i++)
                {
                    FlightSearch searchItem = new FlightSearch();
                    JObject dataItem = (JObject)data[i];
                    searchItem.Id = Convert.ToInt32(dataItem["id"].ToString());
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
                            
                            //Departure Itinerary data
                            Airport_Info departure_airport_info = GetAirport_Info(segment["departure"]["iataCode"].ToString(), dictionary);
                            itinItem.departure_airport = departure_airport_info.airport;
                            itinItem.departure_city = departure_airport_info.city;
                            itinItem.departure_country = departure_airport_info.country;
                            itinItem.departure_date = segment["departure"]["at"].ToString();

                            //Arrival Itinerary data
                            Airport_Info arrival_airport_info = GetAirport_Info(segment["arrival"]["iataCode"].ToString(), dictionary);
                            itinItem.arrival_airport = arrival_airport_info.airport;
                            itinItem.arrival_city = arrival_airport_info.city;
                            itinItem.arrival_country = arrival_airport_info.country;
                            itinItem.arrival_date = segment["arrival"]["at"].ToString();

                            //Get Aircraft
                            string aircraft_code = segment["aircraft"]["code"].ToString();
                            itinItem.aircraft = dictionary["aircraft"][aircraft_code].ToString();
                            
                            //Get Airline
                            string airline_code = segment["carrierCode"].ToString();
                            itinItem.airline = dictionary["carriers"][airline_code].ToString();

                            searchItem.Itineraries.Add(itinItem);
                        }
                    }

                    searchResult.Add(searchItem);
                }
                //contentString = json.ToString();
                currentSearchResult = searchResult;
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

            var response = await client.PostAsync("https://test.api.amadeus.com/v1/security/oauth2/token",
                body_content);

            var responseString = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseString);
            
            return json["access_token"].ToString();
        }

        public async Task<JObject> GetFlightSearchResult(string flight_type,
            string departure, string arrival,
            string from, string to, int adults, int children, int infants,
            string travelClass, bool noneStop)
        {
            int max = 250;

            var search_url = "https://test.api.amadeus.com/v2/shopping/flight-offers?"
                + "originLocationCode=" + departure
                + "&destinationLocationCode=" + arrival
                + "&departureDate=" + from
                + "&adults=" + adults
                + "&children=" + children
                + "&infants=" + infants
                + "&currencyCode=" + "USD"
                + "&max=" + max;

            if (to != null)
            {
                if(to != "")
                    search_url += "&returnDate=" + to;
            }
            if (noneStop)
                search_url += "&nonStop=true";
            if (travelClass != "ANY")
                search_url += "&travelClass=" + travelClass;

            var response = await client.GetAsync(search_url);
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }
        public static Airport_Info GetAirport_Info(string code, JObject dictionary)
        {
            Airport_Info airport_Info = new Airport_Info();

            string cityCode = dictionary["locations"][code]["cityCode"].ToString();
            string countryCode = dictionary["locations"][code]["countryCode"].ToString(); ;

            var foundCity = city_list.SingleOrDefault(item => item.codeIataCity == cityCode);
            var foundCountry = country_list.SingleOrDefault(item => item.codeIso2Country == countryCode);
            airport_Info.airport = code;
            airport_Info.city = foundCity.nameCity;
            airport_Info.country = foundCountry.nameCountry;
            return airport_Info;
        }
    }
}