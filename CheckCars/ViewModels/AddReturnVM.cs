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

        private readonly APIService _apiService = new();
        private SensorManager _sensorManager = new();

        private ObservableCollection<Photo> _photos = new();

        public ObservableCollection<Photo> Photos
        {
            get { return _photos; }
            set
            {
                if (_photos != value)
                {
                    _photos = value;
                    OnPropertyChanged(nameof(Photos));
                }
            }

        }

        private bool _loading = false;
        public bool loading
        {
            get { return _loading; }
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    OnPropertyChanged(nameof(loading));
                }
            }
        }

        private VehicleReturn _VehicleReturn { get; set; } = new VehicleReturn();
        private string[] _CarsInfo;
        public VehicleReturn VehicleReturn
        {
            get { return _VehicleReturn; }
            set
            {
                if (_VehicleReturn != value)
                {
                    _VehicleReturn = value;
                    OnPropertyChanged(nameof(VehicleReturn));
                }
            }
        }
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

        #endregion

        #region Commands

        public ICommand AddReportCommand
        {
            get
            {
                return new Command( async ()=> await AddVehicleReturnAsync() );
            }
            private set { }
        }

        public ICommand DeletePhotoCommand { get; }

        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command( async () => await TakePhotoAsync());
            }
            private set { }
        }

        #endregion

        #region Methods
        private async Task<string[]> GetCarsInfoAsync()
        {
            using (var db = new ReportsDBContextSQLite())
            {
                return (from C in db.Cars
                        orderby C.Plate ascending
                        select $"{C.Plate} {C.Model}"

                            ).ToArray();
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                Photo photo = await _sensorManager.TakePhoto();
                if (photo != null)
                {
                    Photos.Add(photo);
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error interno", "Ok");
                //throw;
            }
        }

        private async Task AddVehicleReturnAsync()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                 "Confirmación",
                 "¿Deseas continuar?",
                 "Sí",
                 "No"
             );
                var IsValid = await PromptPhotosAsync();
                if (IsValid && answer)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        VehicleReturn.Created = DateTime.Now;
                        VehicleReturn.CarPlate = VehicleReturn.CarPlate.Split(' ').First();
                        VehicleReturn.Photos = Photos.Select(p =>
                        {
                            p.PhotoId = Guid.NewGuid().ToString();
                            return p;
                        }).ToList();
                        // Add in the DB
                        db.Returns.Add(VehicleReturn);
                        db.SaveChanges();

                        // Send To The Server
                        await SendDataAsync(VehicleReturn);

                        CloseAsync();
                    }

                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error: " + e.Message, "Ok");

                //        throw;
            }
        }

        private async Task CloseAsync()
        {
            var ThisPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(ThisPage);
        }

        private void DeletePhoto(Photo photo)
        {
            if (photo == null) return;

            try
            {
                if(File.Exists(photo.FilePath))
                {
                    File.Delete(photo.FilePath);
                }
                Photos.Remove(photo);
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se logró borrar la foto", "Ok");
            }
        }

        private async Task LoadUbicationAsync()
        {
            try
            {
                double[] location = await _sensorManager.GetCurrentLocation();

                if (location != null)
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
            catch (Exception)
            {
                _sensorManager.CancelRequest();
            }
        }

        private async Task<bool> PromptPhotosAsync()
        {
            if (Photos.Count > 0)
            {
                return true;
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Advertencia", "No se pueden agregar registros sin fotos", "Ok");
                return false;
            }

        }

        private async Task SendDataAsync(VehicleReturn vehicleReturn)
        {
            try
            {
                loading = true;
                TimeSpan tp;

                const int baseTime = 30;
                const int TimePerPhoto = 20;
                
                if(vehicleReturn.Photos?.Count > 0)
                {
                    int totalTime = baseTime + ( vehicleReturn.Photos.Count * TimePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    var photos = vehicleReturn.Photos.Select(e=> e.FilePath).ToList();
                    await _apiService.PostAsync<VehicleReturn>("api/VehicleReturns/form", vehicleReturn, photos, tp);
                }
                else
                {
                    tp = TimeSpan.FromSeconds(baseTime);

                    await _apiService.PostAsync<VehicleReturn>("api/VehicleReturns/json", vehicleReturn, tp);
                }
            }
            catch (Exception ef)
            {
                Console.WriteLine(ef.Message);

                //throw;
            }
            finally
            { 
                loading = false; 
            }    
        }


        #endregion

    }
}
