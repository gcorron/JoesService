using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corron.CarService;
using System.Data;
using System.Configuration;

namespace WpfApp5.Data
{
    public static class DataAccess
    {
       // public delegate void HandleError(string Message);

        private static bool _useSQL;

        public static string Initialize(SQLData.HandleError handleError)
        {
            string webAddress = Corron.CarService.SQLData.WebConnection();
            _useSQL = String.IsNullOrEmpty(webAddress);
            if (_useSQL)
            {
                SQLData.Initialize(handleError);
                return "Connected via SQL";
                }
            else
        	{
                WebClient.Initialize(handleError, webAddress);
                return "Connected via Web";
            }
        }

        public static List<CarModel> GetCars()
        {
            if (_useSQL)
                return SQLData.GetCars();
            else
                return WebClient.GetCars();
        }

        public static List<ServiceModel> GetServices(int CarID)
        {
            if (_useSQL)
                return SQLData.GetServices(CarID);
            else
                return WebClient.GetServices(CarID);
        }

        public static bool UpdateCar(CarModel car)
        {
            if (_useSQL)
                return SQLData.UpdateCar(car);
            else
            {
                return WebClient.UpdateCar(car);
            }
        }

        public static bool UpdateService(ServiceModel service)
        {
            if (_useSQL)
                return SQLData.UpdateService(service);
            else
                return WebClient.UpdateService(service);
        }

     }
}
