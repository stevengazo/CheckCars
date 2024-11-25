using vehiculosmecsa.Data;
using vehiculosmecsa.Models;
using vehiculosmecsa.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vehiculosmecsa.ViewModels
{
    public class ViewIssueVM : INotifyPropertyChangedAbst
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

        private IssueReport _Report = new();
        public IssueReport Report
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
        public ViewIssueVM()
        {
            var Id = Data.StaticData.ReportId;

            IDeleteReport = new Command(async () => await DeleteReport());
            ISendReport = new Command(async () => await SendReport());

            DownloadReportCommand = new Command(async () => await DownloadReport());

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.IssueReports.Include(E => E.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));

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
        public ICommand DownloadReportCommand { get; }
        public ICommand IDeleteReport { get; }
        public ICommand ISendReport { get; }
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
                    db.IssueReports.Remove(Report);
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
        private async Task DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        byte[] pdfBytes = await d.IssueReport(Report); // Asegúrate de usar 'await' con métodos async.
                        var filePath = Path.Combine(FileSystem.CacheDirectory, $"Problema {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf");
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
        public async Task SendReport()
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
        // Método para compartir el archivo usando el sistema de compartición de MAUI
        private async Task ShareFile(string filePath)
        {
            var request = new ShareFileRequest
            {
                Title = "Enviar Reporte",
                File = new ShareFile(filePath)
            };

            await Share.Default.RequestAsync(request);
        }

    }
}
