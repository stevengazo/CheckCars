﻿using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {
        #region Constructor 
        public AddEntryExitReportVM()
        {
            CarsInfo = GetCarsInfoAsync().Result;
            DeletePhotoCommand = new Command<Photo>(DeletePhotoAsync);
            Task.Run(() => LoadUbicationAsync());
            Report.Author = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
        }
        #endregion
      
        #region Properties
        private readonly APIService _apiService = new APIService();
        private CheckCars.Utilities.SensorManager SensorManager = new();
        private string[] _CarsInfo;
        private EntryExitReport _report = new() { Created = DateTime.Now };
        private ObservableCollection<Photo> _imgs = new();
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
        public ObservableCollection<Photo> ImgList
        {
            get { return _imgs; }
            set
            {
                if (_imgs != value)
                {
                    _imgs = value;
                    OnPropertyChanged(nameof(ImgList));  // Notificamos el cambio de lista
                }
            }
        }

        #endregion

        #region Commands
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhotosAsync));
            }
            private set { }
        }
        public ICommand AddReport
        {
            get
            {
                return new Command(() => AddReportEntryAsync());
            }
            private set { }
        }
        public ICommand DeletePhotoCommand { get; }
        #endregion

        #region Methods

        private async Task SendDataAsync(EntryExitReport obj)
        {
            try
            {
                SendingData = true;
                TimeSpan tp;

                // Tiempo base: 30 segundos para datos sin fotos
                const int baseTime = 30;

                // Incremento: 10 segundos por cada foto
                const int timePerPhoto = 15;

                if (obj.Photos?.Count > 0)
                {
                    // Calcula el tiempo dinámicamente
                    int totalTime = baseTime + (obj.Photos.Count * timePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    // Envía los datos con las fotos
                    var photos = obj.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/form", obj, photos, tp);
                }
                else
                {
                    // Tiempo para envío sin fotos
                    tp = TimeSpan.FromSeconds(baseTime);

                    // Envía los datos sin fotos
                    await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/json", obj, tp);
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al enviar los datos: " + e.Message, "OK");
                Console.Write(e.Message);
                throw;
            }
            finally
            {
                SendingData = false;
            }
        }
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
                Application.Current.MainPage.DisplayAlert("Error", "Error al tomar la foto: " + e.Message, "OK");
            }
        }
        private void DeletePhotoAsync(Photo photo)
        {
            if (photo == null) return; // Evitar argumentos nulos

            try
            {
                if (File.Exists(photo.FilePath))
                {
                    File.Delete(photo.FilePath);
                }
                ImgList.Remove(photo); // Eliminar de la lista
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, loguearla)
                Application.Current.MainPage.DisplayAlert("Error", "Error al eliminar la foto: " + ex.Message, "OK");
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }
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
                        //Report.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;
                        Report.CarPlate = Report.CarPlate.Split(' ').First();

                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        Report.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = Guid.NewGuid().ToString();  // Reset para que se genere automáticamente
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
                    await Application.Current.MainPage.DisplayAlert("Error", "Añada Imágenes del Vehículo.", "Ok");
                }
                else {
                    await Application.Current.MainPage.DisplayAlert("Error", "Verifique los Datos, faltan realizar algunas verificaciones.", "Ok");
                }
            }
            catch (Exception rf)
            {
                await Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
                CloseAsync();
            }
        }
        private async Task CloseAsync()
        {
            try
            {
                var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                Application.Current.MainPage.Navigation.RemovePage(d);

            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al cerrar la página: " + e.Message, "OK");
            }
        }
        // Método para capturar y guardar la foto
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
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los vehículos: " + d.Message, "OK");
                CloseAsync();
                return null;
            }
        }
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
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar la ubicación: " + r.Message, "OK");
                SensorManager.CancelRequest();
            }
        }
        #endregion
    }
}
