using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp5.Models;

namespace WpfApp5.ViewModels
{
    class ServicesViewModel:Screen
    {
        //Private variables
        private BindableCollection<ServiceModel> _services;
        private ServiceModel _selectedService;
        private CarModel _selectedCar;
        private bool _screenEditingMode;

        //Events
        public event EventHandler<ServiceModel> SelectedServiceChanged;
        public EventHandler<bool> ScreenStateChanged;

        //Constructor
        public ServicesViewModel(CarModel selectedCar)
        {
            _selectedCar = selectedCar;
        }
        
        //Methods

        public void Add()
        {
            SelectedService=new ServiceModel(_selectedCar.CarID);
            ScreenEditingMode = true;
        }

        public void Cancel()
        {
            ScreenEditingMode = false;
        }

        //Properties

        public bool ScreenEditingMode
        {
            get { return _screenEditingMode; }
            set
            {
                _screenEditingMode = value;
                NotifyOfPropertyChange(() => ScreenEditingMode);
                NotifyOfPropertyChange(() => NotScreenEditingMode);
                ScreenStateChanged?.Invoke(this, !_screenEditingMode);
            }
        }

        public bool NotScreenEditingMode
        {
            get { return !_screenEditingMode; }
        }

        public ServiceModel SelectedService
        {
            get { return _selectedService; }
            set {
                _selectedService = value;
                NotifyOfPropertyChange(() => SelectedService);
            }
        }

    }
}
