
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing the list of cars, including adding, deleting, and loading cars.
    /// Implements property change notifications.
    /// </summary>
    public class CarListVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CarListVM"/> class.
        /// Sets up the API service, commands, and loads cars from local database.
        /// </summary>
        public CarListVM()
        {
            _apiService = new APIService();

            IDeleteCar = new Command<CarModel>(DeleteCar);
            RequestCars();

            using (var db = new ReportsDBContextSQLite())
            {
                // Load cars ordered by Brand and then by Model into the observable collection
                var d = db.Cars.OrderBy(e => e.Brand).ThenBy(e => e.Model).ToList();
                d.ForEach(car => { Cars.Add(car); });
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Instance of the API service for server communication.
        /// </summary>
        private readonly APIService _apiService;

        private bool _isLoading;

        /// <summary>
        /// Indicates if data loading is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CarModel> _Cars = new();

        /// <summary>
        /// Collection of cars used for data binding.
        /// </summary>
        public ObservableCollection<CarModel> Cars
        {
            get => _Cars;
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

        /// <summary>
        /// Current car model used for add/edit operations.
        /// </summary>
        public CarModel Car
        {
            get => _Car;
            set
            {
                _Car = value;
                OnPropertyChanged(nameof(Car));
            }
        }

        private string _CarBrand;

        /// <summary>
        /// Brand of the car being added or edited.
        /// </summary>
        public string CarBrand
        {
            get => _CarBrand;
            set
            {
                _CarBrand = value;
                OnPropertyChanged(nameof(CarBrand));
            }
        }

        private string _CarModel;

        /// <summary>
        /// Model of the car being added or edited.
        /// </summary>
        public string CarModel
        {
            get => _CarModel;
            set
            {
                _CarModel = value;
                OnPropertyChanged(nameof(CarModel));
            }
        }

        private string _CarPlate;

        /// <summary>
        /// License plate of the car being added or edited.
        /// </summary>
        public string CarPlate
        {
            get => _CarPlate;
            set
            {
                _CarPlate = value;
                OnPropertyChanged(nameof(CarPlate));
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to add a new car.
        /// </summary>
        public ICommand IAddCar => new Command(() => AddCar());

        /// <summary>
        /// Command to delete a selected car.
        /// </summary>
        public ICommand IDeleteCar { get; }

        /// <summary>
        /// Command to clear all input properties.
        /// </summary>
        public ICommand IClean => new Command(() => CleanProperties());

        /// <summary>
        /// Command to view details of a specific car by its ID.
        /// Navigates to ViewCar page.
        /// </summary>
        public ICommand IViewIssue { get; } = new Command(async (id) =>
        {
            if (id is string CarId)
            {
                Data.StaticData.CarId = CarId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewCar(), true);
            }
        });

        /// <summary>
        /// Command to update the list of cars from the server.
        /// </summary>
        public ICommand Update => new Command(async () => await RequestCars());

        #endregion

        #region Methods

        /// <summary>
        /// Clears the input properties related to the car being added or edited.
        /// </summary>
        private void CleanProperties()
        {
            CarBrand = string.Empty;
            CarModel = string.Empty;
            CarPlate = string.Empty;
            Car = new();
        }

        /// <summary>
        /// Requests the list of cars from the API and updates the local database and observable collection.
        /// </summary>
        private async Task RequestCars()
        {
            try
            {
                IsLoading = true;
                var CarFromServer = await _apiService.GetAsync<List<CarModel>>("api/Cars", TimeSpan.FromSeconds(30));
                if (CarFromServer == null)
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
                await Application.Current.MainPage.DisplayAlert("Advertencia", ef.Message, "OK");
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error de la aplicación, vuelva a intentarlo", "OK");
                Console.WriteLine(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new car to the local database and attempts to upload it to the server.
        /// Performs validation on inputs to disallow spaces and underscores.
        /// </summary>
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
                        await UploadCar(Car);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "Los datos no pueden contener espacios, ni guión bajo", "OK");
                }
                CleanProperties();
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error de la aplicación, vuelva a intentarlo más tarde", "OK");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Uploads a car to the remote API server.
        /// Displays user notification based on success or failure.
        /// </summary>
        /// <param name="car">Car model to upload.</param>
        private async Task UploadCar(CarModel car)
        {
            try
            {
                var carAdded = await _apiService.PostAsync("api/Cars", car, TimeSpan.FromSeconds(5));
                if (carAdded)
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "Vehículo Agregado en el servidor", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "El vehículo no enviado al servidor", "OK");
                }
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Información", "Error al enviar al servidor\n Borre el vehículo e inténtelo de nuevo", "OK");
            }
        }

        /// <summary>
        /// Deletes a car from the local database and updates the observable collection.
        /// Shows alert if an error occurs.
        /// </summary>
        /// <param name="Car">Car model to delete.</param>
        private void DeleteCar(CarModel Car)
        {
            if (Car == null) return; // Avoid null arguments

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
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }

        #endregion
    }
}
