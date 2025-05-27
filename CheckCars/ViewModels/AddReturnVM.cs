using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace CheckCars.ViewModels
{
    public class AddReturnVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        public AddReturnVM()
        {
            CarsInfo = GetCarsInfoAsync().Result;
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            Task.Run(async () => LoadUbicationAsync());
            VehicleReturn.Author = Preferences.Get(nameof(UserProfile.UserName), "Nombre Usuario");

        }
        #endregion

        #region Properties

        /// <summary>
        /// API service used for network or data operations.
        /// </summary>
        private readonly APIService _apiService = new();

        /// <summary>
        /// Manager for sensor-related functionalities.
        /// </summary>
        private SensorManager _sensorManager = new();

        /// <summary>
        /// Backing field for the collection of photos.
        /// </summary>
        private ObservableCollection<Photo> _imgs = new();

        /// <summary>
        /// Collection of photos displayed or managed in the UI.
        /// </summary>
        public ObservableCollection<Photo> ImgList
        {
            get => _imgs;
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
        /// Backing field for the loading state.
        /// </summary>
        private bool _loading = false;

        /// <summary>
        /// Indicates whether the ViewModel is currently performing a loading operation.
        /// </summary>
        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    OnPropertyChanged(nameof(Loading));
                }
            }
        }

        /// <summary>
        /// Backing field for vehicle return data.
        /// </summary>
        private VehicleReturn _vehicleReturn = new();

        /// <summary>
        /// Vehicle return data, likely retrieved from the API or processed in the app.
        /// </summary>
        public VehicleReturn VehicleReturn
        {
            get => _vehicleReturn;
            set
            {
                if (_vehicleReturn != value)
                {
                    _vehicleReturn = value;
                    OnPropertyChanged(nameof(VehicleReturn));
                }
            }
        }

        /// <summary>
        /// Backing field for an array of car information strings.
        /// </summary>
        private string[] _carsInfo;

        /// <summary>
        /// Array of string information about cars, such as plates.
        /// </summary>
        public string[] CarsInfo
        {
            get => _carsInfo;
            set
            {
                if (_carsInfo != value)
                {
                    _carsInfo = value;
                    OnPropertyChanged(nameof(CarsInfo));
                }
            }
        }

        #endregion


        #region Commands

        public ICommand AddReportCommand
        {
            get
            {
                return new Command(async () => await AddVehicleReturnAsync());
            }
            private set { }
        }

        public ICommand DeletePhotoCommand { get; }

        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(async () => await TakePhotoAsync());
            }
            private set { }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves an array of car information strings ordered by plate.
        /// </summary>
        /// <returns>Array of strings with car plate and model, or null if error.</returns>
        private async Task<string[]> GetCarsInfoAsync()
        {
            try
            {
                using var db = new ReportsDBContextSQLite();
                return (from C in db.Cars
                        orderby C.Plate ascending
                        select $"{C.Plate} {C.Model}").ToArray();
            }
            catch (Exception d)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error: " + d.Message, "Ok");
                return null;
            }
        }

        /// <summary>
        /// Takes a photo using the sensor manager and adds it to the image list.
        /// </summary>
        private async Task TakePhotoAsync()
        {
            try
            {
                Photo photo = await _sensorManager.TakePhoto();
                if (photo != null)
                {
                    ImgList.Add(photo);
                }
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Internal error while taking photo", "Ok");
            }
        }

        /// <summary>
        /// Adds a vehicle return record after user confirmation and photo validation.
        /// Saves to local DB and sends data to the server.
        /// </summary>
        private async Task AddVehicleReturnAsync()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas agregar una entrega?",
                    "Sí",
                    "No");

                var isValid = await PromptPhotosAsync();

                if (isValid && answer)
                {
                    using var db = new ReportsDBContextSQLite();

                    VehicleReturn.Created = DateTime.Now;
                    VehicleReturn.CarPlate = VehicleReturn.CarPlate.Split(' ').First();
                    VehicleReturn.Photos = ImgList.Select(p =>
                    {
                        p.PhotoId = Guid.NewGuid().ToString();
                        return p;
                    }).ToList();

                    // Add in the DB
                    db.Returns.Add(VehicleReturn);
                    db.SaveChanges();

                    // Send to the server
                    await SendDataAsync(VehicleReturn);

                    await CloseAsync();
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Internal error adding vehicle: " + e.Message, "Ok");
            }
        }

        /// <summary>
        /// Closes the current page from the navigation stack.
        /// </summary>
        private async Task CloseAsync()
        {
            try
            {
                var currentPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                if (currentPage != null)
                {
                    Application.Current.MainPage.Navigation.RemovePage(currentPage);
                }
            }
            catch (Exception d)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to close page: " + d.Message, "Ok");
                throw;
            }
        }

        /// <summary>
        /// Deletes the photo file from disk and removes it from the image list.
        /// </summary>
        /// <param name="photo">Photo to delete.</param>
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
            catch (Exception)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Failed to delete photo", "Ok");
            }
        }

        /// <summary>
        /// Loads the current geographic location and updates the VehicleReturn coordinates.
        /// </summary>
        private async Task LoadUbicationAsync()
        {
            try
            {
                double[] location = await _sensorManager.GetCurrentLocation();

                if (location != null && location.Length >= 2)
                {
                    VehicleReturn.Latitude = location[0];
                    VehicleReturn.Longitude = location[1];
                }
                else
                {
                    VehicleReturn.Latitude = 0;
                    VehicleReturn.Longitude = 0;
                }
            }
            catch (Exception c)
            {
                _sensorManager.CancelRequest();
                Console.WriteLine("Error obtaining location: " + c.Message);
            }
        }

        /// <summary>
        /// Checks if there are photos in the list and prompts the user if none exist.
        /// </summary>
        /// <returns>True if photos exist; otherwise false.</returns>
        private async Task<bool> PromptPhotosAsync()
        {
            try
            {
                if (ImgList.Count > 0)
                {
                    return true;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Warning", "Cannot add records without photos", "Ok");
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the vehicle return data along with photos to the server.
        /// Shows loading state during the operation.
        /// </summary>
        /// <param name="vehicleReturn">The vehicle return data to send.</param>
        private async Task SendDataAsync(VehicleReturn vehicleReturn)
        {
            try
            {
                Loading = true;

                const int baseTime = 30;
                const int timePerPhoto = 20;
                TimeSpan timeout;

                if (vehicleReturn.Photos?.Count > 0)
                {
                    int totalTime = baseTime + (vehicleReturn.Photos.Count * timePerPhoto);
                    timeout = TimeSpan.FromSeconds(totalTime);

                    var photos = vehicleReturn.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<VehicleReturn>("api/VehicleReturns/form", vehicleReturn, photos, timeout);
                }
                else
                {
                    timeout = TimeSpan.FromSeconds(baseTime);
                    await _apiService.PostAsync<VehicleReturn>("api/VehicleReturns/json", vehicleReturn, timeout);
                }
            }
            catch (Exception ef)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to send report: " + ef.Message, "Ok");
                Console.WriteLine(ef.Message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion


    }
}
