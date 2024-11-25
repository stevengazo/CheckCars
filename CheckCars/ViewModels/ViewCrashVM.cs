using vehiculosmecsa.Data;
using vehiculosmecsa.Models;
using vehiculosmecsa.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vehiculosmecsa.ViewModels
{
   public class ViewCrashVM : INotifyPropertyChangedAbst
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



        private CrashReport _Report = new();
        public CrashReport Report
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
        public ViewCrashVM()
        {
            var Id = Data.StaticData.ReportId;

            IDeleteReport = new Command(async () => await DeleteReport());
            DownloadReportCommand = new Command(async () => await DownloadReport());

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.CrashReports.Include(E => E.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));

            }

        }
        public ICommand IDeleteReport { get; }
        public ICommand DownloadReportCommand { get; }


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

        private async Task DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        byte[] pdfBytes = await d.CrashReports(Report); // Asegúrate de usar 'await' con métodos async.
                        var filePath = Path.Combine(FileSystem.CacheDirectory, $"Accidente {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf");
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
                    db.CrashReports.Remove(Report);
                    db.SaveChanges();

                    var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

                    var paths = Report.Photos.Select(e => e.FilePath).ToList();
                    if (paths.Any())
                    {
                        // Ejecuta la eliminación de fotos en un hilo aparte
                        new Thread(() => DeletePhotos(paths)).Start();
                    }
                    Application.Current.MainPage.Navigation.RemovePage(d);

                }
            }
        }
    }
}
