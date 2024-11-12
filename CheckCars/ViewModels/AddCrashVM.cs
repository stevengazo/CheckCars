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
        #region General
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        private CheckCars.Utilities.SensorManager SensorManager = new();

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
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas continuar?",
                    "Sí",
                    "No"
                );

                if (answer)
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

                        db.CrashReports.Add(newCrashReport);
                        db.SaveChanges();
                        Close();
                    }
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


    }
}
