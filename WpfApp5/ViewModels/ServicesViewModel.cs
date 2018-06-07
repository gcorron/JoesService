using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WpfApp5.Models;
using static WpfApp5.DataAccess;

namespace WpfApp5.ViewModels
{
    class ServicesViewModel:Screen
    {
        #region Private variables, Events, Constructor
        private List<ServiceModel> _serviceList;
        BindingList<ServiceModel> _services;
        private BindingListCollectionView _sortedServices;

        private ServiceModel _fieldedService;
        private bool _screenEditingMode;
        private HandleError _handleError;
        private CarModel _car;
        private int _listBookMark;

        public EventHandler<bool> ScreenStateChanged;

        //Constructor
        public ServicesViewModel(HandleError handleError)
        {
            _handleError = handleError;
        }

        //Load Data, always call on activation!
        public bool LoadServiceData(CarModel car)
        {
            _car = car;

            _serviceList = DataAccess.GetServices(car.CarID, _handleError); // load up Services from DB
            if (_serviceList is null)
                return false;
            _serviceList.Sort();

            //Binding
            _services = new BindingList<ServiceModel>(_serviceList);
            _services.RaiseListChangedEvents = true;

            _sortedServices = new BindingListCollectionView(_services);
            _sortedServices.MoveCurrentToFirst();
            _fieldedService = _sortedServices.CurrentItem as ServiceModel;
            NotifyOfPropertyChange(() => SortedServices);
            NotifyOfPropertyChange(() => CanDelete);
            return true;
        }

        #endregion


        #region Properties

        public BindingListCollectionView SortedServices
        {
            get { return _sortedServices; }
        }

        public ServiceModel FieldedService
        {
            get { return _fieldedService; }
            set
            {
                _fieldedService = value;
                NotifyOfPropertyChange(() => FieldedService);
            }
        }

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

        public bool CanDelete
        {
            get
            {     
                return _sortedServices.Count > 0;
            }
        }

        #endregion

        #region Methods

        public void SelectFirstCar()
        {
            _sortedServices.MoveCurrentToFirst();
        }

        public void Edit()
        {
            _sortedServices.EditItem(_sortedServices.CurrentItem);
            ScreenEditingMode = true;
        }

        public void Add()
        {
            if (_sortedServices.Count == 0)
                _listBookMark = -1;
            else
                _listBookMark = _sortedServices.CurrentPosition;
            FieldedService = _sortedServices.AddNew() as ServiceModel;
            FieldedService.CarID = _car.CarID;
            ScreenEditingMode = true;
        }
        public void Delete()
        {
            if (MessageBox.Show("Do you want to Delete this service?", "Confirm",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _fieldedService.ServiceID = -_fieldedService.ServiceID;
                if (DataAccess.UpdateService(_fieldedService, _handleError))
                {
                    _sortedServices.Remove(_fieldedService);
                    _sortedServices.Refresh();
                    NotifyOfPropertyChange(() => CanDelete);
                }
            }

        }

        public void Save(DateTime fieldedService_ServiceDate, string fieldedService_TechName, decimal fieldedService_PartsCostString, decimal fieldedService_LaborCostString)
        {

            bool isnew = _fieldedService.ServiceID == 0;

            DataAccess.UpdateService(_fieldedService,_handleError);
            if (isnew)
            {
                _sortedServices.CommitNew();
                _sortedServices.MoveCurrentTo(_fieldedService);
            }
            else
            {
                _sortedServices.CommitEdit();
            }
            _serviceList.Sort();
            _sortedServices.Refresh();
            NotifyOfPropertyChange(() => CanDelete);
            ScreenEditingMode = false;
        }

        public bool CanSave(DateTime fieldedService_ServiceDate, string fieldedService_TechName, decimal fieldedService_PartsCostString, decimal fieldedService_LaborCostString)
        {

            if (NotScreenEditingMode)
                return true;



            string[] _validateProperties = { "ServiceTech", "ServiceDate", "LaborCostString", "PartsCostString" };


            foreach (string s in _validateProperties)
            {
                if (!(_fieldedService[s] is null))
                    return false;
            }
            return true;
        }

        public void Cancel()
        {
            ScreenEditingMode = false;
            if (_sortedServices.IsAddingNew)
                _sortedServices.CancelNew();
            else if (_sortedServices.IsEditingItem)
                _sortedServices.CancelEdit();
            if (_listBookMark >= 0)
            {
                _sortedServices.MoveCurrentTo(_listBookMark);
                _fieldedService = _sortedServices.CurrentItem as ServiceModel;
            }
        }
        #endregion

    }
}
