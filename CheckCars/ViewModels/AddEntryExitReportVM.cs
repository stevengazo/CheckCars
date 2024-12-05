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
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace CheckCars.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {
        private CheckCars.Utilities.SensorManager SensorManager = new();

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

        private EntryExitReport _report = new()
        {
            Created = DateTime.Now
        };
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
        public AddEntryExitReportVM()
        {
            CarsInfo = GetCarsInfo().Result;
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
        }
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhotos));
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
        public ICommand DeletePhotoCommand { get; }
        private async Task TakePhotos()
        {
            Photo photo = await SensorManager.TakePhoto();
            if (photo != null)
            {
                ImgList.Add(photo);
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
                // Manejar la excepción (por ejemplo, loguearla)
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
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

                var valid = await ValidateData();

                if (answer && valid)
                {
                    SensorManager._isCheckingLocation = true;

                    using (var db = new ReportsDBContextSQLite())
                    {
                        
                        double[] location =await  SensorManager.GetCurrentLocation();

                        if(location != null)
                        {
                            Report.Latitude = location[0];
                            Report.Longitude = location[1];
                        }
                        else
                        {
                            Report.Latitude = 0;
                            Report.Longitude = 0;
                        }
                        Report.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;
                    
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
                else
                {
                  await Application.Current.MainPage.DisplayAlert("Error", "Verifique los datos", "ok");
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
        // Método para capturar y guardar la foto
        private async Task<bool> ValidateData()
        {
            if( Report.mileage == 0)
            {
                return false;
            }
            if(string.IsNullOrEmpty(Report.Justify) )
            {
                return false;
            }
            if( string.IsNullOrWhiteSpace(Report.TiresState))
            {
                return false;
            }
            if( string.IsNullOrEmpty(Report.PaintState) )
            {
                return false;
            }
            if( string.IsNullOrEmpty(Report.MecanicState) )
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
        private async Task<string[]> GetCarsInfo()
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
