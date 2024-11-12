﻿using CheckCars.Data;
using CheckCars.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {

        #region General
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public AddEntryExitReportVM()
        {
            Report = new();
            Report.Created = DateTime.Now;
        }
        #endregion

        private EntryExitReport _report = new();
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
                return new Command(() => Task.Run(TakePhoto));
            }
            private set { }
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
                    _isCheckingLocation = true;

                    Report.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {
                        double[] location =await GetCurrentLocation();
                        Report.Latitude = location[0];
                        Report.Longitude = location[1]; 
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        Report.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.EntryExitReports.Add(Report);
                        db.SaveChanges();
                        Close();
                    }
                }
            }
            catch (Exception rf)
            {
                Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
                CancelRequest();
            }
        }
        private async Task Close()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }
        // Método para capturar y guardar la foto
        private async Task TakePhoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Captura la foto
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        // Obtiene la ruta local del archivo (temporal)
                        string filePath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

                        // Guarda la foto en el almacenamiento local
                        using (var stream = await photo.OpenReadAsync())
                        {
                            using (var fileStream = File.OpenWrite(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                        }

                        // Crea un objeto Photo con la información de la imagen
                        var newPhoto = new Photo
                        {
                            PhotoId = ImgList.Count + 1, // O cualquier otra lógica para generar PhotoId
                            FileName = photo.FileName,
                            FilePath = filePath,
                            DateTaken = DateTime.Now
                        };

                        // Agrega el objeto Photo a la lista
                        ImgList.Add(newPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones, por ejemplo, mostrar un mensaje de error
                Console.WriteLine($"Error al capturar y guardar la foto: {ex.Message}");
            }
        }

        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;

        public async Task<double[]> GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                _cancelTokenSource = new CancellationTokenSource();

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

               return new double[] { location.Latitude,location.Longitude};


              
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                // Unable to get location
                return null;
            }
            finally
            {
                _isCheckingLocation = false;
             
            }
        }

        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }
    }
}
