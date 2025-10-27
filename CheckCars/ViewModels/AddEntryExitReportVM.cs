using ReviCar.Data;
using ReviCar.Models;
using ReviCar.Services;
using ReviCar.Utilities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReviCar.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {
        #region Constructor 
        /// <summary>
        /// Initializes a new instance of the AddEntryExitReportVM class.
        /// Loads cars info, initializes commands and location.
        /// </summary>
        public AddEntryExitReportVM()
        {
            Report.Author = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");

            // Comandos async correctamente
            TakePhotoCommand = new Command(async () => await TakePhotosAsync());
            AddReport = new Command(async () => await AddReportEntryAsync());
            DeletePhotoCommand = new Command<Photo>(DeletePhotoAsync);

            // Inicialización asincrónica sin bloquear UI
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Cargar autos de forma async sin bloquear
                CarsInfo = await GetCarsInfoAsync();

                // Cargar ubicación en hilo principal si es necesario
                await LoadUbicationAsync();
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowLongToast("Error al inicializar: " + ex.Message);
            }
        }

        #endregion

        #region Properties
        private readonly APIService _apiService = new APIService();

        private ReviCar.Utilities.SensorManager SensorManager = new();

        private string[] _CarsInfo;

        private EntryExitReport _report = new() { Created = DateTime.Now };

        private ObservableCollection<Photo> _imgs = new();

        /// <summary>
        /// Gets or sets the list of cars information for UI display.
        /// </summary>
        public string[] CarsInfo
        {
            get { return _CarsInfo; }
            set
            {
                if (_CarsInfo != value)
                {
                    _CarsInfo = value;
                    OnPropertyChanged(nameof(CarsInfo));
                }
            }
        }

        private bool _SendingData;

        /// <summary>
        /// Gets or sets a value indicating whether data is currently being sent.
        /// </summary>
        public bool SendingData
        {
            get { return _SendingData; }
            set
            {
                if (_SendingData != value)
                {
                    _SendingData = value;
                    OnPropertyChanged(nameof(SendingData));
                }
            }
        }

        /// <summary>
        /// Gets or sets the entry-exit report object.
        /// </summary>
        public EntryExitReport Report
        {
            get { return _report; }
            set
            {
                if (_report != value)
                {
                    _report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }

        /// <summary>
        /// Gets or sets the observable collection of photos related to the report.
        /// </summary>
        public ObservableCollection<Photo> ImgList
        {
            get { return _imgs; }
            set
            {
                if (_imgs != value)
                {
                    _imgs = value;
                    OnPropertyChanged(nameof(ImgList));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to take a photo asynchronously.
        /// </summary>
        public ICommand TakePhotoCommand { get; }

        /// <summary>
        /// Command to add a new entry-exit report asynchronously.
        /// </summary>
        public ICommand AddReport { get; }

        /// <summary>
        /// Command to delete a photo from the collection and storage.
        /// </summary>
        public ICommand DeletePhotoCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the entry-exit report data and associated photos asynchronously.
        /// </summary>
        /// <param name="obj">The EntryExitReport object to send.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SendDataAsync(EntryExitReport obj)
        {
            try
            {
                SendingData = true;
                TimeSpan tp;

                const int baseTime = 30;
                const int timePerPhoto = 15;

                if (obj.Photos?.Count > 0)
                {
                    int totalTime = baseTime + (obj.Photos.Count * timePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    var photos = obj.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/form", obj, photos, tp);
                }
                else
                {
                    tp = TimeSpan.FromSeconds(baseTime);
                    await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/json", obj, tp);
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al enviar los datos: " + e.Message);
            }
            finally
            {
                SendingData = false;
            }
        }

        /// <summary>
        /// Takes a photo asynchronously using the sensor manager.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TakePhotosAsync()
        {
            try
            {
                Photo photo = await SensorManager.TakePhoto();
                if (photo != null)
                {
                    ImgList.Add(photo);
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("No se pudo tomar la foto. Error: " +e.Message);
            }
        }

        /// <summary>
        /// Deletes the specified photo from disk and collection.
        /// </summary>
        /// <param name="photo">The photo to delete.</param>
        private void DeletePhotoAsync(Photo photo)
        {
            if (photo == null) return;

            try
            {
                if (File.Exists(photo.FilePath))
                {
                    File.Delete(photo.FilePath);
                }
                ImgList.Remove(photo);
            }
            catch (Exception ex)
            {
                 MessageUtilities.ShowLongToast("Error al eliminar la foto. Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Validates and adds the entry-exit report, saves to database, and sends data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task AddReportEntryAsync()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas continuar?",
                    "Sí",
                    "No"
                );

                var valid = await ValidateDataAsync();
                var validPhotos = ImgList.Count > 0;

                if (answer && valid && validPhotos)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        Report.Created = DateTime.Now;
                        Report.CarPlate = Report.CarPlate.Split(' ').First();

                        Report.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = Guid.NewGuid().ToString();
                            return photo;
                        }).ToList();

                        db.EntryExitReports.Add(Report);
                        db.SaveChanges();
                        await SendDataAsync(Report);
                        CloseAsync();
                    }
                }
                else if (!validPhotos)
                {
                    await MessageUtilities.ShowLongToast("Es necesario añadir al menos una imagen del vehículo.");
                }
                else
                {
                    await MessageUtilities.ShowLongToast("Verifique los Datos, faltan realizar algunas verificaciones.");
                }
            }
            catch (Exception rf)
            {
                await MessageUtilities.ShowLongToast("No se pudo agregar el reporte. Error: " + rf.Message);
                CloseAsync();
            }
        }

        /// <summary>
        /// Closes the current page by removing it from the navigation stack.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task CloseAsync()
        {
            try
            {
                var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                Application.Current.MainPage.Navigation.RemovePage(d);
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al cerrar la página: " + e.Message);
            }
        }

        /// <summary>
        /// Validates the report data to ensure all required fields are completed.
        /// </summary>
        /// <returns>True if data is valid; otherwise false.</returns>
        private async Task<bool> ValidateDataAsync()
        {
            if (Report.mileage == 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.Notes))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(Report.TiresState))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.PaintState))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.MecanicState))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.OilLevel))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.InteriorsState))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.CarPlate))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves all cars information from the database.
        /// </summary>
        /// <returns>An array of car plate and model strings.</returns>
        private async Task<string[]> GetCarsInfoAsync()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    return (from C in db.Cars
                            orderby C.Plate ascending
                            select $"{C.Plate} {C.Model}"
                           ).ToArray();
                }
            }
            catch (Exception d)
            {
                await MessageUtilities.ShowLongToast("No se pudieron cargar los vehículos. Por favor, reinicie la aplicación. Error: " + d.Message);
                CloseAsync();
                return null;
            }
        }

        /// <summary>
        /// Loads the current location asynchronously and updates report latitude and longitude.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LoadUbicationAsync()
        {
            try
            {
                SensorManager._isCheckingLocation = true;
                double[] location = await SensorManager.GetCurrentLocation();

                if (location != null)
                {
                    Report.Latitude = location[0];
                    Report.Longitude = location[1];
                }
                else
                {
                    Report.Latitude = 0;
                    Report.Longitude = 0;
                }
            }
            catch (Exception r)
            {
                await MessageUtilities.ShowLongToast("No se pudo obtener la ubicación. Error: " + r.Message);
                SensorManager.CancelRequest();
            }
        }
        #endregion
    }
}
