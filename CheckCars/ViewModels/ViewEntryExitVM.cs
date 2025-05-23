﻿using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class ViewEntryExitVM : INotifyPropertyChangedAbst
    {

        #region Properties  

        private readonly APIService _apiService;
        private Thread reportThread = new Thread(() => { });

        private bool _SendingDataCheck;
        public bool SendingDataCheck
        {
            get { return _SendingDataCheck; }
            set
            {
                if (_SendingDataCheck != value)
                {
                    _SendingDataCheck = value;
                    OnPropertyChanged(nameof(SendingDataCheck));
                }
            }
        }

        private bool _SendingData = false;
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
        private double _FuelLevel;
        public double FuelLevel
        {
            get { return _FuelLevel; }
            set
            {
                if (_FuelLevel != value)
                {
                    _FuelLevel = value;
                    OnPropertyChanged(nameof(FuelLevel));
                }
            }
        }
        private EntryExitReport _Report = new();
        public EntryExitReport Report
        {
            get { return _Report; }
            set
            {
                if (_Report != value)
                {
                    _Report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }

        #endregion

        #region Command
        public ICommand DownloadReportCommand { get; }
        public ICommand ISendReport { get; }
        public ICommand IDeleteReport { get; }
        public ICommand IShareImage { get; }
        public ICommand ISendServerReport { get; }
        #endregion

        #region Constructor
        public ViewEntryExitVM()
        {
            _apiService = new();
            var Id = Data.StaticData.ReportId;

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.EntryExitReports.Include(E => E.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));
            }

            if (Report != null)
            {
                FuelLevel = Report.FuelLevel / 100;
            }

            DownloadReportCommand = new Command(() => DownloadReport());
            IDeleteReport = new Command(async () => await DeleteReport());
            ISendReport = new Command(async () => await SendPDFReport());
            ISendServerReport = new Command(async () => await SendServer());
            IShareImage = new Command<string>(SharePhotoAsync);

        }
        #endregion

        #region Methods
        /// <summary>
        /// This Method get the data and send to the server 
        /// </summary>
        /// <returns></returns>
        public async Task SendServer()
        {
            try
            {
                SendingDataCheck = true;

                if (!Report.isUploaded && !SendingData)
                {
                    TimeSpan tp = new TimeSpan();
                    tp = TimeSpan.FromSeconds(100);
                    var result = false;
                    if (Report.Photos.Count > 0)
                    {
                        var photos = Report.Photos.Select(e => e.FilePath).ToList();
                        result = await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/form", Report, photos, tp);
                    }
                    else
                    {
                        result = await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports/json", Report, tp);
                    }
                    if (result)
                    {
                        UpdateReport(true);
                        Application.Current.MainPage.DisplayAlert("Información", "Datos enviados al servidor", "Ok");
                    }
                    else
                    {
                        Application.Current.MainPage.DisplayAlert("Información", "Error al enviar los datos", "Ok");
                    }
                }

                else if (Report.isUploaded && !SendingData)
                {
                    Application.Current.MainPage.DisplayAlert("Información", "Este reporte ya fue enviado", "Ok");
                }
                else if (SendingData)
                {
                    Application.Current.MainPage.DisplayAlert("Información", "Ya se está enviando un reporte", "Ok");
                }
            }
            catch (System.Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "Ok");
                throw;
            }
            finally
            {
                SendingDataCheck = false;
            }
        }
        public async Task DeleteReport()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                 "Confirmación",
                 "¿Deseas borrar este reporte?",
                 "Sí",
                 "No"
             );

                if (answer)
                {

                    using (var db = new ReportsDBContextSQLite())
                    {
                        db.Photos.RemoveRange(Report.Photos);
                        db.SaveChanges();

                        db.EntryExitReports.Remove(Report);
                        db.SaveChanges();
                        var paths = Report.Photos.Select(e => e.FilePath).ToList();
                        if (paths.Any())
                        {
                            // Ejecuta la eliminación de fotos en un hilo aparte
                            new Thread(() => DeletePhotos(paths)).Start();
                        }
                        var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                        Application.Current.MainPage.Navigation.RemovePage(d);
                    }
                }
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "Ok");
                throw;
            }
        }
        public async Task SendPDFReport()
        {
            try
            {
                // 1. Serializar el objeto 'Report' a JSON
                string jsonContent = JsonConvert.SerializeObject(Report, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                // 2. Generar un archivo y guardar el JSON en el almacenamiento local
                string fileName = "reporte.json";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                // Guardar el JSON en el archivo
                await File.WriteAllTextAsync(filePath, jsonContent);

                // 3. Compartir el archivo (opcional)
                await ShareFile(filePath, "Compartir Reporte de Entrada y Salida");
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", $"No se pudo compartir el archivo: {ex.Message}", "OK");
                // Manejo de errores
                Console.WriteLine($"Error al generar o enviar el reporte: {ex.Message}");
            }
        }
        private async Task DeletePhotos(List<string> paths)
        {
            foreach (var item in paths)
            {
                try
                {
                    File.Delete(item);
                }
                catch (Exception ex)
                {
                    // Puedes registrar el error o manejarlo según lo necesites
                    Console.WriteLine($"Error al eliminar el archivo {item}: {ex.Message}");
                }
            }
        }
        private void DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        byte[] pdfBytes = await d.EntryExitReport(Report); // Asegúrate de usar 'await' con métodos async.
                        var filePath = Path.Combine(FileSystem.CacheDirectory, $"Reporte Entrada {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf");
                        File.WriteAllBytes(filePath, pdfBytes);
                        ShareFile(filePath, "Compartir Reporte de Entrada y Salida");
                    }
                }
                catch (Exception e)
                {
                    Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
                    // Manejo de errores (log, mensajes, etc.)
                    Console.WriteLine($"Error: {e.Message}");
                }
            });
        }
        private async Task ShareFile(string filePath, string title)
        {
            try
            {
                // Compartir el archivo PDF usando el servicio de MAUI
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = title,
                    File = new ShareFile(filePath) // Usar "File" en lugar de "Files"
                });
            }
            catch (Exception ex)
            {
                // Manejo de errores en caso de que no se pueda compartir el archivo
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo compartir el archivo: {ex.Message}", "OK");
            }
        }
        private void SharePhotoAsync(string obj)
        {
            ShareFile(obj, "Imagen");
        }
        private async Task UpdateReport(bool state)
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Report.isUploaded = state;
                    db.EntryExitReports.Update(Report);
                    db.SaveChanges();
                }
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "Ok");    
                throw;
            }
        }
        #endregion

    }
}
