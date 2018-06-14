using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using static WpfApp5.Models.ServiceLineModel;
using static WpfApp5.Models.Validation;

namespace WpfApp5.Models
{
    public class ServiceModel : PropertyChangedBase, IComparable<ServiceModel>, IDataErrorInfo, IEditableObject
    {
 
        private ServiceModel _editCopy;
        private List<ServiceLineModel> _editServiceLines;

 
        const string MONEY_FORMAT = "{0:0.00}";
        public readonly string[] _validateProperties = { "TechName", "ServiceDate" };


        // Constructors
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
            _serviceLineList = new List<ServiceLineModel>();
        }

        // Properties
        public ServiceLineModel CurrentServiceLine { get; set; }

        public List<ServiceLineModel> ServiceLineList
        {
            get
            {
                return
                _serviceLineList;
            }
        }
        private List<ServiceLineModel> _serviceLineList;


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
        private DateTime _serviceDate;


        public string TechName
        {
            get { return _techName; }
            set {
                _techName = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(()=>IsValidState);
            }
        }
        private string _techName;

        public decimal LaborCost
        {
            get { return _laborCost; }
            set
            {
                _laborCost = value;
                NotifyOfPropertyChange(()=>LaborCostString);
            }
        }
        private decimal _laborCost;

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
            set
            {
                _partsCost = value;
                NotifyOfPropertyChange(()=>PartsCostString);
            }
        }
        private decimal _partsCost;

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
                if (_validateProperties.Any(s => !(this[s] is null)))
                    return false;
                return true;
            }
        }

        public bool ServiceLinesAreValidState //property is notified whenever fields change within the collection
        {
            get
            {
                if (ServiceLineList is null)
                    return false;
                return ServiceLineList.Any(s => !s.IsValidState);
            }
        }


        private BindingList<ServiceLineModel> _blServiceLines;
        private BindingListCollectionView _cvServiceLines;

        public BindingListCollectionView ServiceLines
        {
            get {
                if (_cvServiceLines is null)
                    BindServiceLineList();
                return _cvServiceLines;
            }
        }

        private void BindServiceLineList()
        {
            //Binding detail lines
            _blServiceLines = new BindingList<ServiceLineModel>(ServiceLineList);

            //need to keep totals updated whenever a detail line changes
            _blServiceLines.RaiseListChangedEvents = true;
            _blServiceLines.ListChanged += new System.ComponentModel.ListChangedEventHandler(OnListChanged);

            _cvServiceLines = new BindingListCollectionView(_blServiceLines);
            _cvServiceLines.Refresh();
 
        }
 

        internal void RecalcCost()
        {
            decimal pCost=0, lCost=0;

            foreach(ServiceLineModel serviceLine in _serviceLineList)
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


         private void OnListChanged(object sender, System.ComponentModel.ListChangedEventArgs args)
        {
            int i, j;
            decimal lc, pc;

            i = args.NewIndex;
            j = -1;

            if (i >= 0)
                j = args.OldIndex;
            else
                i = args.OldIndex;

            if (i < 0)
                return;

            var SLL = ServiceLineList;

            if (i >= SLL.Count)
                return;

            switch (args.ListChangedType)
            {
                case ListChangedType.ItemChanged:

                    decimal[] changes = SLL[i].ChargeChanges();

                    lc = LaborCost + changes[0];
                    pc = PartsCost + changes[1];

                    if (j >= 0)
                    {
                        changes = SLL[j].ChargeChanges();
                        lc -= changes[0];
                        pc -= changes[1];
                    }
                    LaborCost = lc;
                    PartsCost = pc;
                    break;
            }
        }

        // Implements IComparable
        public int CompareTo(ServiceModel rightService)
        {
            ServiceModel leftService = this;
            return leftService.ServiceDate.CompareTo(rightService.ServiceDate);
        }

        // Implements IDataErrorInfo
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
      
        // Implements IEditableObject
        public void BeginEdit()
        {
            //make a copy of the original in case cancels
            ObjectCopier.CopyFields(_editCopy = new ServiceModel(0), this);
            CopyDetailLines(ref _editServiceLines,_serviceLineList);
            foreach (ServiceLineModel SL in _serviceLineList)
            {
                SL.SnapShotCharge();
            }

            NotifyOfPropertyChange(()=>IsValidState);
        }

        public void EndEdit()
        {
            _editCopy = null;
            _editServiceLines = null;
        }

        public void CancelEdit()
        {
            if (_cvServiceLines.IsAddingNew)
                _cvServiceLines.CancelNew();
            else if (_cvServiceLines.IsEditingItem)
                _cvServiceLines.CancelEdit();

            ObjectCopier.CopyFields(this, _editCopy);
            _editCopy = null;

            CopyDetailLines(ref _serviceLineList,_editServiceLines);

            _cvServiceLines.Refresh();
        }

        private void CopyDetailLines(ref List<ServiceLineModel> to, List<ServiceLineModel> from)
        {
            if (to is null)
                to = new List<ServiceLineModel>();
            else
                to.Clear();
            foreach (ServiceLineModel item in from)
            {
                to.Add(new ServiceLineModel(item));
            }
        }


    }
}
