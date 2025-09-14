using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reqnroll;
using RestfulBooker.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RestfulBooker.Steps
{
    [Binding]
    public class GetBookingSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ApiClient _api = ApiClient.Instance;
       // private RestClient _client;
        private RestRequest _request;
        private RestResponse _response;
        private Stopwatch _stopwatch;

        public GetBookingSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
           // _client = new RestClient("https://restful-booker.herokuapp.com");
        }

        [Given(@"I have a valid auth token")]
        public void GivenIHaveAValidAuthToken()
        {
            // If your API needs an auth token, fetch and store here.
            // For restful-booker, GET booking doesn't need token, so skipping.
            _scenarioContext["AuthToken"] = AuthenticationHelper.GenerateToken();
        }

        [When(@"I retrieve booking ids without filters")]
        public void WhenIRetrieveBookingIdsWithoutFilters()
        {
            _request = new RestRequest("/booking", Method.Get);
            _stopwatch = Stopwatch.StartNew();
            _response = _api.Execute(_request);
            _stopwatch.Stop();
            _scenarioContext["Response"] = _response;
        }

        [When(@"I retrieve booking ids with filters:")]
        public void WhenIRetrieveBookingIdsWithFilters(Table table)
        {
            _request = new RestRequest("/booking", Method.Get);
            if (table.Rows.Count == 0)
            {
                string key = table.Header.First();  
                string value = table.Header.Last();      
                _request.AddQueryParameter(key, value);
                _request.AddHeader("accept", "application/json");
            }
            else
            {
                foreach (var row in table.Rows)
                {
                    var key = row["Key"];  
                    var value = row["Value"];        
                    _request.AddQueryParameter(key, value);
                    _request.AddHeader("accept", "application/json");
                } 
            }

            _stopwatch = Stopwatch.StartNew();
            _response = _api.Execute(_request);
            _stopwatch.Stop();
            _scenarioContext["Response"] = _response;
        }

        [When(@"I send an empty request to booking endpoint")]
        public void WhenISendAnEmptyRequest()
        {
            _request = new RestRequest("/booking", Method.Post); // Example of invalid call
            _request.AddStringBody("", DataFormat.Json);
            _stopwatch = Stopwatch.StartNew();
            _response = _api.Execute(_request);
            _stopwatch.Stop();
            _scenarioContext["Response"] = _response;
        }

        [When(@"I send malformed JSON to booking endpoint")]
        public void WhenISendMalformedJsonToBookingEndpoint()
        {
            _request = new RestRequest("/booking", Method.Post);
            _request.AddStringBody("{ invalidJson ", DataFormat.Json);
            _stopwatch = Stopwatch.StartNew();
            _response = _api.Execute(_request);
            _stopwatch.Stop();
            _scenarioContext["Response"] = _response;
        }

        [Then(@"the response status code should be (\d+)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            Assert.That((int)response.StatusCode, Is.EqualTo(expectedStatusCode),
                $"Expected status code {expectedStatusCode} but got {(int)response.StatusCode}");
        }

        [Then(@"the response should contain an array of objects with ""(.*)""")]
        public void ThenTheResponseShouldContainArrayOfObjectsWith(string propertyName)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            var json = JArray.Parse(response.Content);
            Assert.That(json.Count, Is.GreaterThan(0), "Expected at least one booking");
            foreach (var obj in json)
            {
                Assert.That(obj[propertyName], Is.Not.Null,
                    $"Expected property '{propertyName}' to be present in each object");
            }
        }

        [Then(@"the response should match the expected data types")]
        public void ThenTheResponseShouldMatchTheExpectedDataTypes()
        {
            var response = _scenarioContext["Response"] as RestResponse;
            var json = JArray.Parse(response.Content);

            foreach (var obj in json)
            {
                Assert.That(obj["bookingid"].Type, Is.EqualTo(JTokenType.Integer),
                    "bookingid should be an integer");
            }
        }
        
        
        [Then(@"every result should have checkin date greater than or equal to ""(.*)""")]
        public void ThenEveryResultShouldHaveCheckinDateGreaterThanOrEqualTo(string checkin)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            var json = JArray.Parse(response.Content);
            var expectedDate = DateTime.Parse(checkin);

            foreach (var obj in json)
            {
                int bookingId = obj["bookingid"].Value<int>();
                var detailRequest = new RestRequest($"/booking/{bookingId}", Method.Get);
                detailRequest.AddHeader("accept", "application/json");
                var detailResponse = _api.Execute(detailRequest);
                var detailJson = JObject.Parse(detailResponse.Content);

                DateTime actualCheckin = DateTime.Parse(detailJson["bookingdates"]["checkin"].ToString());
                Assert.That(actualCheckin, Is.GreaterThanOrEqualTo(expectedDate),
                    $"Expected checkin >= {expectedDate}, got {actualCheckin}");
            }
        }

        [Then(@"every result should have checkout date less than or equal to ""(.*)""")]
        public void ThenEveryResultShouldHaveCheckoutDateLessThanOrEqualTo(string checkout)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            var json = JArray.Parse(response.Content);
            var expectedDate = DateTime.Parse(checkout);

            foreach (var obj in json)
            {
                int bookingId = obj["bookingid"].Value<int>();
                var detailRequest = new RestRequest($"/booking/{bookingId}", Method.Get);
                detailRequest.AddHeader("accept", "application/json");
                var detailResponse = _api.Execute(detailRequest);
                var detailJson = JObject.Parse(detailResponse.Content);

                DateTime actualCheckout = DateTime.Parse(detailJson["bookingdates"]["checkout"].ToString());
                Assert.That(actualCheckout, Is.LessThanOrEqualTo(expectedDate),
                    $"Expected checkout <= {expectedDate}, got {actualCheckout}");
            }
        }

        [Then(@"the response time should be less than (\d+) ms")]
        public void ThenTheResponseTimeShouldBeLessThanMs(int maxMilliseconds)
        {
            Assert.That(_stopwatch.ElapsedMilliseconds, Is.LessThan(maxMilliseconds),
                $"Expected response time < {maxMilliseconds} ms but got {_stopwatch.ElapsedMilliseconds} ms");
        }
    }
}

public static class RestSharpExtensions
{
    public static TimeSpan ResponseTime(this RestResponse response)
    {
        return TimeSpan.FromMilliseconds(200);
    }
}

