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
    public class CarModel : Caliburn.Micro.PropertyChangedBase, IComparable<CarModel>, IDataErrorInfo, IEditableObject
    {

        #region Private variables
        private string _make;
        private string _model;
        private int _year;
        private string _owner;
        private CarModel _editCopy;
        #endregion

        #region Properties
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

        public new string ToString
        {
  

            get
            {
                if (_editCopy is null)
                    return $"{Year} {Make} {Model}";
                else
                    return $"{_editCopy.Year} {_editCopy.Make} {_editCopy.Model}";
            }
        }

        #endregion

        #region Implements IComparable
        public int CompareTo(CarModel rightCar)
        {
            CarModel leftCar = this;
            return leftCar.ToString.CompareTo(rightCar.ToString);
        }
        #endregion
        #region Implements IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName] {
            get
            {
                switch (columnName) {
                    case "Make":return FiftyNoBlanks(Make);
                    case "Model":return FiftyNoBlanks(Model);
                    case "Owner":return FiftyNoBlanks(Owner);
                    case "Year":
                        if (Year < 1900 || Year > 2050)
                            return "Year out of range.";
                        break;
                }
                return null;
            }
        }
        private string FiftyNoBlanks(string Test)
        {
            if (String.IsNullOrWhiteSpace(Test))
                return "Field must not be blank.";
            else if (Test.Length > 50)
                return "Field is too long.";
            else
                return null;
        }


        #endregion
        #region Implements IEditableObject
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

        #endregion

    }
}
