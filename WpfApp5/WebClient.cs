using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Corron.CarService;

namespace WpfApp5.Data
{
    public static class WebClient
    {

        private static HttpClient _client;
        private static SQLData.HandleError _handleError;

        public static void Initialize(SQLData.HandleError handleError,string WebAddress)
        {
            _handleError = handleError;               

            if (_client is null)
            {
                _client = new HttpClient();
                _client.BaseAddress = new Uri(WebAddress);
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

        public static List<ServiceModel> GetServices(int CarID)
        {
            try
            {
                return GetServicesTask(CarID).GetAwaiter().GetResult();
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

        public static async Task<List<ServiceModel>> GetServicesTask(int CarID)
        {
            using (var response = await _client.GetAsync($"api/Services?id={CarID}", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var reader = await response.Content.ReadAsAsync<List<ServiceModel>>().ConfigureAwait(false);
                return reader.ToList<ServiceModel>();
            }
        }
        public static bool UpdateCar(ICarModel car)
        {
            try
            {
                switch(car.CarID.CompareTo(0))
                {
                    case -1:
                        car.CarID = DeleteCarTask(car.CarID).GetAwaiter().GetResult();
                        break;
                    case 0:
                        car.CarID = PostCarTask(car).GetAwaiter().GetResult();
                        break;
                    case 1:
                        car.CarID = PutCarTask(car).GetAwaiter().GetResult();
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                    _handleError(e.Message);
                else
                    _handleError(e.InnerException.Message);
                return false;
            }
            return true;
        }
        public static async Task<int> PutCarTask(ICarModel car)
        {
            using (var response = await _client.PutAsJsonAsync<ICarModel>($"api/Cars?id={car.CarID}",car).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
              int id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
              return id;
            }
        }

        public static async Task<int> PostCarTask(ICarModel car)
        {
            using (var response = await _client.PostAsJsonAsync<ICarModel>($"api/Cars?id={car.CarID}", car).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                int id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
                return id;
            }
        }

        public static async Task<int> DeleteCarTask(int id)
        {
            using (var response = await _client.DeleteAsync($"api/Cars?id={id}").ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
                return id;
            }
        }



        public static bool UpdateService(ServiceModel service)
        {
            try
            {
                switch (service.ServiceID.CompareTo(0))
                {
                    case -1:
                        service.ServiceID = DeleteServiceTask(service.ServiceID).GetAwaiter().GetResult();
                        break;
                    case 0:
                        service.ServiceID = PostServiceTask(service).GetAwaiter().GetResult();
                        break;
                    case 1:
                        service.ServiceID = PutServiceTask(service).GetAwaiter().GetResult();
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                    _handleError(e.Message);
                else
                    _handleError(e.InnerException.Message);
                return false;
            }
            return true;
        }
        public static async Task<int> PutServiceTask(ServiceModel service)
        {
            using (var response = await _client.PutAsJsonAsync<ServiceModel>($"api/Services?id={service.ServiceID}", service).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                int id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
                return id;
            }
        }

        public static async Task<int> PostServiceTask(ServiceModel service)
        {
            using (var response = await _client.PostAsJsonAsync<ServiceModel>($"api/Services?id={service.CarID}", service).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                int id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
                return id;
            }
        }

        public static async Task<int> DeleteServiceTask(int id)
        {
            using (var response = await _client.DeleteAsync($"api/Services?id={id}").ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                id = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);
                return id;
            }
        }


    }
}
