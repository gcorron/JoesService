using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp5.Models;
using static WpfApp5.DataAccess;

namespace WpfApp5.ViewModels
{
    class ServicesViewModel:Screen
    {
        #region Private variables, Events, Constructor


        private List<ServiceModel> _serviceList;
        private BindingList<ServiceModel> _services;
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
            foreach(ServiceModel SM in _serviceList)
            {
                SM.RecalcCost();
            }

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


 
        // Objects

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
                NotifyOfPropertyChange();
            }
        }

        public List<NameValueByte> ComboBoxTypes // a list of line type Name/Value pairs for the View
        {
            get
            {
                var LineTypesList = new List<NameValueByte>();
                LineTypesList.Add(new NameValueByte() { Name = "Labor", Value = ServiceLineModel.LineTypes.Labor });
                LineTypesList.Add(new NameValueByte() { Name = "Parts", Value = ServiceLineModel.LineTypes.Parts });
                return LineTypesList;
            }
        }

        //Misc. State
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

        //Actions

        public void Edit()
        {
            _sortedServices.EditItem(_sortedServices.CurrentItem);

            ScreenEditingMode = true;
        }
        public bool CanEdit // keep as property!
        {
            get
            {
                return _sortedServices.Count > 0;
            }
        }

        public void Add()
        {
            if (_sortedServices.Count == 0)
                _listBookMark = -1;
            else
                _listBookMark = _sortedServices.CurrentPosition;
            FieldedService = _sortedServices.AddNew() as ServiceModel;
            FieldedService.CarID = _car.CarID;
            FieldedService.ServiceDate = DateTime.Today;
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
        public void Save(bool FieldedService_IsValidState, bool screenEditingMode)
        {

            bool isnew = _fieldedService.ServiceID == 0;

            _fieldedService.ServiceLineList.RemoveAll(SL => SL.Delete != 0);


            DataAccess.UpdateService(_fieldedService, _handleError);
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
            //_fieldedService.ServiceLines.Refresh();
            NotifyOfPropertyChange(() => CanDelete);
            ScreenEditingMode = false;
        }
        public bool CanSave(bool FieldedService_IsValidState, bool screenEditingMode)
        {
            return FieldedService_IsValidState && ScreenEditingMode;
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
                if (!_sortedServices.MoveCurrentToPosition(_listBookMark))
                    return;
                FieldedService = _sortedServices.CurrentItem as ServiceModel;
            }
        }

        public bool CanDelete // keep as property!
        {
            get
            {
                return _sortedServices.Count > 0;
            }
        }
    }

    // Tiny helper class
    public class NameValueByte
    {
        public ServiceLineModel.LineTypes Value { get; set; }
        public string Name { get; set; }
    }

}
