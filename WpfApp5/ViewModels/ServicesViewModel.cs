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
using Corron.CarService;
using WpfApp5.Data;

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
        private CarModel _car;
        private int _listBookMark;

        public EventHandler<bool> ScreenStateChanged;

        //Load Data, always call on activation!
        public bool LoadServiceData(CarModel car)
        {
            _car = car;

            _serviceList = DataAccess.GetServices(car.CarID); // load up Services from DB
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
                _cvServiceLines = null; //force refresh of bindings for detail lines
                NotifyOfPropertyChange();
                if (!(ServiceLines is null))
                    NotifyOfPropertyChange(() => ServiceLines); 
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

        //detail lines binding

        private BindingList<ServiceLineModel> _blServiceLines;
        private BindingListCollectionView _cvServiceLines;

        public BindingListCollectionView ServiceLines
        {
            get
            {
                if (FieldedService is null)
                    return null;
                if (_cvServiceLines is null)
                    BindServiceLineList();
                return _cvServiceLines;
            }
        }

        private void BindServiceLineList()
        {
            //Binding detail lines
            _blServiceLines = new BindingList<ServiceLineModel>(FieldedService.ServiceLineList);
//            _blServiceLines.RaiseListChangedEvents = true;
            _cvServiceLines = new BindingListCollectionView(_blServiceLines);
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

        //Command Methods

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
                if (DataAccess.UpdateService(_fieldedService))
                {
                    _sortedServices.Remove(_fieldedService);
                    _sortedServices.Refresh();
                    NotifyOfPropertyChange(() => CanDelete);
                }
            }
        }
        public bool CanDelete // keep as property!
        {
            get
            {
                return _sortedServices.Count > 0;
            }
        }


        public void Save(bool fieldedService_IsValidState, bool screenEditingMode)
        {

            bool isnew = _fieldedService.ServiceID == 0;

            _fieldedService.ServiceLineList.RemoveAll(SL => SL.Delete != 0);


            if (!DataAccess.UpdateService(_fieldedService))
                return;

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
            ServiceLines.Refresh();
            NotifyOfPropertyChange(() => CanDelete);
            NotifyOfPropertyChange(() => CanEdit);
            ScreenEditingMode = false;
        }
        public bool CanSave(bool fieldedService_IsValidState, bool screenEditingMode)
        {
            if (ScreenEditingMode == false)
                return false;

            return FieldedService.IsValidState;
        }

        public void Cancel()
        {

            if (_sortedServices.IsAddingNew)
                _sortedServices.CancelNew();
            else if (_sortedServices.IsEditingItem)
                _sortedServices.CancelEdit();
            _sortedServices.Refresh(); //prevent exceptions downstream

            ScreenEditingMode = false;
            NotifyOfPropertyChange(() => ServiceLines);
 
            //if (_listBookMark >= 0)
            //{
            //    if (!_sortedServices.MoveCurrentToPosition(_listBookMark))
            //        return;
            //    FieldedService = _sortedServices.CurrentItem as ServiceModel;
            //}
        }
     }
    // Tiny helper class
    public class NameValueByte
    {
        public ServiceLineModel.LineTypes Value { get; set; }
        public string Name { get; set; }
    }
}
