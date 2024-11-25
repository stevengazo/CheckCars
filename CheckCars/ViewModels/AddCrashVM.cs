using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vehiculosmecsa.Data;
using vehiculosmecsa.Models;

namespace vehiculosmecsa.ViewModels
{
    public class AddCrashVM : INotifyPropertyChangedAbst
    {
        #region General
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private vehiculosmecsa.Utilities.SensorManager SensorManager = new();

        public AddCrashVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
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
        private CrashReport _newCrashReport = new() 
        { 
            DateOfCrash = DateTime.Now,
            Created = DateTime.Now
        };
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
                return new Command(() => AddReportEntry());
            }
            private set { }
        }
        private async Task AddReportEntry()
        {
            try
            {
                bool valid = await ValidateData();

                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas continuar?",
                    "Sí",
                    "No"
                );

              
                if (answer && valid)
                {
                    newCrashReport.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {
                        double[] location = await SensorManager.GetCurrentLocation();
                        newCrashReport.Latitude = location[0];
                        newCrashReport.Longitude = location[1];
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newCrashReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();
                        valid = !valid;
                        db.CrashReports.Add(newCrashReport);
                        db.SaveChanges();
                        Close();
                    }
                }else if (answer && !valid)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Valide la información", "ok");
                }

            }
            catch (Exception rf)
            {
                Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
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

        private async Task<bool> ValidateData()
        {

            if (string.IsNullOrEmpty(newCrashReport.CarPlate))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.Location))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.Details))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newCrashReport.CrashedParts))
            {
                return false;
            }
            return true;

        }


    }
}
