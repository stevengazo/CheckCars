﻿
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class CarListVM : INotifyPropertyChangedAbst
    {
        
        #region Constructor
     
        public CarListVM()
        {
            _apiService = new APIService();

            IDeleteCar = new Command<CarModel>(DeleteCar);
            RequestCars();

            using (var db = new ReportsDBContextSQLite())
            {
                var d = db.Cars.OrderBy(e=>e.Brand).OrderBy(e=>e.Model).ToList();
                d.ForEach(car => { Cars.Add(car); });
            }
        }

        #endregion

        #region Properties

        private readonly APIService _apiService;
      
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
       
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
            CarId = Guid.NewGuid().ToString(),
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

        public ICommand IViewIssue { get; } = new Command( async (id) =>
        {
            if( id is string CarId)
            {
                Data.StaticData.CarId = CarId;
                await Application.Current.MainPage.Navigation.PushAsync( new ViewCar() , true);
            }
        } 
        );
        #endregion

        #region Methods
        private void CleanProperties()
        {
            CarBrand = string.Empty;
            CarModel = string.Empty;
            CarPlate = string.Empty;
            Car = new();
        }
        private async Task RequestCars()
        {
            try
            {
                IsLoading = true;
                var CarFromServer = await _apiService.GetAsync<List<CarModel>>("api/Cars", TimeSpan.FromSeconds(30));
                if(CarFromServer == null)
                {
                    throw new NullReferenceException("No fue posible obtener vehículos desde el servidor");
                }
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
            catch (NullReferenceException ef)
            {
                Application.Current.MainPage.DisplayAlert("Advertencia", ef.Message, "OK");
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error de la aplicación, vuelva a intentarlo", "OK");
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
                        UploadCar(Car);
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
                Application.Current.MainPage.DisplayAlert("Error", "Error de la aplicación, vuelva a intentarlo màs tarde", "OK");
                Console.WriteLine(e.Message);
//                throw;
            }
        }
        private async Task UploadCar(CarModel car)
        {
            try
            {
              
                var carAdded = await _apiService.PostAsync("api/Cars", car, TimeSpan.FromSeconds(5));
                if (carAdded)
                {
                    Application.Current.MainPage.DisplayAlert("Información", "Vehículo Agregado en el servidor", "OK");
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Información", "El vehículo no envidado al servidor", "OK");
                }
            }
            catch (Exception  e)
            {
                Application.Current.MainPage.DisplayAlert("Información", "Error al enviar al servidor\n Borre el vehículo e intentelo de nuevo", "OK");
            }
        }
        private void DeleteCar(CarModel Car)
        {
            if (Car == null) return; // Evitar argumentos nulos

            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    db.Cars.Remove(Car);
                    db.SaveChanges();
                    Cars.Remove(Car);
                }

            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert(CarPlate, "Error al eliminar el vehículo", "OK");
                // Manejar la excepción (por ejemplo, loguearla)
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }
        #endregion
    }
}
