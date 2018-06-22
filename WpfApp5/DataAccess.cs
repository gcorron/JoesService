
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WpfApp5.ViewModels;
using Corron.CarService;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

namespace WpfApp5
{
    public static class DataAccess
    {

        public delegate void HandleError(string Message);

        static HttpClient client;

        public static void Initialize()
        {
            // get mode from App.config
            if (client is null)
            {
                client = new HttpClient(); // for Web API
                client.BaseAddress = new Uri("http://localhost:55679/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = System.TimeSpan.FromSeconds(1);
            }
        }

        public static List<CarModel> GetCars(HandleError handleError)
        {
            Initialize();
            List<CarModel> cars;
            try
            {
                if (true) // (client is null)
                {
                    using (IDbConnection connection = DataHelper.GetJoesDBConnection())
                    {
                        cars = connection.Query<CarModel>("SelectCars").ToList();
                    }
                }
                else
                {
                    cars = GetCarsAPI().GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                    handleError(e.Message);
                else
                    handleError(e.InnerException.Message);
                return null;
            }

            return cars;
        }

        public static async Task<List<CarModel>> GetCarsAPI()
        {
            using (var response = await client.GetAsync("api/Cars", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var reader = await response.Content.ReadAsAsync<List<CarModel>>().ConfigureAwait(false);
                return reader.AsList<CarModel>();
                //   var jsonAsString = await response.Content.ReadAsStringAsync();
                //   return JsonConvert.DeserializeObject<List<CarModel>>(jsonAsString);

            }
        }

        public static List<ServiceModel> GetServices(int CarID, HandleError handleError)
        {
            var lookup = new Dictionary<int, ServiceModel>();
            try
            {
                using (IDbConnection connection = DataHelper.GetJoesDBConnection())
                {
                    connection.Query<ServiceModel, ServiceLineModel, ServiceModel>
                        ($"SelectServicesForCar @CarID", (S, L) =>
                        {
                            ServiceModel SM;
                            if (!lookup.TryGetValue(S.ServiceID, out SM))
                            {
                                lookup.Add(S.ServiceID, SM = S);
                            }

                            SM.ServiceLineList.Add(L);
                            return SM;

                        }, new { CarID }, splitOn: "ServiceID");
                }
                return lookup.Values.ToList() as List<ServiceModel>;
            }
            catch (Exception e)
            {
                handleError(e.Message);
                return null;
            }
        }

        public static void UpdateCar(CarModel car, HandleError handleError)
        {
            try
            {
                using (IDbConnection connection = DataHelper.GetJoesDBConnection())
                {

                    List<int> results;
                    results = connection.Query<int>("dbo.UpdateCar @CarID, @Make, @Model, @Year, @Owner", car) as List<int>;

                    if (results[0] >= 0 && car.CarID <= 0)
                        car.CarID = results[0];
                }
            }
            catch (Exception e)
            {
                handleError(e.Message);
            }
        }

        public static bool UpdateService(ServiceModel service, HandleError handleError)
        {

            try
            {
                using (IDbConnection connection = DataHelper.GetJoesDBConnection())
                {
                    List<int> results;
                    results = connection.Query<int>("dbo.UpdateService @ServiceID, @ServiceDate, @TechName, @LaborCost, @PartsCost, @CarID", service) as List<int>;

                    if (results[0] >= 0 && service.ServiceID <= 0)
                        service.ServiceID = results[0];

                    List<ServiceLineModel> SL = service.ServiceLineList;
                    if (SL.Count > 255)
                        throw new Exception("Too many detail lines!");
                    else
                    {
                        byte b = 0;
                        foreach (ServiceLineModel s in SL)
                        {
                            s.ServiceLineOrder = b++; //save the order
                            s.ServiceID = service.ServiceID;
                        }
                        // stored procedure deletes all lines before inserting new ones
                        connection.Execute($"dbo.UpdateServiceLine @ServiceID, @ServiceLineOrder, @ServiceLineType, @ServiceLineDesc, @ServiceLineCharge", SL);
                    }
                }
            }
            catch (Exception e)
            {
                handleError(e.Message);
                return false;
            }
            return true;
        }
    }
    public static class DataHelper
    {
        public static IDbConnection GetJoesDBConnection()
        {
            return new System.Data.SqlClient.SqlConnection(CnnVal("Joes"));
        }

        public static string CnnVal(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch
            {
                throw new ArgumentException($"Database connection for {name} not found in app.config.");
            }
        }
    }
    // Tiny helper class
    public class NameValueByte
    {
        public ServiceLineModel.LineTypes Value { get; set; }
        public string Name { get; set; }
    }
}
