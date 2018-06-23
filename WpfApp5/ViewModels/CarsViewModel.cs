using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Corron.CarService;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using WpfApp5.Data;
using System.Diagnostics;

namespace WpfApp5.ViewModels
{
    class CarsViewModel : Screen, ICarsViewModel
    {
        private List<CarModel> _carList;
        private int _listBookMark;


        public event EventHandler<CarModel> SelectedCarChanged;
        public event EventHandler<bool> ScreenStateChanged;

        public CarsViewModel()
        {
            BindingList<CarModel> _cars;

            _carList = DataAccess.GetCars();
            if (_carList is null)
                return;
            Debug.Assert(_carList[0].CarID != 0); //bad data

 
            _carList.Sort();
            _cars = new BindingList<CarModel>(_carList); // load up cars from DB
            _cars.RaiseListChangedEvents = true;
            _sortedCars =  new BindingListCollectionView(_cars);
        }

        // Properties

        public BindingListCollectionView SortedCars
        {
            get { return _sortedCars; }
        }
        private BindingListCollectionView _sortedCars;

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
        private CarModel _fieldedCar;

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
        private bool _screenEditingMode;


        public bool NotScreenEditingMode
        {
            get { return !_screenEditingMode; }
        }

        // Methods

        public void Edit()
        {
            _sortedCars.EditItem(_sortedCars.CurrentItem);
            ScreenEditingMode = true;
        }

        public void Delete(bool FieldedCar_HasService)
        {
            if (MessageBox.Show("Do you want to Delete this car?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _fieldedCar.CarID = -_fieldedCar.CarID;
                try
                {
                    DataAccess.UpdateCar(_fieldedCar);
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $"Database Error: {e.Message}");
                    return;                
                }
                if (_fieldedCar.CarID == 0)
                {
                    _sortedCars.Remove(_fieldedCar);
                    _sortedCars.Refresh();
                }
            }

        }

        public bool CanDelete(bool FieldedCar_HasService)
        {
            return FieldedCar_HasService == false;
        }

        public void Add()
        {
            FieldedCar = _sortedCars.AddNew() as CarModel;
            _listBookMark = _sortedCars.CurrentPosition;
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
            _carList.Sort();
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

            if (_listBookMark >= 0)
            {
                if (!_sortedCars.MoveCurrentToPosition(_listBookMark))
                    return;
                FieldedCar = _sortedCars.CurrentItem as CarModel;
            }
        }

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
    }
}
