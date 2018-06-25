using Caliburn.Micro;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Corron.CarService;
using WpfApp5.Data;

namespace WpfApp5.ViewModels
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private ICarModel _selectedCar;
        private ICarsViewModel _carsScreen;
        private ServicesViewModel _servicesScreen;
        private char _screentype;
        private bool _canChangeScreen;
        private bool _waitingForCar;

        //Constructor
        public ShellViewModel()
        {
            ConnectMethod=DataAccess.Initialize(ShowErrorMessage);
            CarsScreen = true;
            CanChangeScreen = false;
            _waitingForCar = true;
            NotifyOfPropertyChange(() => ErrorMessageVisible);
        }

        //Event Handlers
        private void OnScreenStateChanged(object sender, bool e)
        {
            if (SelectedCar is null)
                _waitingForCar = true;
            else
                CanChangeScreen = e;
        }

        private void OnSelectedCarChanged(object sender, CarModel e)
        {
            SelectedCar = e;
            if (_waitingForCar)
            {
                CanChangeScreen = true;
                _waitingForCar = false;

            }
        }

        //Properties

        public string ConnectMethod { get; set; }

        public bool CarsScreen
        {
            get
            {
                return (_screentype == 'C');
            }
            set
            {
                _screentype = 'C';
                NotifyOfPropertyChange(() => ServicesScreen);
                NotifyOfPropertyChange(); 

                if (_carsScreen == null)
                {
                    _carsScreen = new CarsViewModel();
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
                    _servicesScreen = new ServicesViewModel();
                    _servicesScreen.ScreenStateChanged += OnScreenStateChanged;
                }
                if (!_servicesScreen.LoadServiceData(_carsScreen.FieldedCar))
                    return; //error loading from DB, don't show services screen

                this.ActivateItem((IScreen)_servicesScreen);
                _screentype = 'S';
                NotifyOfPropertyChange(() => CarsScreen);
                NotifyOfPropertyChange();
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
                NotifyOfPropertyChange();
            }

        }

        public bool CanChangeScreen
        {
            get { return _canChangeScreen; }
            set
            {
                _canChangeScreen = value;
                NotifyOfPropertyChange();
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