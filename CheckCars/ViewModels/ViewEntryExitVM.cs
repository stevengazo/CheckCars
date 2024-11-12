using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class ViewEntryExitVM : INotifyPropertyChangedAbst
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

        public ViewEntryExitVM()
        {
            var Id = Data.StaticData.ReportId;

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.EntryExitReports.Include(E=>E.Photos).FirstOrDefault(e => e.ReportId == Id);
            }

            if (Report != null) {
                FuelLevel = Report.FuelLevel / 100;
            
            }
            DownloadReportCommand = new Command( () =>  DownloadReport());
            IDeleteReport = new Command(async () => await DeleteReport());
        }

        // Propiedad para el comando DownloadReport
        public ICommand DownloadReportCommand { get; } 
        public ICommand IDeleteReport {  get; }


        public async Task DeleteReport()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                   "Confirmación",
                   "¿Deseas borrar este reporte?",
                   "Sí",
                   "No"
               );

            if( answer )
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



        private bool isDownloading = false;

        private void DownloadReport()
        {
            // Verificar si ya se está descargando el reporte
            if (isDownloading)
            {
                Application.Current.MainPage.DisplayAlert("Información", "El reporte ya se encuentra generándose", "ok");
                return;
            }

            // Marcar como en proceso de descarga
            isDownloading = true;

            // Ejecutar el proceso en un hilo separado
            Thread reportThread = new Thread(new ThreadStart(async () =>
            {
                try
                {
                    
                    // Verificar si el reporte existe
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        // Generar el PDF
                        byte[] pdfBytes = await d.EntryExitReport(Report);

                        // Guardar el archivo PDF en el almacenamiento local
                        var filePath = Path.Combine(FileSystem.CacheDirectory, "reporte.pdf");
                        File.WriteAllBytes(filePath, pdfBytes);

                        // Llamar a la función para compartir el archivo
                        await SharePdf(filePath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
                finally
                {
                    // Liberar la bandera al finalizar el proceso
                    isDownloading = false;
                }
            }));

            // Iniciar el hilo
            reportThread.Start();
        }

        // Método para compartir el PDF
        private async Task SharePdf(string filePath)
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


    }
}
