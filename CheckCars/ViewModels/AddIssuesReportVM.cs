using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddIssuesReportVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the AddIssuesReportVM class.
        /// Sets up commands, loads cars info and current location, and sets the author.
        /// </summary>
        public AddIssuesReportVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            CarsInfo = GetCarsInfoAsync().Result;
            Task.Run(() => LoadUbicationAsync());
            newIssueReport.Author = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
        }
        #endregion

        #region Properties
        private readonly APIService _apiService = new();
        private CheckCars.Utilities.SensorManager SensorManager = new();
        private ObservableCollection<Photo> _imgs = new();

        private IssueReport _newIssueReport = new()
        {
            Created = DateTime.Now,
        };

        /// <summary>
        /// Indicates whether data is being sent.
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
        /// Holds the cars information for display.
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
        /// The new issue report being created.
        /// </summary>
        public IssueReport newIssueReport
        {
            get { return _newIssueReport; }
            set
            {
                _newIssueReport = value;
                if (_newIssueReport != value)
                {
                    OnPropertyChanged(nameof(newIssueReport));
                }
            }
        }

        /// <summary>
        /// Collection of photos related to the report.
        /// </summary>
        public ObservableCollection<Photo> ImgList
        {
            get { return _imgs; }
            set
            {
                if (_imgs != value)
                {
                    _imgs = value;
                    OnPropertyChanged(nameof(ImgList));  // Notify changes to the list
                }
            }
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command to add the issue report asynchronously.
        /// </summary>
        public ICommand AddReport
        {
            get
            {
                return new Command(() => AddReportEntryAsync());
            }
            private set { }
        }

        /// <summary>
        /// Command to delete a photo from the collection and disk.
        /// </summary>
        public ICommand DeletePhotoCommand { get; }

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
        /// Asynchronously takes a photo using the sensor manager and adds it to ImgList.
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
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo tomar la foto", "ok");
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// Validates and adds the issue report to the database, sends data, and closes the page.
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
                bool valid = await ValidateDataAsync();

                if (answer && valid)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        // Set car plate to only the first word
                        newIssueReport.CarPlate = newIssueReport.CarPlate.Split(' ').First();

                        // Generate new PhotoId for each photo
                        newIssueReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = Guid.NewGuid().ToString();
                            return photo;
                        }).ToList();

                        db.IssueReports.Add(newIssueReport);
                        db.SaveChanges();

                        await SendDataAsync(newIssueReport);
                        CloseAsync();
                    }
                }
                else if (answer && !valid)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Verifique la información", "ok");
                }
            }
            catch (Exception rf)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar el reporte. Error: " + rf.Message, "ok");
                Console.WriteLine(rf.ToString());
                CloseAsync();
            }
        }

        /// <summary>
        /// Closes the current page by removing it from the navigation stack.
        /// </summary>
        private async Task CloseAsync()
        {
            try
            {
                var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                Application.Current.MainPage.Navigation.RemovePage(d);
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo cerrar la página", "ok");
                throw e;
            }
        }

        /// <summary>
        /// Deletes the specified photo from disk and removes it from ImgList.
        /// </summary>
        /// <param name="photo">The photo to delete.</param>
        private void DeletePhoto(Photo photo)
        {
            if (photo == null) return; // Prevent null arguments

            try
            {
                if (File.Exists(photo.FilePath))
                {
                    File.Delete(photo.FilePath);
                }
                ImgList.Remove(photo); // Remove from the list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
                Application.Current.MainPage.DisplayActionSheet("Error", "ok", null, "No se pudo eliminar la foto");
            }
        }

        /// <summary>
        /// Validates the issue report data to ensure all required fields are completed.
        /// </summary>
        /// <returns>True if data is valid; otherwise false.</returns>
        private async Task<bool> ValidateDataAsync()
        {
            if (string.IsNullOrEmpty(newIssueReport.CarPlate))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newIssueReport.Details))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newIssueReport.Type))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves all cars information from the database.
        /// </summary>
        /// <returns>An array of strings containing car plates and models.</returns>
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
                Application.Current.MainPage.DisplayActionSheet("Error", "ok", null, "No se pudo cargar la información de los autos");
                CloseAsync();
                return null;
            }
        }

        /// <summary>
        /// Loads current GPS location asynchronously and updates the issue report's latitude and longitude.
        /// </summary>
        private async Task LoadUbicationAsync()
        {
            try
            {
                double[] location = await SensorManager.GetCurrentLocation();

                if (location != null)
                {
                    newIssueReport.Latitude = location[0];
                    newIssueReport.Longitude = location[1];
                }
                else
                {
                    newIssueReport.Latitude = 0;
                    newIssueReport.Longitude = 0;
                }
            }
            catch (Exception d)
            {
                SensorManager.CancelRequest();
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener la ubicación", "ok");
            }
        }

        /// <summary>
        /// Sends the issue report data and photos asynchronously via API.
        /// </summary>
        /// <param name="report">The issue report to send.</param>
        private async Task SendDataAsync(IssueReport report)
        {
            try
            {
                SendingData = true;
                TimeSpan tp;

                // Base time: 30 seconds for data without photos
                const int baseTime = 30;

                // Increment: 10 seconds per photo
                const int timePerPhoto = 10;

                if (report.Photos?.Count > 0)
                {
                    int totalTime = baseTime + (report.Photos.Count * timePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    var photos = report.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<IssueReport>("api/IssueReports/form", report, photos, tp);
                }
                else
                {
                    tp = TimeSpan.FromSeconds(baseTime);
                    await _apiService.PostAsync<IssueReport>("api/IssueReports/json", report, tp);
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo enviar el reporte. Error: " + e.Message, "ok");
            }
            finally
            {
                SendingData = false;
            }
        }

        #endregion
    }
}
