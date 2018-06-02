using Caliburn.Micro;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp5.Models;


namespace WpfApp5.ViewModels
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private CarModel _selectedCar;
        private CarsViewModel _carsScreen;
        private ServicesViewModel _servicesScreen;
        private char _screentype;
        private bool _canChangeScreen;

        //Constructor
        public ShellViewModel()
        {
            CarsScreen = true;
            CanChangeScreen = true;
        }

        //Event Handlers
        private void OnScreenStateChanged(object sender, bool e)
        {
            CanChangeScreen = e;
        }

        private void OnSelectedCarChanged(object sender, CarModel e)
        {
            SelectedCar = e;
        }

        //Properties

            

        public bool CarsScreen
        {
            get
            {
                return (_screentype == 'C');
            }
            set
            {
                _screentype = 'C';
                NotifyOfPropertyChange("ServicesScreen");
                NotifyOfPropertyChange("CarsScreen");

                if (_carsScreen == null)
                {
                    _carsScreen = new CarsViewModel();
                    _carsScreen.SelectedCarChanged += OnSelectedCarChanged;
                    _carsScreen.SelectFirstCar();
                    _carsScreen.ScreenStateChanged += OnScreenStateChanged;
                }
                this.ActivateItem((IScreen)_carsScreen);
            }
        }

        public bool ServicesScreen
        {
            get
            {
                return (_screentype == 'S');
            }
            set
            {
                _screentype = 'S';
                NotifyOfPropertyChange("ServicesScreen");
                NotifyOfPropertyChange("CarsScreen");
                if (_servicesScreen == null)
                {
                    _servicesScreen = new ServicesViewModel(_carsScreen.SelectedCar);
                    _servicesScreen.ScreenStateChanged += OnScreenStateChanged;
                }
                this.ActivateItem((IScreen)_servicesScreen);
            }
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
                NotifyOfPropertyChange(() => SelectedCar);
            }

        }
   
        public bool CanChangeScreen
        {
            get { return _canChangeScreen; }
            set {
                _canChangeScreen = value;
                NotifyOfPropertyChange(()=>CanChangeScreen);
            }
        }
 
        public bool CanSaveCar(string selectedcar_make,string selectedcar_model, int selectedcar_year, string selectedcar_owner)
        {
                if (String.IsNullOrWhiteSpace(selectedcar_make)) return false;
                if (String.IsNullOrWhiteSpace(selectedcar_model)) return false;
                if (String.IsNullOrWhiteSpace(selectedcar_owner)) return false;
                if (selectedcar_year < 1900 || selectedcar_year > 2050) return false;
                return true;
        }

        //Methods

        public void SaveCar(string selectedcar_make, string selectedcar_model, int selectedcar_year, string selectedcar_owner)

        {
            DataAccess.UpdateCar(_selectedCar);
        }       
    }

}