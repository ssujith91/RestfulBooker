using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework;
using Reqnroll;
using RestfulBooker.Utils;
using RestfulBooker.Utils;
using RestSharp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace RestfulBooker.Steps
{
    [Binding]
    public class BookingE2ETestsSteps
    {
        private readonly ApiClient _api = ApiClient.Instance;
        private List<int> _createdBookingIds = new();
        private RestResponse _response;
        private int _currentBookingId;
        private JObject _latestBookingPayload;
        private readonly ScenarioContext _scenarioContext;
        public BookingE2ETestsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }


        [Given(@"I create a new booking with the following data:")]
        [When(@"I create a new booking with the following data:")]
        public void WhenICreateANewBooking(Table table)
        {
            foreach (var row in table.Rows)
            {
                // var payload = table.Rows.ToDictionary(r => r["firstname"], r => (object)r["lastname"]);
               var payload = new Dictionary<string, object>
            {
                {"firstname", row["firstname"]},
                {"lastname", row["lastname"]},
                {"totalprice", int.Parse(row["totalprice"])},
                {"depositpaid", bool.Parse(row["depositpaid"])} ,
                {"bookingdates", new {
                        checkin = row["checkin"],
                        checkout = row["checkout"] }
                }
            };
                _latestBookingPayload = JObject.FromObject(payload);
                _response = _api.Post("/booking", payload);
                if (_response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Failed to create booking. Status: {_response.StatusCode}, Content: {_response.Content}");
                }
                var json = JObject.Parse(_response.Content);
                _currentBookingId = (int)json["bookingid"];
                _createdBookingIds.Add(_currentBookingId);
            }
            _scenarioContext["CreatedBookingIds"] = _createdBookingIds;
            _scenarioContext["Response"] = _response;
        }

        [Then(@"the booking should be created successfully")]
        public void ThenBookingCreatedSuccessfully()
        {
            Assert.IsNotNull(_currentBookingId);
            Assert.Greater(_currentBookingId, 0);
        }

        [Then("the response statuscode should be {int}")]
        public void ThenTheResponseStatuscodeShouldBe(int expectedStatusCode)
        {
            var response = _scenarioContext["Response"] as RestResponse;
            Assert.That((int)response.StatusCode, Is.EqualTo(expectedStatusCode),
                $"Expected status code {expectedStatusCode} but got {(int)response.StatusCode}");
        }

        [When(@"I update the booking with:")]
        public void WhenIUpdateBookingWith(Table table)
        {
            var payload = table.Rows.ToDictionary(r => r["field"], r => (object)r["value"]);
            _latestBookingPayload = JObject.FromObject(payload);
            _response = _api.Patch($"/booking/{_currentBookingId}", payload);
            _scenarioContext["Response"] = _response;
        }

        [Then(@"the fields should be updated accordingly")]
        public void ThenFieldsShouldBeUpdatedAccordingly()
        {
            _response = _scenarioContext["Response"] as RestResponse;
            var json = JObject.Parse(_response.Content);
            foreach (var kvp in _latestBookingPayload)
            {
                if (json.ContainsKey(kvp.Key))
                    Assert.AreEqual(kvp.Value.ToString(), json[kvp.Key]?.ToString());
            }
        }

        [When(@"I retrieve the booking by ID")]
        public void WhenIRetrieveBookingById()
        {
            _response = _api.Get($"/booking/{_currentBookingId}");
            
        }

        [Then(@"the booking details should reflect the latest updates")]
        public void ThenBookingDetailsShouldReflectUpdates()
        {
            _response = _scenarioContext["Response"] as RestResponse;
            var json = JObject.Parse(_response.Content);
            foreach (var kvp in _latestBookingPayload)
            {
                if (json.ContainsKey(kvp.Key))
                    Assert.AreEqual(kvp.Value.ToString(), json[kvp.Key]?.ToString());
            }
                 
        }
        
        [When("I delete booking")]
        public void WhenIDeleteBooking()
        {
            _response = _api.Delete($"/booking/{_currentBookingId}");
        }

        [When(@"I create multiple bookings from data file ""(.*)""")]
        public void WhenICreateMultipleBookings(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
            var jsonArray = JArray.Parse(File.ReadAllText(path));
            foreach (var booking in jsonArray)
            {
                var payload = JsonConvert.SerializeObject(booking);

                var response = _api.Post("/booking", payload);
                var created = JObject.Parse(response.Content);
                _createdBookingIds.Add((int)created["bookingid"]);     
               
            }
        }

        [Then(@"the response status code should be 200 for all bookings")]
        public void ThenStatusCode200ForAll()
        {
            Assert.IsTrue(_createdBookingIds.Count > 0);
        }

        [When(@"I filter bookings by firstname ""(.*)""")]
        public void WhenIFilterBookingsByFirstname(string firstname)
        {
            _response = _api.Get($"/booking?firstname={firstname}");
        }

        [Then(@"at least one booking ID should be returned")]
        public void ThenAtLeastOneBookingIdShouldBeReturned()
        {
            var json = JArray.Parse(_response.Content);
            Assert.IsTrue(json.Count > 0);
        }

        [When(@"I update all filtered bookings with:")]
        public void WhenIUpdateAllFilteredBookings(Table table)
        {
            var payload = table.Rows.ToDictionary(r => r["field"], r => (object)r["value"]);
            foreach (var id in _createdBookingIds)
            {
                _api.Patch($"/booking/{id}", payload);
            }
        }

        [Then(@"all bookings should be updated with the new totalprice")]
        public void ThenAllBookingsShouldBeUpdated()
        {
            foreach (var id in _createdBookingIds)
            {
                var response = _api.Get($"/booking/{id}");
                var json = JObject.Parse(response.Content);
                Assert.AreEqual("999", json["totalprice"]?.ToString());
            }
        }

        [When(@"I delete all filtered bookings")]
        public void WhenIDeleteAllFilteredBookings()
        {
            foreach (var id in _createdBookingIds)
            {
                _api.Delete($"/booking/{id}");
            }
        }

        [Then(@"the response status code should be 201 for all deletions")]
        public void ThenStatusCode201ForAllDeletions()
        {
            foreach (var id in _createdBookingIds)
            {
                var response = _api.Get($"/booking/{id}");
                Assert.AreEqual(404, (int)response.StatusCode);
            }
        }

        [Then(@"all bookings should no longer exist when retrieved")]
        public void ThenAllBookingsShouldNoLongerExist()
        {
            foreach (var id in _createdBookingIds)
            {
                var response = _api.Get($"/booking/{id}");
                Assert.AreEqual(404, (int)response.StatusCode);
            }
        }

        [Then("a subsequent GET request for that bookingId should return {int}")]
        public void ThenASubsequentGETRequestForThatBookingIdShouldReturn(int expectedstatuscode)
        {
            var getResponse = _api.Get($"/booking/{_currentBookingId}");
            Assert.AreEqual(expectedstatuscode, (int)getResponse.StatusCode);
        }

        public void CleanupBookings()
        {
            var createdBookingIds = _scenarioContext.ContainsKey("CreatedBookingIds") 
                ? (List<int>)_scenarioContext["CreatedBookingIds"] 
                : new List<int>();
            foreach (var id in createdBookingIds)
            {
                _api.Delete($"/booking/{id}");
            }
            createdBookingIds.Clear();
        }
    }
}
