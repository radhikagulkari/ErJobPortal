using ErJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Web;

namespace ErJobPortal.Controllers
{
    public class LocationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LocationController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // =========================================================
        // GET ALL COUNTRIES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                string url = "https://api.geocoded.me/v2/countries?fields=id,iso2,name&limit=300";

                using HttpClient client = _httpClientFactory.CreateClient();

                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                string json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<CountryApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || result.Data == null)
                {
                    return Json(new List<Country>());
                }

                return Json(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Country API Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStates(string countryCode)
        {
            try
            {
                // =====================================================
                // CHECK COUNTRY CODE
                // =====================================================

                if (string.IsNullOrWhiteSpace(countryCode))
                {
                    return BadRequest("Country code is required.");
                }

                // =====================================================
                // CLEAN COUNTRY CODE
                // =====================================================

                countryCode = countryCode.Trim().ToUpperInvariant();

                // =====================================================
                // API URL
                // =====================================================

                string url = "https://api.geocoded.me/v2/states" +
                             "?filter[country]=" +
                             Uri.EscapeDataString(countryCode) +
                             "&fields=id,name,countryCode,stateCode" +
                             "&limit=5000";

                Console.WriteLine("========================================");
                Console.WriteLine("COUNTRY CODE: " + countryCode);
                Console.WriteLine("STATE API URL: " + url);

                // =====================================================
                // HTTP CLIENT
                // =====================================================

                using HttpClient client = _httpClientFactory.CreateClient();

                // =====================================================
                // CALL API
                // =====================================================

                HttpResponseMessage response = await client.GetAsync(url);


                // =====================================================
                // READ RESPONSE
                // =====================================================

                string json = await response.Content.ReadAsStringAsync();

                Console.WriteLine("STATE API STATUS: " + (int)response.StatusCode);


                Console.WriteLine("STATE API RESPONSE: " + json);


                Console.WriteLine("========================================");


                // =====================================================
                // CHECK API RESPONSE
                // =====================================================

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, json);
                }


                // =====================================================
                // DESERIALIZE
                // =====================================================

                var result = JsonSerializer.Deserialize<StateApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                // =====================================================
                // CHECK RESULT
                // =====================================================

                if (result == null)
                {
                    return Json(new List<State>());
                }


                if (result.Data == null)
                {
                    return Json(new List<State>());
                }


                // =====================================================
                // SORT STATES
                // =====================================================

                var states = result.Data.OrderBy(x => x.Name).ToList();


                // =====================================================
                // RETURN STATES
                // =====================================================

                return Json(states);
            }
            catch (Exception ex)
            {
                Console.WriteLine("STATE EXCEPTION: " + ex.ToString());
                return StatusCode(500, "State API Error: " + ex.Message);
            }
        }

        // =========================================================
        // GET CITIES BY COUNTRY + STATE
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetCities(string countryCode, string stateCode)
        {
            try
            {
                Console.WriteLine(
                    $"Country: {countryCode}"
                );

                Console.WriteLine(
                    $"State: {stateCode}"
                );


                if (string.IsNullOrWhiteSpace(countryCode))
                {
                    return BadRequest(
                        "Country code is required."
                    );
                }

                if (string.IsNullOrWhiteSpace(stateCode))
                {
                    return BadRequest(
                        "State code is required."
                    );
                }


                countryCode =
                    countryCode.Trim().ToUpperInvariant();

                stateCode =
                    stateCode.Trim().ToUpperInvariant();


                string url =
                    "https://api.geocoded.me/v2/cities" +
                    "?filter[country]=" +
                    Uri.EscapeDataString(countryCode) +
                    "&filter[state]=" +
                    Uri.EscapeDataString(stateCode) +
                    "&fields=id,name,countryCode,stateCode" +
                    "&limit=5000";


                Console.WriteLine(
                    "CITY API URL: " + url
                );


                using HttpClient client =
                    _httpClientFactory.CreateClient();


                HttpResponseMessage response =
                    await client.GetAsync(url);


                string json =
                    await response.Content.ReadAsStringAsync();


                Console.WriteLine(
                    "API STATUS: " +
                    response.StatusCode
                );

                Console.WriteLine(
                    "API RESPONSE: " +
                    json
                );


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        json
                    );
                }


                var result =
                    JsonSerializer.Deserialize<CityApiResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );


                if (result == null)
                {
                    return Json(new
                    {
                        message = "Result is null",
                        cities = new List<City>()
                    });
                }


                if (result.Data == null)
                {
                    return Json(new
                    {
                        message = "Data is null",
                        cities = new List<City>()
                    });
                }


                var cities =
                    result.Data
                        .OrderBy(x => x.Name)
                        .ToList();


                Console.WriteLine(
                    $"Total Cities: {cities.Count}"
                );


                return Json(cities);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CITY ERROR: " + ex.ToString()
                );

                return StatusCode(
                    500,
                    "City API Error: " + ex.Message
                );
            }
        }
        // =============================================================
        // COUNTRY API RESPONSE
        // =============================================================

        public class CountryApiResponse
        {
            public List<Country> Data { get; set; } = new List<Country>();
        }


        // =============================================================
        // STATE API RESPONSE
        // =============================================================

        public class StateApiResponse
        {
            public List<State> Data { get; set; } = new List<State>();
        }


        // =============================================================
        // CITY API RESPONSE
        // =============================================================

        public class CityApiResponse
        {
            public List<City> Data { get; set; } = new List<City>();
        }
    }
}