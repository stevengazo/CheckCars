using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Data;
using CheckCars.Models;

namespace CheckCars.ViewModels
{
    public class AddCrashVM : INotifyPropertyChangedAbst
    {
        private CheckCars.Utilities.SensorManager SensorManager = new();
        public AddCrashVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            CarsInfo = GetCarsInfo();
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
        // Lista para almacenar las fotos capturadas
        private ObservableCollection<Photo> _imgs = new();
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
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhotos));
            }
            private set { }
        }
        private async Task TakePhotos()
        {
            Photo photo = await SensorManager.TakePhoto();
            if (photo != null)
            {
                ImgList.Add(photo);
            }
        }
        private CrashReport _newCrashReport = new() { DateOfCrash = DateTime.Now,
        Created = DateTime.Now};
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
        public ICommand DeletePhotoCommand { get; }
        public ICommand AddReport
        {
            get
            {
                return new Command( async () =>await AddReportEntry());
            }
            private set { }
        }

        private async Task AddReportEntry()
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
                        newCrashReport.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;
                        
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newCrashReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.CrashReports.Add(newCrashReport);
                        db.SaveChanges();
                        Close();
                    }
                }else if (answer && !valid)
                {
                   await Application.Current.MainPage.DisplayAlert("Error", "Valide la información", "ok");
                }
            }
            catch (Exception rf)
            {
               await Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
                SensorManager.CancelRequest();
            }
        }
        private async Task Close()
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
        private string[] GetCarsInfo()
        {
            using (var db = new ReportsDBContextSQLite())
            {
                return (from C in db.Cars
                        select $"{C.Brand}-{C.Model}-{C.Plate}"
                            ).ToArray();
            }
        }


    }
}
