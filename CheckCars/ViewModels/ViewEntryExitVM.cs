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
            DownloadReportCommand = new Command(async () => await DownloadReport());
        }

        // Propiedad para el comando DownloadReport
        public ICommand DownloadReportCommand { get; } 



        // Método para descargar el reporte en formato PDF
        private async Task DownloadReport()
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
            catch (NullReferenceException e)
            {

                throw;
            }
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
