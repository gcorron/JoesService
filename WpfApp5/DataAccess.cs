using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corron.CarService;

namespace WpfApp5.Data
{


    public static class DataAccess
    {

        public delegate void HandleError(string Message);

        private static bool _useSQL;
 

        public static void Initialize(HandleError handleError)
        {
            _useSQL = true; //TODO get from config 
            if (_useSQL)
                SQLData.Initialize(handleError);
            else
                WebClient.Initialize(handleError);
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
            return SQLData.GetServices(CarID);
        }

        public static void UpdateCar(CarModel car)
        {
            SQLData.UpdateCar(car);
        }

        public static bool UpdateService(ServiceModel service)
        {
            return SQLData.UpdateService(service);
        }

     }
}
