﻿using CheckCars.Data;
using CheckCars.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {

        public AddEntryExitReportVM()
        {
            CarsInfo = GetCarsInfoAsync().Result;
            DeletePhotoCommand = new Command<Photo>(DeletePhotoAsync);
            Task.Run(() => LoadUbicationAsync());
        }

        #region Properties
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
        private async Task TakePhotosAsync()
        {
            Photo photo = await SensorManager.TakePhoto();
            if (photo != null)
            {
                ImgList.Add(photo);
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

                if (answer && valid)
                {


                    using (var db = new ReportsDBContextSQLite())
                    {


                        Report.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;

                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        Report.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.EntryExitReports.Add(Report);
                        db.SaveChanges();
                        CloseAsync();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Verifique los datos", "ok");
                }
            }
            catch (Exception rf)
            {
                await Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");

            }
        }
        private async Task CloseAsync()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }
        // Método para capturar y guardar la foto
        private async Task<bool> ValidateDataAsync()
        {
            if (Report.mileage == 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(Report.Justify))
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
            using (var db = new ReportsDBContextSQLite())
            {
                return (from C in db.Cars
                        select $"{C.Brand}_{C.Model}_{C.Plate}"
                            ).ToArray();
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
            catch (Exception)
            {
                SensorManager.CancelRequest();

            }
        }
        #endregion
    }
}
