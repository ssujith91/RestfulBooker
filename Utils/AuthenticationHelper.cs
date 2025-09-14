using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulBooker.Utils
{
    public static class AuthenticationHelper
    {
        public static string GenerateToken(string username = "admin", string password = "password123")
        {
            var client = new RestClient("https://restful-booker.herokuapp.com");
            var request = new RestRequest("/auth", Method.Post);
            request.AddJsonBody(new { username, password });


            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return json["token"]?.ToString();
        }
    }
}
