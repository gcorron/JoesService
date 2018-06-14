using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static WpfApp5.Models.Validation;

namespace WpfApp5.Models
{
    public class ServiceLineModel : PropertyChangedBase, IDataErrorInfo
    {

        public enum LineTypes : byte { Labor, Parts };
        const string MONEY_FORMAT = "{0:0.00}";

        private decimal[] _calcLineCharge;
        private static LineTypes _lastLineType;

        //constructors
        public ServiceLineModel()
        {
            ServiceLineType = _lastLineType;
            ServiceLineDesc = "< Enter Description >";
            ServiceLineCharge = 0;
            _calcLineCharge = new decimal[] {0M,0M};
        }

        public ServiceLineModel(ServiceLineModel copy)
        {
            ObjectCopier.CopyFields(this, copy);
        }

 
        //properties

        public int ServiceID { get; set; } //used only for saving with dapper

        public byte ServiceLineOrder { get; set; } //used only to retrieve from storage in original order entered

        public LineTypes ServiceLineType
        {
            get { return _serviceLineType; }
            set
            {
                _serviceLineType = value;
                _lastLineType = value;
                NotifyOfPropertyChange();
            }
        }
        private LineTypes _serviceLineType;

        public string ServiceLineTypeString
        {
            get { return new[] { "Labor", "Parts" }[(int)_serviceLineType]; }
        }

        public string ServiceLineDesc
        {
            get { return _serviceLineDesc; }
            set
            {
                _serviceLineDesc = value;
                NotifyOfPropertyChange(() => IsValidState);
            }
        }
        private string _serviceLineDesc;

        public decimal ServiceLineCharge
        {
            get { return _serviceLineCharge; }
            set
            {
                _serviceLineCharge = value;
                if (ServiceLineCharge == 0)
                    ChargeString = "";
                else
                    ChargeString = String.Format(MONEY_FORMAT, _serviceLineCharge);
            }
        }
        private decimal _serviceLineCharge;

        public string ChargeString
        {
            get
            {
                return _chargeString;
            }
            set
            {
                _chargeString = value;
                NotifyOfPropertyChange();

                ValidateCostString(value, out _serviceLineCharge);
                NotifyOfPropertyChange(() => IsValidState);
            }
        }
        private string _chargeString;

        public byte Delete {
            get
            {
                return _delete;
            }
            set
            {
                _delete = value;
                NotifyOfPropertyChange();
            }
        }
        private byte _delete;

        public bool IsValidState
        {
            get
            {
                if (new string[] { "ServiceLineDesc", "ChargeString" }.Any(s => !(this[s] is null)))
                    return false;
                else
                    return true;
            }
        }

        // Methods
        public void SnapShotCharge() //allows efficient recalc whenever total charges are recalculated
        {
            if (Delete !=0)
            {
                _calcLineCharge[0] = 0M; _calcLineCharge[1] = 0M;
                return;
            }

            _calcLineCharge[0] = (_serviceLineType == LineTypes.Labor && Delete == 0) ? _serviceLineCharge : 0;
            _calcLineCharge[1] = (_serviceLineType == LineTypes.Parts && Delete == 0) ? _serviceLineCharge : 0;
        }

        public decimal[] ChargeChanges() //retrieves changes in line charge since the last snapshot, then takes a new snapshot
        {
            decimal[] change = new decimal[] { 0M , 0M } ;


            if (_serviceLineType == LineTypes.Labor && Delete == 0)
                change[0] = _serviceLineCharge - _calcLineCharge[0];
            else
                change[0] =  - _calcLineCharge[0];

            if (_serviceLineType == LineTypes.Parts && Delete == 0 )
                change[1] = _serviceLineCharge - _calcLineCharge[1];
            else
                change[1] = -_calcLineCharge[1];

            SnapShotCharge();
            return change;
        }


        //IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                decimal junk;
                switch (columnName)
                {
                    case "ServiceLineDesc": return FiftyNoBlanks(ServiceLineDesc);
                    case "ChargeString": return ValidateCostString(ChargeString, out junk);
                    default:
                        return "Invalid Column Name";
                }
            }
        }

        //experimentally used, no longer needed

        private bool CanDeleteRow()
        {
            // Verify command can be executed here
            return false;
        }

        private void DeleteRowObject()
        {
            MessageBox.Show("HI");
            //return true;
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                if (_deleteRowCommand == null)
                {
                    _deleteRowCommand = new RelayCommand(
                        param => CanDeleteRow(),
                        param => DeleteRowObject()
                    );
                }
                return _deleteRowCommand;
            }
        }
        private ICommand _deleteRowCommand;

    }
}
