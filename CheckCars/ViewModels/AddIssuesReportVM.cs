using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddIssuesReportVM : INotifyPropertyChangedAbst
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
        public AddIssuesReportVM()
        {
            
        }
        private IssueReport _newIssueReport = new();
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
        private ObservableCollection<Photo> _imgs = new();
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
                    newIssueReport.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {
                        double[] location = await SensorManager.GetCurrentLocation();
                        newIssueReport.Latitude = location[0];
                        newIssueReport.Longitude = location[1];
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newIssueReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.IssueReports.Add(newIssueReport);
                        db.SaveChanges();
                        Close();
                    }
                }
            }
            catch (Exception rf)
            {
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
