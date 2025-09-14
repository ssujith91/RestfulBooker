using Reqnroll;
using RestfulBooker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulBooker.Hooks
{
    [Binding]
    public sealed class Hooks
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ApiClient _api = ApiClient.Instance;
        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var token = AuthenticationHelper.GenerateToken();
            ApiClient.Instance.SetAuthToken(token);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var createdBookingIds = _scenarioContext.ContainsKey("CreatedBookingIds")
                ? (List<int>)_scenarioContext["CreatedBookingIds"]
                : new List<int>();
            foreach (var id in createdBookingIds)
            {
                _api.Delete($"/booking/{id}");
            }
            
        }
    }
}
