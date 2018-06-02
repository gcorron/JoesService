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
        #region Private variables
        private List<CarModel> _carList;
        private BindingListCollectionView _sortedCars;
 
        private CarModel _fieldedCar;
        private bool _screenEditingMode;
        #endregion

        #region Events
        public event EventHandler<CarModel> SelectedCarChanged;
        public EventHandler<bool> ScreenStateChanged;
        #endregion

        #region Constructor
        public CarsViewModel()
        {
            BindingList<CarModel> _cars;

            _carList = DataAccess.GetCars();
            _carList.Sort(); 
            _cars =new BindingList<CarModel>(_carList); // load up cars from DB
            _cars.RaiseListChangedEvents = true;
            _cars.ListChanged += _cars_ListChanged;
            _sortedCars =  new BindingListCollectionView(_cars);
            _sortedCars.CurrentChanged += _sortedCars_CurrentChanged;
            _sortedCars.MoveCurrentToFirst();
        }
        #endregion

        #region Properties

        public BindingListCollectionView SortedCars
        {
            get { return _sortedCars; }
        }

        public CarModel FieldedCar
        {
            get { return _fieldedCar; }
            set
            {
                _fieldedCar = value;
                NotifyOfPropertyChange(() => FieldedCar);
                SelectedCarChanged?.Invoke(this, FieldedCar);
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

        #endregion
        #region Methods

        public void SelectFirstCar()
        {
            _sortedCars.MoveCurrentToFirst();
        }
     
        public void Edit()
        {
            _sortedCars.EditItem(_sortedCars.CurrentItem);
            ScreenEditingMode = true;
        }

        public void Add()
        {
            FieldedCar = _sortedCars.AddNew() as CarModel;
            ScreenEditingMode = true;
        }

        public void Save(string Fieldedcar_Make, string Fieldedcar_Model, string Fieldedcar_Owner, int Fieldedcar_Year)
        {

            bool isnew = _fieldedCar.CarID == 0;

            DataAccess.UpdateCar(_fieldedCar);
            if (isnew)
            { 
                _sortedCars.CommitNew();
                _sortedCars.MoveCurrentTo(_fieldedCar);
            }
            else
            {
                _sortedCars.CommitEdit();
            }
            _sortedCars.Refresh();
            ScreenEditingMode = false;
        }

        public void Cancel()
        {
            if (_sortedCars.IsAddingNew)
                _sortedCars.CancelNew();
            else if (_sortedCars.IsEditingItem)
                _sortedCars.CancelEdit();
            ScreenEditingMode = false;
        }

        #endregion
        #region Event Handlers
        private void _cars_ListChanged(object sender, ListChangedEventArgs e)
        {
            _carList.Sort();
            //_sortedCars.Refresh();
        }
        private void _sortedCars_CurrentChanged(object sender, EventArgs e)
        {
            FieldedCar = _sortedCars.CurrentItem as CarModel;
            SelectedCarChanged?.Invoke(this, FieldedCar);
        }
        #endregion
    }
}
