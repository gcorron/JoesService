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


        const string MONEY_FORMAT = "{0:0.00}";
        public readonly string[] _validateProperties = { "TechName", "ServiceDate" };


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


        public ServiceLineModel CurrentServiceLine { get; set; }
        public List<ServiceLineModel> ServiceList
            { get { return _serviceLines; } }

        public int ServiceID { get; set; }
        public int CarID { get; set; }

        public DateTime ServiceDate
        {
            get { return _serviceDate; }
            set {
                _serviceDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsValidState);
            }
        }

        public string TechName
        {
            get { return _techName; }
            set {
                _techName = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(()=>IsValidState);
            }
        }

        public decimal LaborCost
        {
            get { return _laborCost; }
            set {_laborCost = value; }
        }

        public string LaborCostString
        {
            get
            {
                return String.Format(MONEY_FORMAT, _laborCost);
            }
        }

        public decimal PartsCost
        {
            get { return _partsCost; }
            set { _partsCost = value; }
        }

        public string PartsCostString
        {
            get
            {
                return String.Format(MONEY_FORMAT,_partsCost);
            }
 
        }

         public decimal TotalCost
        {
            get
            {
                return _partsCost + _laborCost;
            }
        }

        public bool IsValidState
        {
            get
            {
                foreach (string s in _validateProperties)
                {
                    if (!(this[s] is null))
                        return false;
                }
                return ServiceList.Any(sv => !sv.IsValidState);
            }
        }


 

        #endregion
        #region Methods

        public void AddPartsLine()
        {
            AddServiceLine(new ServiceLineModel(LineTypes.Parts));
        }

        public void AddLaborLine()
        {
            AddServiceLine(new ServiceLineModel(LineTypes.Labor));
        }

        public void AddServiceLine(ServiceLineModel serviceLine)
        {
            _serviceLines.Add(serviceLine);
            CurrentServiceLine = serviceLine;
            NotifyOfPropertyChange(() => IsValidState);
            NotifyOfPropertyChange(() => CurrentServiceLine);
        }

        public void RemoveServiceLine(ServiceLineModel serviceLine)
        {
            _serviceLines.Remove(serviceLine);
            NotifyOfPropertyChange(() => IsValidState);
            RecalcCost();
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
                switch (columnName)
                {
                    case "TechName": return FiftyNoBlanks(TechName);
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
            _editServiceLines = CopyDetailLines(_serviceLines);
            NotifyOfPropertyChange(()=>IsValidState);
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
            _serviceLines=CopyDetailLines(_editServiceLines);
        }

        private List<ServiceLineModel> CopyDetailLines(List<ServiceLineModel> from)
        {
            List<ServiceLineModel> to = new List<ServiceLineModel>();
            foreach (ServiceLineModel item in from)
            {
                to.Add(new ServiceLineModel(item));
            }
            return to;
        }

        #endregion

    }
}
