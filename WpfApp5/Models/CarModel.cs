using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp5.Models
{
    public class CarModel : Caliburn.Micro.PropertyChangedBase, IComparable<CarModel>, IDataErrorInfo, IEditableObject, ICarModel
    {

        private CarModel _editCopy;

        public int CarID { get; set; }

        public string Make
        {
            get { return _make; }
            set
            {
                _make = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }
        private string _make;

        public string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }
        private string _model;


        public int Year
        {
            get { return _year; }
            set
            {
                _year = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => ToString);
            }
        }
        private int _year;

        public string Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                NotifyOfPropertyChange();
                //                NotifyOfPropertyChange(() => ToString);
            }
        }
        private string _owner;


        public new string ToString
        {
            get
            {
                if (_editCopy is null)
                    return $"{Owner}'s {Year} {Make} {Model}";
                else
                    return $"{_editCopy.Owner}'s {_editCopy.Year} {_editCopy.Make} {_editCopy.Model}";
            }
        }

        public bool HasService { get; set; }


        //IComparable
        public int CompareTo(CarModel rightCar)
        {
            int result;

            CarModel leftCar = this;
            result = leftCar.Year.CompareTo(rightCar.Year);
            if (result==0)
                result = leftCar.Make.CompareTo(rightCar.Make);
            if (result == 0)
                result = leftCar.Model.CompareTo(rightCar.Model);


            return result;
        }

        // IDataErrorInfo
        public string Error
        {
            get { return _error; }
            set { _error = value; }
        }
        private string _error;

        public string this[string columnName] {
            get
            {
                switch (columnName) {
                    case "Make":return Validation.FiftyNoBlanks(Make);
                    case "Model":return Validation.FiftyNoBlanks(Model);
                    case "Owner":return Validation.FiftyNoBlanks(Owner);
                    case "Year":
                        if (Year < 1900 || Year > 2050)
                            return "Year out of range.";
                        break;
                }
                return null;
            }
        }

        // IEditableObject
        public void BeginEdit()
        {
            //make a copy of the original in case cancels
            ObjectCopier.CopyFields(_editCopy = new CarModel(), this);
        }

        public void EndEdit()
        {
            _editCopy = null;
        }

        public void CancelEdit()
        {
            ObjectCopier.CopyFields(this, _editCopy);
            _editCopy = null;
        }

    }
}
