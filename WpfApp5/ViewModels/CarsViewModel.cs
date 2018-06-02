using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WpfApp5.Models;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfApp5.ViewModels
{
    class CarsViewModel: Screen
    {
        //Private variables
        private BindingList<CarModel> _cars;
        private BindingListCollectionView _sortedCars;
 
        private CarModel _selectedCar;
        private CarModel _fieldedCar;
        private bool _screenEditingMode;

        //Events
        public event EventHandler<CarModel> SelectedCarChanged;
        public EventHandler<bool> ScreenStateChanged;


        //Constructor
        public CarsViewModel()
        {
            _cars=new BindingList<CarModel>(DataAccess.GetCars()); // load up cars from DB
            _sortedCars = new BindingListCollectionView(_cars);
            ///To do: figure out how to specify the sorting key

        }

        //Properties

        public BindingListCollectionView SortedCars {
            get { return _sortedCars; }
        }

 
        public CarModel SelectedCar
        {
            get
            {
                return _selectedCar;
            }
            set
            {
                _selectedCar = value;
                FieldedCar = value;
                NotifyOfPropertyChange(() => SelectedCar);
                SelectedCarChanged?.Invoke(this,SelectedCar);
            }
        }
 
        public CarModel FieldedCar
        {
            get { return _fieldedCar; }
            set {
                _fieldedCar = value;
                NotifyOfPropertyChange(() => FieldedCar);
            }
        }


        public bool CanSave(string Fieldedcar_Make, string Fieldedcar_Model, string Fieldedcar_Owner, int Fieldedcar_Year)
        {
            if (String.IsNullOrWhiteSpace(Fieldedcar_Make)) return false;
            if (String.IsNullOrWhiteSpace(Fieldedcar_Model)) return false;
            if (String.IsNullOrWhiteSpace(Fieldedcar_Owner)) return false;
            if (Fieldedcar_Year < 1900 || Fieldedcar_Year > 2050) return false;
            return true;
        }

        public bool ScreenEditingMode
        {
            get { return _screenEditingMode; }
            set
            {
                _screenEditingMode = value;
                NotifyOfPropertyChange("ScreenEditingMode");
                NotifyOfPropertyChange("NotScreenEditingMode");
                ScreenStateChanged?.Invoke(this, !_screenEditingMode);
            }
        }

        public bool NotScreenEditingMode
        {
            get { return !_screenEditingMode; }
        }

        //Methods        

        public void SelectFirstCar()
        {
            SelectedCar = _cars[0];
        }
     
        public void Edit()
        {
            FieldedCar = _selectedCar.ShallowCopy();
            ScreenEditingMode = true;
        }

        public void Add()
        {
            FieldedCar = new CarModel();
            ScreenEditingMode = true;
        }

        public void Save(string Fieldedcar_Make, string Fieldedcar_Model, string Fieldedcar_Owner, int Fieldedcar_Year)
        {
            int i;
            bool isnew = _fieldedCar.CarID == 0;

            DataAccess.UpdateCar(_fieldedCar);
            if (isnew)
                _cars.Add(_fieldedCar);
            else
            { 
                i = _cars.IndexOf(_selectedCar);
                _cars[i] = _fieldedCar;
            }
            SelectedCar = _fieldedCar;
            _sortedCars.Refresh();
            ScreenEditingMode = false;
        }

        public void Cancel()
        {
            FieldedCar = _selectedCar;
            ScreenEditingMode = false;
        }

        public void NewCar()
        {
            CarModel car = new CarModel();
            _cars.Add(car);
            SelectedCar = car;
        }

    }
}
