using Caliburn.Micro;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp5.Models;


namespace WpfApp5.ViewModels
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private ICarModel _selectedCar;
        private ICarsViewModel _carsScreen;
        private ServicesViewModel _servicesScreen;
        private char _screentype;
        private bool _canChangeScreen;

        //Constructor
        public ShellViewModel()
        {
            CarsScreen = true;
            CanChangeScreen = true;
            NotifyOfPropertyChange(() => ErrorMessageVisible);
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
                    _carsScreen = new CarsViewModel(ShowErrorMessage);
                    _carsScreen.SelectedCarChanged += OnSelectedCarChanged;
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
                if (_servicesScreen == null)
                {
                    _servicesScreen = new ServicesViewModel(ShowErrorMessage);
                    _servicesScreen.ScreenStateChanged += OnScreenStateChanged;
                }
                if (!_servicesScreen.LoadServiceData(_carsScreen.FieldedCar))
                    return; //error loading from DB, don't show services screen

                this.ActivateItem((IScreen)_servicesScreen);
                _screentype = 'S';
                NotifyOfPropertyChange("ServicesScreen");
                NotifyOfPropertyChange("CarsScreen");

            }
        }

        public ICarModel SelectedCar
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
            set
            {
                _canChangeScreen = value;
                NotifyOfPropertyChange(() => CanChangeScreen);
            }
        }

        public string ErrorMessage { get; set; }
        public Visibility ErrorMessageVisible
        {
            get {
                if (string.IsNullOrEmpty(ErrorMessage))
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }       
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessage = message;
            NotifyOfPropertyChange(() => ErrorMessage);
            NotifyOfPropertyChange(() => ErrorMessageVisible);
        }
        public void ClearError()
        {
            ShowErrorMessage("");
        }
    }

}