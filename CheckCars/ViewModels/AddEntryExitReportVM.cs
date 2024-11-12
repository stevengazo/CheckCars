﻿using CheckCars.Data;
using CheckCars.Models;
using SkiaSharp;
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
        private CheckCars.Utilities.SensorManager SensorManager = new();

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
                return new Command(() => Task.Run(TakePhotos));
            }
            private set { }
        }

        private async Task TakePhotos()
        {
            Photo photo = await SensorManager.TakePhoto();
            if(photo != null)
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
                    SensorManager._isCheckingLocation = true;

                    Report.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {
                        double[] location =await  SensorManager.GetCurrentLocation();
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
                SensorManager.CancelRequest();
            }
        }
        private async Task Close()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }
        // Método para capturar y guardar la foto
  

   


    }
}
