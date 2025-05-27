using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Views;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for viewing car details and associated reports.
    /// </summary>
    public class ViewCarVM : INotifyPropertyChangedAbst
    {
        #region Fields

        private readonly APIService _apiService = new();

        #endregion

        #region Commands

        /// <summary>
        /// Navigates to the AddBooking page.
        /// </summary>
        public ICommand AddBooking => new Command(async () =>
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddBooking());
        });

        /// <summary>
        /// Prompts the user for confirmation before deleting a car.
        /// </summary>
        public ICommand DeleteCar => new Command(async () =>
        {
            await Application.Current.MainPage.DisplayPromptAsync(
                "Eliminar Vehículo",
                "¿Estás seguro de eliminar este vehículo?",
                "Eliminar",
                "Cancelar",
                "Escribe 'eliminar' para confirmar",
                2,
                keyboard: Keyboard.Create(KeyboardFlags.CapitalizeCharacter));
        });

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the ViewModel, loads vehicle data and associated reports.
        /// </summary>
        public ViewCarVM()
        {
            var id = StaticData.CarId;
            using (var db = new ReportsDBContextSQLite())
            {
                Vehicle = db.Cars.FirstOrDefault(e => e.CarId == id);
            }

            RequestExists();
            IssuesExists();
            ReturnsExists();
        }

        #endregion

        #region Properties

        private CarModel _Vehicle;

        /// <summary>
        /// Gets or sets the vehicle details.
        /// </summary>
        public CarModel Vehicle
        {
            get => _Vehicle;
            set
            {
                if (_Vehicle != value)
                {
                    _Vehicle = value;
                    OnPropertyChanged(nameof(Vehicle));
                }
            }
        }

        private ObservableCollection<EntryExitReport> _ExistsReports = new();

        /// <summary>
        /// Gets or sets the entry/exit reports related to the vehicle.
        /// </summary>
        public ObservableCollection<EntryExitReport> ExistsReports
        {
            get => _ExistsReports;
            set
            {
                if (_ExistsReports != value)
                {
                    _ExistsReports = value;
                    OnPropertyChanged(nameof(ExistsReports));
                }
            }
        }

        private ObservableCollection<IssueReport> _IssuesReports = new();

        /// <summary>
        /// Gets or sets the issue reports related to the vehicle.
        /// </summary>
        public ObservableCollection<IssueReport> IssuesReports
        {
            get => _IssuesReports;
            set
            {
                if (_IssuesReports != value)
                {
                    _IssuesReports = value;
                    OnPropertyChanged(nameof(IssuesReports));
                }
            }
        }

        private ObservableCollection<VehicleReturn> _ReturnReports = new();

        /// <summary>
        /// Gets or sets the return reports related to the vehicle.
        /// </summary>
        public ObservableCollection<VehicleReturn> ReturnReports
        {
            get => _ReturnReports;
            set
            {
                if (_ReturnReports != value)
                {
                    _ReturnReports = value;
                    OnPropertyChanged(nameof(ReturnReports));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads entry/exit reports for today and yesterday.
        /// </summary>
        private async void RequestExists()
        {
            try
            {
                ExistsReports.Clear();

                var today = await _apiService.GetAsync<List<EntryExitReport>>(
                    $"api/EntryExitReports/search?date={DateTime.Today:yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (today != null)
                {
                    foreach (var i in today)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        ExistsReports.Add(i);
                    }
                }

                var yesterday = await _apiService.GetAsync<List<EntryExitReport>>(
                    $"api/EntryExitReports/search?date={DateTime.Today.AddDays(-1):yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (yesterday != null)
                {
                    foreach (var i in yesterday)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        ExistsReports.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes de entrada/salida", "OK");
                throw;
            }
        }

        /// <summary>
        /// Loads issue reports for today and yesterday.
        /// </summary>
        private async void IssuesExists()
        {
            try
            {
                IssuesReports.Clear();

                var today = await _apiService.GetAsync<List<IssueReport>>(
                    $"api/IssueReports/search?date={DateTime.Today:yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (today != null)
                {
                    foreach (var i in today)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        IssuesReports.Add(i);
                    }
                }

                var yesterday = await _apiService.GetAsync<List<IssueReport>>(
                    $"api/IssueReports/search?date={DateTime.Today.AddDays(-1):yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (yesterday != null)
                {
                    foreach (var i in yesterday)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        IssuesReports.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes de problemas. " + ex.Message, "OK");
                throw;
            }
        }

        /// <summary>
        /// Loads return reports for today and yesterday.
        /// </summary>
        private async void ReturnsExists()
        {
            try
            {
                ReturnReports.Clear();

                var today = await _apiService.GetAsync<List<VehicleReturn>>(
                    $"api/VehicleReturns/search?date={DateTime.Today:yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (today != null)
                {
                    foreach (var i in today)
                    {
                        ReturnReports.Add(i);
                    }
                }

                var yesterday = await _apiService.GetAsync<List<VehicleReturn>>(
                    $"api/VehicleReturns/search?date={DateTime.Today.AddDays(-1):yyyy-MM-dd}&carId={Vehicle.CarId}",
                    TimeSpan.FromSeconds(30));

                if (yesterday != null)
                {
                    foreach (var i in yesterday)
                    {
                        ReturnReports.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes de devoluciones. " + ex.Message, "OK");
                throw;
            }
        }

        /// <summary>
        /// Retrieves photos associated with a report ID.
        /// </summary>
        /// <param name="id">The report ID.</param>
        /// <returns>A list of associated photos.</returns>
        private async Task<List<CheckCars.Models.Photo>> GetPhotos(string id)
        {
            var info = await _apiService.GetAsync<List<CheckCars.Models.Photo>>(
                $"api/Photos/report/{id}",
                TimeSpan.FromSeconds(30));

            return info ?? new List<CheckCars.Models.Photo>();
        }

        #endregion
    }
}
