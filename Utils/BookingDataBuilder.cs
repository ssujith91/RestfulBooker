using Newtonsoft.Json.Linq;
using RestSharp;
using RestfulBooker.Utils;

namespace RestfulBooker.Utils
{
    public static class BookingDataBuilder
    {
        private static readonly ApiClient _api = ApiClient.Instance;

        /// <summary>
        /// Creates a new booking with default or provided values and returns the booking ID.
        /// </summary>
        public static int CreateBookingAndReturnId(
            string firstname = "Test",
            string lastname = "User",
            int totalprice = 100,
            bool depositpaid = true,
            string checkin = "2025-01-01",
            string checkout = "2025-01-10")
        {
            var payload = new
            {
                firstname,
                lastname,
                totalprice,
                depositpaid,
                bookingdates = new
                {
                    checkin,
                    checkout
                }
            };

            var response = _api.Post("/booking", payload);
            var json = JObject.Parse(response.Content);
            return (int)json["bookingid"];
        }
    }
}
