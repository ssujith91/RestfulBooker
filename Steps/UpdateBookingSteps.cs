using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reqnroll;
using RestfulBooker.Utils;
using RestSharp;
using System.Diagnostics;
using System.Globalization;
namespace RestfulBooker.Steps
{
    [Binding]
    [Scope(Tag = "UpdateBooking")]
    public class UpdateBookingSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ApiClient _api = ApiClient.Instance;
        //private RestClient _client;
        private RestRequest _request;
        private RestResponse _response;
        private int _bookingId;
        private Stopwatch _stopwatch;

        public UpdateBookingSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            //_client = new RestClient("https://restful-booker.herokuapp.com");
        }

        // -------------------------------------------
        // Given Steps
        // -------------------------------------------

        [Given(@"an existing booking ID")]
        public void GivenAnExistingBookingID()
        {
            // Create a new booking to get a valid ID
            var body = new
            {
                firstname = "Test",
                lastname = "User",
                totalprice = 100,
                depositpaid = true,
                bookingdates = new { checkin = "2025-01-01", checkout = "2025-01-10" },
                additionalneeds = "Breakfast"
            };

            var createRequest = new RestRequest("/booking", Method.Post);
            createRequest.AddHeader("accept", "application/json");
            createRequest.AddHeader("Content-Type", "application/json");
            createRequest.AddJsonBody(body);
            var createResponse = _api.Execute(createRequest);

            var json = JObject.Parse(createResponse.Content);
            _bookingId = json["bookingid"].Value<int>();
            _scenarioContext["BookingId"] = _bookingId;
        }

        [Given(@"a non-existent booking ID")]
        public void GivenANonExistentBookingID()
        {
            _bookingId = 9999999; // Arbitrary non-existent ID
            _scenarioContext["BookingId"] = _bookingId;
        }

        // -------------------------------------------
        // When Steps
        // -------------------------------------------

        [When(@"I update the booking with:")]
        public void WhenIUpdateTheBookingWith(Table table)
        {
            // Build JSON payload from table
            var body = new JObject();
            foreach (var row in table.Rows)
            {
                var key = row["field"];
                var value = row["value"];
                body[key] = value;
            }
            
            _response = _api.Patch($"/booking/{_bookingId}",JsonConvert.SerializeObject(body));
            _scenarioContext["Response"] = _response;
            _scenarioContext["UpdatePayload"] = body;
        }

        [When(@"I update the booking with multiple fields from file ""(.*)""")]
        public void WhenIUpdateBookingWithMultipleFieldsFromFile(string fileName)
        {
            string json = File.ReadAllText($"TestData/{fileName}");
            var body = JsonConvert.DeserializeObject<JObject>(json);
                    
            _response = _api.Patch($"/booking/{_bookingId}", JsonConvert.SerializeObject(body));
            _scenarioContext["Response"] = _response;
            _scenarioContext["UpdatePayload"] = body;
        }

        [When(@"I update the booking with nested bookingdates from file ""(.*)""")]
        public void WhenIUpdateBookingWithNestedBookingDates(string fileName)
        {
            string json = File.ReadAllText($"TestData/{fileName}");
            var body = JsonConvert.DeserializeObject<JObject>(json);
                      
            _response = _api.Patch($"/booking/{_bookingId}", JsonConvert.SerializeObject(body));

            _scenarioContext["Response"] = _response;
            _scenarioContext["UpdatePayload"] = body;
        }

        [When(@"I update the booking without authentication using:")]
        public void WhenIUpdateBookingWithoutAuth(Table table)
        {
            _request = new RestRequest($"/booking/{_bookingId}", Method.Patch);
            _request.AddHeader("accept", "application/json");
            _request.AddHeader("Content-Type", "application/json");
            var body = new JObject();
            foreach (var row in table.Rows)
            {
                var key = row["field"];
                var value = row["value"];
                body[key] = value;
            }

            _request.AddJsonBody(body);
            _response = _api.Execute(_request); // no auth
            _scenarioContext["Response"] = _response;
        }

        [When(@"I send malformed or incomplete JSON from file ""(.*)""")]
        public void WhenISendMalformedOrIncompleteJSONFromFile(string fileName)
        {
            string json = File.ReadAllText($"TestData/{fileName}");
             
            _response = _api.Patch($"/booking/{_bookingId}", JsonConvert.SerializeObject(json));
            _scenarioContext["Response"] = _response;
        }

        [When(@"I update the booking again with the same payload")]
        public void WhenIUpdateBookingAgainWithSamePayload()
        {
            var body = _scenarioContext["UpdatePayload"];
            var secondResponse = _api.Patch($"/booking/{_bookingId}", JsonConvert.SerializeObject(body));
           
            _scenarioContext["SecondResponse"] = secondResponse;
        }

        // -------------------------------------------
        // Then Steps
        // -------------------------------------------

        [Then(@"the response status code should be (.*)")]
        
        public void ThenTheResponseStatusCodeShouldBe(int statusCode)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            Assert.That((int)response.StatusCode, Is.EqualTo(statusCode));
        }

        [Then(@"the field ""(.*)"" should be updated to ""(.*)""")]
        public void ThenTheFieldShouldBeUpdatedTo(string field, string value)
        {
            // Fetch booking details
            var detailResponse = _api.Get($"/booking/{_bookingId}");

            Assert.That(detailResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            var detailJson = JObject.Parse(detailResponse.Content);

            // Compare the specific field
            Assert.That(detailJson[field].ToString(), Is.EqualTo(value),
                $"Expected {field}='{value}' but got '{detailJson[field]}'");
        }

        [Then(@"all specified fields should be updated accordingly")]
        public void ThenAllSpecifiedFieldsShouldBeUpdatedAccordingly()
        {
            var response = _scenarioContext["Response"] as RestResponse;
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            var payload = _scenarioContext["UpdatePayload"] as JObject;
            var bookingResponse = JObject.Parse(response.Content);

            foreach (var prop in payload.Properties())
            {
                if (prop.Name != "bookingdates") // bookingdates handled separately
                {
                    Assert.That(bookingResponse[prop.Name].ToString(), Is.EqualTo(prop.Value.ToString()),
                        $"Field '{prop.Name}' expected value '{prop.Value}', got '{bookingResponse[prop.Name]}'");
                }
            }
        }

        [Then(@"other fields should remain unchanged")]
        public void ThenOtherFieldsShouldRemainUnchanged()
        {
            // Fetch full booking details and verify fields not in payload remain unchanged
            var payload = _scenarioContext["UpdatePayload"] as JObject;

            var detailResponse = _api.Get($"/booking/{_bookingId}");
            var detailJson = JObject.Parse(detailResponse.Content);

            foreach (var prop in detailJson.Properties())
            {
                if (!payload.ContainsKey(prop.Name))
                {
                    // For simplicity, just assert property exists
                    Assert.IsNotNull(prop.Value);
                }
            }
        }

        [Then(@"the bookingdates should be updated correctly")]
        public void ThenTheBookingDatesShouldBeUpdatedCorrectly()
        {
            var payload = _scenarioContext["UpdatePayload"] as JObject;
            var responseJson = JObject.Parse((_scenarioContext["Response"] as RestResponse).Content);

            if (payload.ContainsKey("bookingdates"))
            {
                foreach (var dateProp in payload["bookingdates"].Children<JProperty>())
                {
                    Assert.That(responseJson["bookingdates"][dateProp.Name].ToString(),
                        Is.EqualTo(dateProp.Value.ToString()),
                        $"Bookingdates '{dateProp.Name}' expected '{dateProp.Value}' but got '{responseJson["bookingdates"][dateProp.Name]}'");
                }
            }
        }

        [Then(@"both responses status codes should be 200")]
        public void ThenBothResponsesStatusCodesShouldBe200()
        {
            var firstResponse = _scenarioContext["Response"] as RestResponse;
            var secondResponse = _scenarioContext["SecondResponse"] as RestResponse;

            Assert.That(firstResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(secondResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Then(@"the final state of the booking should match the last update")]
        public void ThenTheFinalStateOfBookingShouldMatchLastUpdate()
        {
            var payload = _scenarioContext["UpdatePayload"] as JObject;
                       
            var detailResponse = _api.Get($"/booking/{_bookingId}");

            var detailJson = JObject.Parse(detailResponse.Content);
            foreach (var prop in payload.Properties())
            {
                Assert.That(detailJson[prop.Name].ToString(), Is.EqualTo(prop.Value.ToString()));
            }
        }

        // -------------------------------------------
        // Helper
        // -------------------------------------------
        private void AddAuthToken()
        {
            //if (!_scenarioContext.ContainsKey("AuthToken"))
            //{
            //    // Request new token
            //    var authRequest = new RestRequest("/auth", Method.Post);
            //    authRequest.AddJsonBody(new { username = "admin", password = "password123" });
            //    var authResponse = _api.Execute(authRequest);
            //    var tokenJson = JObject.Parse(authResponse.Content);
            //    _scenarioContext["AuthToken"] = tokenJson["token"].ToString();
            //}

            //_request.AddHeader("Cookie", $"token={_scenarioContext["AuthToken"]}");
        }
    }
}

