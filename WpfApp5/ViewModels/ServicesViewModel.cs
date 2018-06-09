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
        private BindingList<ServiceModel> _services;
        private BindingListCollectionView _sortedServices;

        private BindingList<ServiceLineModel> _serviceLines;
        private BindingListCollectionView _sortedServiceLines;

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

                if (_serviceLines is null)
                {
                    //Binding detail lines
                    _serviceLines = new BindingList<ServiceLineModel>(_fieldedService.ServiceList);
                    _serviceLines.RaiseListChangedEvents = true;
                    _sortedServiceLines = new BindingListCollectionView(new BindingList<ServiceLineModel>(_serviceLines));
                    _sortedServiceLines.Refresh();
                    NotifyOfPropertyChange(() => ServiceLines);
                }
            }
        }

 
        public BindingListCollectionView ServiceLines
        {
            get { return _sortedServiceLines; }
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

        public bool CanEdit // keep as property!
        {
            get
            {
                return _sortedServices.Count > 0;
            }
        }

        public bool CanDelete // keep as property!
        {
            get
            {
                return _sortedServices.Count > 0;
            }
        }

        #endregion

        #region Methods

 
         public void SelectFirstService()
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

        #region Detail Line Methods
        public void AddPartsLine(ServiceLineModel ServiceLines_CurrentItem)
        { AddServiceLine(ServiceLineModel.LineTypes.Parts); }

        public void AddLaborLine(ServiceLineModel ServiceLines_CurrentItem)
        { AddServiceLine(ServiceLineModel.LineTypes.Labor); }

        public void AddServiceLine(ServiceLineModel.LineTypes lineType)
        {
            //FieldedService.AddServiceLine(new ServiceLineModel(lineType));

            ServiceLineModel serviceLine = _sortedServiceLines.AddNew() as ServiceLineModel;
            serviceLine.ServiceLineType = lineType;
            // _sortedServiceLines.Refresh();
        }

        public bool CanAddPartsLine(bool FieldedService_IsValidState, bool ScreenEditingMode,ServiceLineModel CurrentServiceLine)
        {
            return CanAddDetailLine(CurrentServiceLine);
        }

        public bool CanAddLaborLine(bool FieldedService_IsValidState, bool ScreenEditingMode, ServiceLineModel CurrentServiceLine)
        {
            return CanAddDetailLine(CurrentServiceLine);
        }

        public bool CanAddDetailLine(ServiceLineModel CurrentServiceLine)
        {
            if (!ScreenEditingMode) return false;
            if (!FieldedService.IsValidState) return false;

            if (FieldedService is null) return false;
            if (!(FieldedService.IsValidState)) return false;
            if (CurrentServiceLine is null) return true;
            if (CurrentServiceLine.IsValidState) return true;
            return false;
        }

#endregion
        public void Save(bool FieldedService_IsValidState, bool ScreenEditingMode)
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
        public bool CanSave(bool FieldedService_IsValidState,bool ScreenEditingMode)
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
 
            if (_sortedServiceLines.IsAddingNew)
                _sortedServiceLines.CancelNew();
            else if (_sortedServiceLines.IsEditingItem)
                _sortedServiceLines.CancelEdit();

            if (_listBookMark >= 0)
            {
                _sortedServices.MoveCurrentTo(_listBookMark);
                _fieldedService = _sortedServices.CurrentItem as ServiceModel;
            }
            _sortedServiceLines.Refresh();
        }
        #endregion

    }
}
