
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class CarListVM : INotifyPropertyChangedAbst
    {
        private readonly APIService _apiService;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
        public CarListVM()
        {
            _apiService = new APIService();

            IDeleteCar = new Command<CarModel>(DeleteCar);
            testServer();

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

        private async Task testServer()
        {
            try
            {
                IsLoading = true;
                var CarFromServer = await _apiService.GetAsync<List<CarModel>>("api/Cars");
                using (var db = new ReportsDBContextSQLite())
                {

                    foreach (var item in CarFromServer)
                    {
                        var Exist = (from C in db.Cars
                                     where C.Plate == item.Plate
                                     select C).Any();
                        if (!Exist)
                        {
                            this.Cars.Add(item);
                            db.Cars.Add(item);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se cargaron los vehículos desde el servidor", "OK");
                Console.WriteLine(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async void AddCar()
        {

            try
            {
                Car.Brand = Car.Brand.Trim();
                Car.Plate = Car.Plate.Trim();
                Car.Model = Car.Model.Trim();
                if (!CarBrand.Contains(' ') && !CarModel.Contains(' ') && !CarPlate.Contains(' ') || !CarBrand.Contains('_') && !CarModel.Contains('_') && !CarPlate.Contains('_'))
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        Car.Brand = CarBrand;
                        Car.Model = CarModel;
                        Car.Plate = CarPlate;
                        db.Cars.Add(Car);
                        db.SaveChanges();
                        Cars.Add(Car);
                        SendInfo(Car);
                    }

                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Información", "Los datos no pueden contener espacios, ni guión bajo", "OK");
                }
                CleanProperties();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private async Task SendInfo(CarModel car)
        {
            try
            {
                TimeSpan tmp = new TimeSpan(40);
                var carAdded = await _apiService.PostAsync("api/Cars", car, tmp);
            }
            catch (Exception)
            {
                Application.Current.MainPage.DisplayAlert("Información", "Error al enviar al servidor\n Borre el vehículo e intentelo de nuevo", "OK");
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
