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
    public class ServiceLineModel : PropertyChangedBase, IDataErrorInfo
    {

        private decimal _editLineCharge;
        private string _serviceLineDesc;
        private decimal _serviceLineCharge;
        private string _chargeString;

        public enum LineTypes { Labor = 'L', Parts = 'P' }
        const string MONEY_FORMAT = "{0:0.00}";

        public ServiceLineModel()
        {

        }

        public ServiceLineModel(LineTypes lineType)
        {
            ServiceLineType = lineType;
        }

        public ServiceLineModel(ServiceLineModel copy)
        {
            ObjectCopier.CopyFields(this, copy);
        }

        #region Properties

        public LineTypes ServiceLineType { get; set; }

        public string ServiceLineTypeString
        {
            get
            {
                return ServiceLineType.ToString();
            }
        }
        public string ServiceLineDesc
        {
            get { return _serviceLineDesc; }
            set { _serviceLineDesc = value;
                NotifyOfPropertyChange(()=>IsValidState);
            }
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
                NotifyOfPropertyChange();

                ValidateCostString(value, out _serviceLineCharge);
                NotifyOfPropertyChange(() => IsValidState);
            }
        }
        public bool IsValidState
        {
            get
            {
                if (!(this["ServiceLineDesc"] is null))
                    return false;
                if (!(this["ChargeString"] is null))
                    return false;
                return true;
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
                    case "ServiceLineDesc": return FiftyNoBlanks(ServiceLineDesc);
                    case "ChargeString": return ValidateCostString(ChargeString, out junk);
                }
                return null;
            }
        }
     }
    #endregion
}
