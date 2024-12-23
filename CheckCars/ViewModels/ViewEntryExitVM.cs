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

        private readonly APIService _apiService;
        private Thread reportThread = new Thread(() => { });
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
        public ICommand DownloadReportCommand { get; }
        public ICommand ISendReport { get; }
        public ICommand IDeleteReport { get; }
        public ICommand ISendServerReport { get; }

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

        }

        #region Methods
        public async Task SendServer()
        {
            try
            {
                if (!Report.isUploaded)
                {
                    var result = await _apiService.PostAsync<EntryExitReport>("api/EntryExitReports", Report);
                    if (result)
                    {
                        using (var db = new ReportsDBContextSQLite())
                        {
                            Report.isUploaded = true;
                            db.EntryExitReports.Update(Report);
                            db.SaveChanges();
                        }
                        Application.Current.MainPage.DisplayAlert("Información", "Datos enviados al servidor", "Ok");
                    }
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public async Task DeleteReport()
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
                await ShareFile(filePath);
            }
            catch (Exception ex)
            {
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
                        ShareFile(filePath);
                    }
                }
                catch (Exception e)
                {
                    // Manejo de errores (log, mensajes, etc.)
                    Console.WriteLine($"Error: {e.Message}");
                }
            });
        }
        private async Task ShareFile(string filePath)
        {
            try
            {
                // Compartir el archivo PDF usando el servicio de MAUI
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Compartir Reporte de Entrada y Salida",
                    File = new ShareFile(filePath) // Usar "File" en lugar de "Files"
                });
            }
            catch (Exception ex)
            {
                // Manejo de errores en caso de que no se pueda compartir el archivo
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo compartir el archivo: {ex.Message}", "OK");
            }
        }
        #endregion

    }
}
