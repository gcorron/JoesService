using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfApp5.Models.ServiceLineModel;
using static WpfApp5.Models.Validation;

namespace WpfApp5.Models
{
    public class ServiceModel : PropertyChangedBase, IComparable<ServiceModel>, IDataErrorInfo, IEditableObject
    {
        #region Private variables

        private List<ServiceLineModel> _serviceLines;
        private ServiceModel _editCopy;
        private List<ServiceLineModel> _editServiceLines;
        private DateTime _serviceDate;
        private string _techName;
        private decimal _laborCost;
        private decimal _partsCost;
        private string _laborCostString;
        private string _partsCostString;

        const string MONEY_FORMAT = "{0:0.00}";
        private readonly string[] _validateProperties = { "ServiceTech", "ServiceDate", "LaborCostString", "PartsCostString" };


        #endregion

        #region Constructor
        public ServiceModel(int carID)
        {
            CarID = carID;
            Initialize();
        }

        public ServiceModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            ServiceDate = DateTime.Today;
            _serviceLines = new List<ServiceLineModel>();
        }

        #endregion

        #region Properties

        public IEnumerable<ServiceLineModel> ServiceLines
        {
            get
            {
                return _serviceLines;
            }
        }

        public int ServiceID { get; set; }
        public int CarID { get; set; }

        public DateTime ServiceDate
        {
            get { return _serviceDate; }
            set {
                _serviceDate = value;
                NotifyOfPropertyChange();
            }
        }

        public string TechName
        {
            get { return _techName; }
            set {
                _techName = value;
                NotifyOfPropertyChange();
            }
        }

        public decimal LaborCost
        {
            get { return _laborCost; }
            set {
                _laborCost = value;
                LaborCostString = String.Format(MONEY_FORMAT, _laborCost);
            }
        }

        public string LaborCostString
        {
            get
            {
                return _laborCostString;
            }
            set
            {
                _laborCostString = value;
                NotifyOfPropertyChange();
                if (ValidateCostString(value, out _laborCost) is null)
                {
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => TotalCost);
                }
            }
        }

        public decimal PartsCost
        {
            get { return _partsCost; }
            set
            {
                _partsCost = value;
                PartsCostString = String.Format(MONEY_FORMAT, _partsCost);
            }
        }

        public string PartsCostString
        {
            get
            {
                return _partsCostString;
            }
            set
            {
                _partsCostString = value;

                if (ValidateCostString(value, out _partsCost) is null)
                {
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => TotalCost);
                }
            }
        }

        public string CostsString
        {
            get
            {
                return $"Parts: {PartsCost:C2}  Labor: {LaborCost:C2}";
            }
        }

         public decimal TotalCost
        {
            get
            {
                return _partsCost + _laborCost;
            }
        }


        #endregion
        #region Methods

        public void AddServiceLine(ServiceLineModel serviceLine)
        {
            _serviceLines.Add(serviceLine);
            RecalcCost();
        }

        public void RemoveServiceLine(ServiceLineModel serviceLine)
        {
            _serviceLines.Remove(serviceLine);
            RecalcCost();
        }

        private bool ValidState()
        {
 
            foreach (string s in _validateProperties)
            {
                if (!(this[s] is null))
                    return false;
            }
            return true;
        }
        private void RecalcCost()
        {
            decimal pCost=0, lCost=0;

            foreach(ServiceLineModel serviceLine in _serviceLines)
            {
                switch (serviceLine.ServiceLineType)
                {
                    case LineTypes.Labor:
                        lCost += serviceLine.ServiceLineCharge;
                        break;
                    case LineTypes.Parts:
                        pCost += serviceLine.ServiceLineCharge;
                        break;
                }

            }
            LaborCost = lCost; PartsCost = pCost;
        }


        #endregion  
        #region Implements IComparable
        public int CompareTo(ServiceModel rightService)
        {
            ServiceModel leftService = this;
            return leftService.ServiceDate.CompareTo(rightService.ServiceDate);
        }
        #endregion
        #region Implements IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            
            get
            {
                decimal junk;
                switch (columnName)
                {
                    case "TechName": return FiftyNoBlanks(TechName);
                    case "LaborCostString":return ValidateCostString(LaborCostString,out junk);
                    case "PartsCostString":return ValidateCostString(PartsCostString,out junk);
                    case "ServiceDate":
                        if (ServiceDate.Year < 2010 || ServiceDate.Year > 2050) return "Date out of range.";
                        break;
                }
                return null;
            }
        }

      
        #endregion

        #region Implements IEditableObject
        public void BeginEdit()
        {
            //make a copy of the original in case cancels
            ObjectCopier.CopyFields(_editCopy = new ServiceModel(0), this);
            CopyDetailLines(_serviceLines, _editServiceLines);
        }

        public void EndEdit()
        {
            _editCopy = null;
            _editServiceLines = null;
        }

        public void CancelEdit()
        {
            ObjectCopier.CopyFields(this, _editCopy);
            _editCopy = null;
            CopyDetailLines(_editServiceLines, _serviceLines);
        }

        private void CopyDetailLines(List<ServiceLineModel> from, List<ServiceLineModel> to)
        {
            to = new List<ServiceLineModel>();
            foreach (ServiceLineModel item in from)
            {
                to.Add(new ServiceLineModel(item));
            }

        }

        #endregion

    }
}
