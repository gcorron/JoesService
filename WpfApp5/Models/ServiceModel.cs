using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp5.Models
{
    public class ServiceModel:PropertyChangedBase
    {

        //Private variables
        private DateTime _serviceDate;
        private string _serviceTech;
        private decimal _laborcost;
        private decimal _partsCost;

        //Constructor
        public ServiceModel(int carID)
        {
            CarID = carID;
            ServiceDate = DateTime.Today;
        }

        // Properties
        public int ServiceID { get; set; }
        public int CarID { get; set; }

        public DateTime ServiceDate
        {
            get { return _serviceDate; }
            set {
                _serviceDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public string ServiceTech
        {
            get { return _serviceTech; }
            set {
                _serviceTech = value;
                NotifyOfPropertyChange();
            }
        }
 
        public decimal LaborCost
        {
            get { return _laborcost; }
            set {
                _laborcost = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public decimal PartsCost
        {
            get { return _partsCost; }
            set
            {
                _partsCost = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public new string ToString
        {
            get { return $"{ServiceDate} {LaborCost + PartsCost}";  }

        }


    }
}
