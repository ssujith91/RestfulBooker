using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulBooker.Utils
{
    public class ApiClient
    {
        private static readonly Lazy<ApiClient> lazy = new(() => new ApiClient());
        public static ApiClient Instance => lazy.Value;


        private readonly RestClient _client;
        private string _token;
        public Dictionary<string, string> LastQueryParameters { get; private set; }

        private ApiClient()
        {
            _client = new RestClient("https://restful-booker.herokuapp.com");
            LastQueryParameters = new Dictionary<string, string>();
        }


        public void SetAuthToken(string token)
        {
            _token = token;
        }
        public void ClearAuthToken()
        {
            _token = null;
        }
        private RestRequest CreateRequest(string endpoint, Method method, object body = null, bool useAuth = true)
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("accept", "application/json");
            if (useAuth && !string.IsNullOrEmpty(_token))
                request.AddHeader("Cookie", $"token={_token}");
            if (body != null)
                request.AddJsonBody(body);
            return request;
        }
        public RestResponse Get(string endpoint, Dictionary<string, string> queryParams)
        {
            var request = CreateRequest(endpoint, Method.Get);

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    request.AddQueryParameter(param.Key, param.Value);
                    LastQueryParameters = new Dictionary<string, string>(queryParams);
                }
            }
            else
            {
                LastQueryParameters.Clear();
            }
            
            return _client.Execute(request);
        }
        
        public RestResponse Execute(RestRequest request)
        {
            return _client.Execute(request);
        }
        public RestResponse Get(string endpoint) 
        {
            var request = new RestRequest(endpoint, Method.Get);
            request.AddHeader("accept", "application/json");
            return _client.Execute(request); 
        }
        public RestResponse Post(string endpoint, object body)
        {
            var request = new RestRequest(endpoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("accept", "application/json");
            request.AddJsonBody(body);
            if (!string.IsNullOrEmpty(_token))
            {
                request.AddHeader("Cookie", $"token={_token}");
            }
            return _client.Execute(request);
            //return _client.Execute(CreateRequest(endpoint, Method.Post, body, false));
        }
        public RestResponse Patch(string endpoint, object body) => _client.Execute(CreateRequest(endpoint, Method.Patch, body));
        public RestResponse Delete(string endpoint) => _client.Execute(CreateRequest(endpoint, Method.Delete));
        public RestResponse DeleteRaw(string endpoint, string rawJson)
        {
            var request = new RestRequest(endpoint, Method.Delete);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("accept", "application/json");
            request.AddStringBody(rawJson, DataFormat.Json);

            if (!string.IsNullOrEmpty(_token))
                request.AddHeader("Cookie", $"token={_token}");

            return _client.Execute(request);
        }
    }
}
