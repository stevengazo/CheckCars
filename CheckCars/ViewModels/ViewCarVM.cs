using CheckCar.Data;
using CheckCar.Models;
using CheckCar.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCar.Views;
using CheckCar.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CheckCar.ViewModels
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
        public ICommand AddBooking { get; }

        /// <summary>
        /// Prompts the user for confirmation before deleting a car.
        /// </summary>
        public ICommand DeleteCar { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the ViewModel, loads vehicle data and associated reports.
        /// </summary>
        public ViewCarVM()
        {
            try
            {
                AddBooking = new Command(async () =>
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new AddBooking());
                });
                DeleteCar = new Command(async () =>
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

            

                _ = InitializedAsync();

            }
            catch (Exception f)
            {
                MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: " + f.Message);
            }
        }

        private async Task InitializedAsync()
        {
            try
            {

                var id = StaticData.CarId;
                using (var db = new ReportsDBContextSQLite())
                {
                    Vehicle = await db.Cars?.FirstOrDefaultAsync(e => e.CarId == id);
                }
                if(Vehicle == null)
                {
                    await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: Vehículo no encontrado.");
                    return;
                }

                await RequestExistsAsync();
                await IssuesExistsAsync();
                await ReturnsExistsAsync();
            }
            catch (Exception v)
            {
              await  MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: " + v.Message);
            }
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
        private async Task RequestExistsAsync()
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
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error al cargar los reportes de entrada/salida. Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads issue reports for today and yesterday.
        /// </summary>
        private async Task IssuesExistsAsync()
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
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads return reports for today and yesterday.
        /// </summary>
        private async Task ReturnsExistsAsync()
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
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves photos associated with a report ID.
        /// </summary>
        /// <param name="id">The report ID.</param>
        /// <returns>A list of associated photos.</returns>
        private async Task<List<CheckCar.Models.Photo>> GetPhotos(string id)
        {
            try
            {
                var info = await _apiService.GetAsync<List<CheckCar.Models.Photo>>(
              $"api/Photos/report/{id}",
              TimeSpan.FromSeconds(30));

                return info ?? new List<CheckCar.Models.Photo>();
            }
            catch (Exception f)
            {
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error al cargar las fotos: " + f.Message);
                return null;
            }
        }

        #endregion
    }
}
