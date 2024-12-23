﻿using CheckCars.Data;
using CheckCars.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddCrashVM : INotifyPropertyChangedAbst
    {

        public AddCrashVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            CarsInfo = GetCarsInfo();
            Task.Run(() => LoadUbicationAsync());
        }

        #region Properties

        private CheckCars.Utilities.SensorManager SensorManager = new();
        private ObservableCollection<Photo> _imgs = new();
        private CrashReport _newCrashReport = new() { DateOfCrash = DateTime.Now, Created = DateTime.Now };

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
        // Propiedad para la lista de fotos
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

        public ICommand DeletePhotoCommand { get; }
        public ICommand AddReport
        {
            get
            {
                return new Command(async () => await AddReportEntryAsync());
            }
            private set { }
        }
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
                    newCrashReport.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {

                        newCrashReport.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;

                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newCrashReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.CrashReports.Add(newCrashReport);
                        db.SaveChanges();
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
        private async Task CloseAsync()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
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
                // Manejar la excepción (por ejemplo, loguearla)
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }
        private string[] GetCarsInfo()
        {
            using (var db = new ReportsDBContextSQLite())
            {
                return (from C in db.Cars
                        select $"{C.Brand}-{C.Model}-{C.Plate}"
                            ).ToArray();
            }
        }
        private async Task TakePhotosAsync()
        {
            Photo photo = await SensorManager.TakePhoto();
            if (photo != null)
            {
                ImgList.Add(photo);
            }
        }
        private bool ValidateData()
        {

            if (string.IsNullOrEmpty(newCrashReport.CarPlate))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.Location))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.CrashDetails))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.CrashedParts))
            {
                return false;
            }
            return true;

        }
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
            catch (Exception)
            {
                SensorManager.CancelRequest();
            }
        }

        #endregion

    }
}
