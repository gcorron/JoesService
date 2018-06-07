namespace WpfApp5.Models
{
    public interface ICarModel
    {
        string this[string columnName] { get; }

        int CarID { get; set; }
        string Error { get; set; }
        bool HasService { get; set; }
        string Make { get; set; }
        string Model { get; set; }
        string Owner { get; set; }
        string ToString { get; }
        int Year { get; set; }

        void BeginEdit();
        void CancelEdit();
        int CompareTo(CarModel rightCar);
        void EndEdit();
    }
}