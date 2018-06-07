using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfApp5.Models.Validation;

namespace WpfApp5.Models
{
    public class ServiceLineModel : PropertyChangedBase, IDataErrorInfo, IEditableObject
    {

        private decimal _editLineCharge;
        private decimal _serviceLineCharge;
        private string _chargeString;

        public enum LineTypes { Labor = 'L', Parts = 'P' }
        const string MONEY_FORMAT = "{0:0.00}";

        #region Properties
        public LineTypes ServiceLineType { get; set; }
        public string ServiceLineDesc { get; set; }

        public ServiceLineModel()
        {

        }
        public ServiceLineModel(ServiceLineModel copy)
        {
            ObjectCopier.CopyFields(this, copy);
        }

        public decimal ServiceLineCharge
        {
            get { return _serviceLineCharge; }
            set
            {
                _serviceLineCharge = value;
                ChargeString = String.Format(MONEY_FORMAT, _serviceLineCharge);
            }
        }

        public string ChargeString
        {
            get
            {
                return _chargeString;
            }
            set
            {
                _chargeString = value;

                if (ValidateCostString(value, out _serviceLineCharge) is null)
                {
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion


        #region Interface Implementations

        //IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {

            get
            {
                decimal junk;
                switch (columnName)
                {
                    case "ServiceDesc": return FiftyNoBlanks(ServiceLineDesc);
                    case "ChargeString": return ValidateCostString(ChargeString, out junk);
                }
                return null;
            }
        }

        //IEditableObject
        public void BeginEdit()
        {
            _editLineCharge = ServiceLineCharge;
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }
    }
#endregion
}
