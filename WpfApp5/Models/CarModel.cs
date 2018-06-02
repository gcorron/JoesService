using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace WpfApp5.Models
{
    public class CarModel : Caliburn.Micro.PropertyChangedBase
    {

        //Private variables
        private string _make;
        private string _model;
        private int _year;
        private string _owner;

        //Properties
        public int CarID { get; set; }

        public string Make
        {
            get { return _make; }
            set
            {
                _make = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public int Year
        {
            get { return _year; }
            set
            {
                _year = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }

        public string Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                NotifyOfPropertyChange();
                //                NotifyOfPropertyChange(() => ToString);
            }
        }

        public new string ToString
        {
            get
            {
                return $"{Year} {Make} {Model}";
            }
        }

 
        public CarModel ShallowCopy()
        {
           return (CarModel) this.MemberwiseClone();
        }

    }

}
