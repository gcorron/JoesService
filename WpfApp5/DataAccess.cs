
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WpfApp5.ViewModels;
using WpfApp5.Models;

namespace WpfApp5
{
    public static class DataAccess
    {
        public static List<CarModel> GetCars()
        {
            using (IDbConnection connection = DataHelper.GetJoesDBConnection())
            {
                return connection.Query<CarModel>("SelectCars"). ToList();
            }
        }

        public static List<ServiceModel> GetServices(DateTime beginDate)
        {
            using (IDbConnection connection = DataHelper.GetJoesDBConnection())
            {
                return connection.Query<ServiceModel>($"SelectServices @beginDate='{beginDate:d}'").ToList();
            }

        }

        public static void UpdateCar(CarModel car)
        {
            using (IDbConnection connection = DataHelper.GetJoesDBConnection())
            {

                List<CarModel> cars = new List<CarModel>();

                cars.Add(car);

                int result = connection.Execute("dbo.UpdateCar @CarID, @Make, @Model, @Year, @Owner", cars);
                if (result > 0 && car.CarID == 0)
                    car.CarID = result;
            }
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
}
