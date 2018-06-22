using System;
using System.Windows.Data;
using Corron.CarService;

namespace WpfApp5.ViewModels
{
    interface ICarsViewModel
    {
        CarModel FieldedCar { get; set; }
        bool NotScreenEditingMode { get; }
        bool ScreenEditingMode { get; set; }
        BindingListCollectionView SortedCars { get; }

        event EventHandler<CarModel> SelectedCarChanged;
        event EventHandler<bool> ScreenStateChanged;

        void Add();
        void Cancel();
        bool CanDelete(bool FieldedCar_HasService);
        bool CanSave(string Fieldedcar_Make, string Fieldedcar_Model, string Fieldedcar_Owner, int Fieldedcar_Year);
        void Delete(bool FieldedCar_HasService);
        void Edit();
        void Save(string Fieldedcar_Make, string Fieldedcar_Model, string Fieldedcar_Owner, int Fieldedcar_Year);
    }
}