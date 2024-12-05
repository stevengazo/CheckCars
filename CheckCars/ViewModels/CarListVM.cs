using CheckCars.Data;
using CheckCars.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class CarListVM : INotifyPropertyChangedAbst
    {
        public CarListVM()
        {
            IDeleteCar = new Command<CarModel>(DeleteCar);
            using (var db = new ReportsDBContextSQLite())
            {
                var d = db.Cars.ToList();
                d.ForEach(car => { Cars.Add(car); });
            }
        }
        #region Properties
        private ObservableCollection<CarModel> _Cars = new();
        public ObservableCollection<CarModel> Cars
        {
            get
            {
                return _Cars;
            }
            set
            {
                _Cars = value;
                if (_Cars != null)
                {
                    OnPropertyChanged(nameof(Cars));
                }
            }
        }
        private CarModel _Car = new()
        {
            Id = Guid.NewGuid().ToString(),
            Model = "",
            Plate = "",
            Brand = ""
        };
        public CarModel Car
        {
            get
            {
                return _Car;
            }
            set
            {
                _Car = value;
                OnPropertyChanged(nameof(Car));
            }
        }
        private string _CarBrand;
        public string CarBrand
        {
            get
            {
                return _CarBrand;
            }
            set
            {
                _CarBrand = value;
                OnPropertyChanged(nameof(CarBrand));
            }
        }
        private string _CarModel;
        public string CarModel
        {
            get
            {
                return _CarModel;
            }
            set
            {
                _CarModel = value;
                OnPropertyChanged(nameof(CarModel));
            }
        }
        private string _CarPlate;
        public string CarPlate
        {
            get
            {
                return _CarPlate;
            }
            set
            {
                _CarPlate = value;
                OnPropertyChanged(nameof(CarPlate));
            }
        }
        #endregion
        #region Commands
        public ICommand IAddCar
        {
            get
            {
                return new Command(() => AddCar());
            }
            private set { }
        }
        public ICommand IDeleteCar { get; }
        public ICommand IClean
        {
            get
            {
                return new Command(() => CleanProperties());
            }
            private set { }
        }
#endregion

#region Methods
        private void CleanProperties()
        {
            CarBrand = string.Empty;
            CarModel = string.Empty;
            CarPlate = string.Empty;
            Car = new();
        }
        private void AddCar()
        {

            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Car.Brand = CarBrand;
                    Car.Model = CarModel;
                    Car.Plate = CarPlate;
                    db.Cars.Add(Car);
                    db.SaveChanges();
                    Cars.Add(Car);                }
                CleanProperties();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void DeleteCar(CarModel e)
        {
            if (e == null) return; // Evitar argumentos nulos

            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    db.Cars.Remove(e);
                    db.SaveChanges();
                    Cars.Remove(e);
                }

            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, loguearla)
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }
#endregion
    }
}
