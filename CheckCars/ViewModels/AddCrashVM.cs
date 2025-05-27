using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddCrashVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the AddCrashVM class,
        /// sets up commands, loads cars info, and begins location loading.
        /// </summary>
        public AddCrashVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            CarsInfo = GetCarsInfo();
            Task.Run(() => LoadUbicationAsync());
            newCrashReport.Author = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
        }
        #endregion

        #region Properties

        /// <summary>
        /// API service for network requests.
        /// </summary>
        private readonly APIService _apiService = new APIService();

        /// <summary>
        /// Sensor manager for camera and location functionality.
        /// </summary>
        private CheckCars.Utilities.SensorManager SensorManager = new();

        /// <summary>
        /// Collection of photos attached to the crash report.
        /// </summary>
        private ObservableCollection<Photo> _imgs = new();

        /// <summary>
        /// The crash report being created or edited.
        /// </summary>
        private CrashReport _newCrashReport = new() { DateOfCrash = DateTime.Now, Created = DateTime.Now };

        /// <summary>
        /// Indicates whether data is currently being sent.
        /// </summary>
        private bool _SendingData;
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
        /// List of car info strings for selection.
        /// </summary>
        private string[] _CarsInfo;
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

        /// <summary>
        /// List of photos associated with the crash report.
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

        /// <summary>
        /// The crash report model instance.
        /// </summary>
        public CrashReport newCrashReport
        {
            get { return _newCrashReport; }
            set
            {
                _newCrashReport = value;
                if (_newCrashReport != value)
                {
                    OnPropertyChanged(nameof(newCrashReport));
                }
            }
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command to delete a photo from the list.
        /// </summary>
        public ICommand DeletePhotoCommand { get; }

        /// <summary>
        /// Command to add the crash report.
        /// </summary>
        public ICommand AddReport
        {
            get
            {
                return new Command(async () => await AddReportEntryAsync());
            }
            private set { }
        }

        /// <summary>
        /// Command to take photos asynchronously.
        /// </summary>
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhotosAsync));
            }
            private set { }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Validates and adds the crash report entry to the database and sends data.
        /// </summary>
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

                bool valid = ValidateData();

                if (answer && valid)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        newCrashReport.CarPlate = newCrashReport.CarPlate.Split(' ').First();
                        newCrashReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = Guid.NewGuid().ToString();
                            return photo;
                        }).ToList();

                        db.CrashReports.Add(newCrashReport);
                        db.SaveChanges();
                        await SendingDataAsync(newCrashReport);
                        CloseAsync();
                    }
                }
                else if (answer && !valid)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Valide la información", "ok");
                }
            }
            catch (Exception rf)
            {
                await Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
            }
        }

        /// <summary>
        /// Closes the current page asynchronously.
        /// </summary>
        private async Task CloseAsync()
        {
            try
            {
                var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                Application.Current.MainPage.Navigation.RemovePage(d);
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "ok");
            }
            finally
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        /// <summary>
        /// Deletes a photo both from disk and from the list.
        /// </summary>
        /// <param name="photo">The photo to delete.</param>
        private void DeletePhoto(Photo photo)
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
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the list of car info strings from the database.
        /// </summary>
        /// <returns>Array of car info strings.</returns>
        private string[] GetCarsInfo()
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
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", e.Message, "ok");
                CloseAsync();
                return null;
            }
        }

        /// <summary>
        /// Takes a photo asynchronously using the sensor manager.
        /// </summary>
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
                Application.Current.MainPage.DisplayAlert("Error", e.Message, "ok");
            }
        }

        /// <summary>
        /// Validates required fields in the crash report.
        /// </summary>
        /// <returns>True if valid; otherwise false.</returns>
        private bool ValidateData()
        {
            if (string.IsNullOrEmpty(newCrashReport.CarPlate))
                return false;

            if (string.IsNullOrEmpty(newCrashReport.Location))
                return false;

            if (string.IsNullOrEmpty(newCrashReport.CrashDetails))
                return false;

            if (string.IsNullOrEmpty(newCrashReport.CrashedParts))
                return false;

            return true;
        }

        /// <summary>
        /// Loads the current GPS location asynchronously and updates the crash report.
        /// </summary>
        private async Task LoadUbicationAsync()
        {
            try
            {
                double[] location = await SensorManager.GetCurrentLocation();

                if (location != null)
                {
                    newCrashReport.Latitude = location[0];
                    newCrashReport.Longitude = location[1];
                }
                else
                {
                    newCrashReport.Latitude = 0;
                    newCrashReport.Longitude = 0;
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "ok");
                SensorManager.CancelRequest();
            }
        }

        /// <summary>
        /// Sends the crash report data, including photos if any, to the API.
        /// </summary>
        /// <param name="crashReport">Crash report data to send.</param>
        private async Task SendingDataAsync(CrashReport crashReport)
        {
            try
            {
                SendingData = true;

                const int baseTime = 30;        // Base time in seconds
                const int timePerPhoto = 10;    // Additional seconds per photo

                TimeSpan tp;

                if (crashReport.Photos?.Count > 0)
                {
                    int totalTime = baseTime + (crashReport.Photos.Count * timePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    var photos = crashReport.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<CrashReport>("api/CrashReports/form", crashReport, photos, tp);
                }
                else
                {
                    tp = TimeSpan.FromSeconds(baseTime);
                    await _apiService.PostAsync<CrashReport>("api/CrashReports/json", crashReport, tp);
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", e.Message, "ok");
                throw;
            }
            finally
            {
                SendingData = false;
            }
        }
        #endregion

    }
}
