﻿using CheckCars.Data;
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
        public ICommand AddReport
        {
            get
            {
                return new Command(() => AddReportEntryAsync());
            }
            private set { }
        }
        public ICommand DeletePhotoCommand { get; }
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
                        // newIssueReport.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;
                        newIssueReport.CarPlate = newIssueReport.CarPlate.Split(' ').First();
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
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
        private void DeletePhoto(Photo photo)
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
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
                Application.Current.MainPage.DisplayActionSheet("Error", "ok", null, "No se pudo eliminar la foto");
            }
        }
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
        private async Task SendDataAsync(IssueReport report)
        {
            try
            {
                SendingData = true;
                TimeSpan tp;

                // Tiempo base: 30 segundos para datos sin fotos
                const int baseTime = 30;

                // Incremento: 10 segundos por cada foto
                const int timePerPhoto = 10;

                if (report.Photos?.Count > 0)
                {
                    // Calcula el tiempo dinámicamente
                    int totalTime = baseTime + (report.Photos.Count * timePerPhoto);
                    tp = TimeSpan.FromSeconds(totalTime);

                    // Envía los datos con las fotos
                    var photos = report.Photos.Select(e => e.FilePath).ToList();
                    await _apiService.PostAsync<IssueReport>("api/IssueReports/form", report, photos, tp);
                }
                else
                {
                    // Tiempo para envío sin fotos
                    tp = TimeSpan.FromSeconds(baseTime);

                    // Envía los datos sin fotos
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
