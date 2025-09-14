using NUnit.Framework;
using Reqnroll;
using RestfulBooker.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;

using RestfulBooker.Utils;

namespace RestfulBooker.Steps
{
    [Binding]
    public class DeleteBookingSteps
    {
        private readonly ApiClient _api = ApiClient.Instance;
        private RestResponse _response;
        private List<RestResponse> _parallelResponses = new();
        private double _bookingId;

        [Given(@"an existing booking ID")]
        public void GivenAnExistingBookingID()
        {
            _bookingId = BookingDataBuilder.CreateBookingAndReturnId();
        }

        [Given(@"a booking ID from data file ""(.*)"" that does not exist")]
        public void GivenNonExistentBookingIdFromFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
            var line = File.ReadAllText(path);               
            var parts = line.Split(',');                     
            var ids = parts.Skip(1)                
                           .Where(p => !string.IsNullOrWhiteSpace(p))
                           .Select(double.Parse)            
                           .ToList();
            _bookingId = (double)ids.First();
        }

        [Given(@"a booking ID of (.*)")]
        public void GivenBookingIdOf(string id)
        {
            if (int.TryParse(id, out var parsed))
                _bookingId = parsed;
            else
                _bookingId = -1; // fallback for invalid input
        }

        [When(@"I delete the booking")]
        public void WhenIDeleteTheBooking()
        {
            _response = _api.Delete($"/booking/{_bookingId}");
        }

        [When(@"I delete the booking without authentication")]
        public void WhenIDeleteWithoutAuthentication()
        {
            _api.ClearAuthToken();
            _response = _api.Delete($"/booking/{_bookingId}");
        }

        [When(@"I delete the booking with an invalid token")]
        public void WhenIDeleteWithInvalidToken()
        {
            _api.SetAuthToken("InvalidToken123");
            _response = _api.Delete($"/booking/{_bookingId}");
        }

        [When(@"I send a malformed DELETE payload from file ""(.*)""")]
        public void WhenISendMalformedDeletePayload(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
            var raw = File.ReadAllText(path);
            _response = _api.DeleteRaw($"/booking/{_bookingId}", raw);
        }

        [When(@"I send multiple parallel delete requests for that booking")]
        public void WhenISendParallelDeleteRequests()
        {
            var tasks = new List<Task<RestResponse>>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() => _api.Delete($"/booking/{_bookingId}")));
            }
            Task.WaitAll(tasks.ToArray());
            _parallelResponses = tasks.Select(t => t.Result).ToList();
        }
        
        [Then("the response status code must be (\\d+)")]
        public void ThenTheResponseStatusCodeMustBe(int expected)
        {
            var status = (int)_response.StatusCode;
            Assert.AreEqual(expected, (int)_response.StatusCode);
        }

        [Then(@"a subsequent GET request for that booking should return 404")]
        public void ThenSubsequentGetReturns404()
        {
            var getResponse = _api.Get($"/booking/{_bookingId}");
            Assert.AreEqual(404, (int)getResponse.StatusCode);
        }

        [Then("exactly one request should return (\\d+)")]
        public void ThenExactlyOneRequestShouldReturn(int expectedStatusCode)
        {
            var successCount = _parallelResponses.Count(r => (int)r.StatusCode == expectedStatusCode);
            Assert.AreEqual(1, successCount, $"Expected exactly one success, got {successCount}");
        }

        [Then("the remaining requests should return (\\d+)")]
        public void ThenTheRemainingRequestsShouldReturn(int expectedStatusCode)
        {
            var notFoundCount = _parallelResponses.Count(r => (int)r.StatusCode == expectedStatusCode);
            Assert.AreEqual(_parallelResponses.Count - 1, notFoundCount);
        }

    }
}


