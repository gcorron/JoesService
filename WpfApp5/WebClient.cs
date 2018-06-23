using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Corron.CarService;
using static WpfApp5.Data.DataAccess;

namespace WpfApp5.Data
{
    public static class WebClient
    {

        private static HttpClient _client;
        private static HandleError _handleError;

        public static void Initialize(HandleError handleError)
        {
            _handleError = handleError;               

            if (_client is null)
            {
                _client = new HttpClient(); // for Web API
                _client.BaseAddress = new Uri("http://localhost:55679/"); //TODO put in config file
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                _client.Timeout = System.TimeSpan.FromSeconds(5);
            }
        }

        public static List<CarModel> GetCars()
        {
            try
            {
                return GetCarsTask().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                    _handleError(e.Message);
                else
                    _handleError(e.InnerException.Message);
                return null;
            }
        }

        public static async Task<List<CarModel>> GetCarsTask()
        {
            using (var response = await _client.GetAsync("api/Cars", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var reader = await response.Content.ReadAsAsync<List<CarModel>>().ConfigureAwait(false);
                return reader.ToList<CarModel>();
            }
        }
    }
}
