using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class ViewIssueVM : INotifyPropertyChangedAbst
    {
        private readonly APIService _apiService;
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
            _apiService = new APIService(); 

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
                SendingData = true;
                if(Data.StaticData.UseAPI)
                {
                    var result = false;
                    if(Report.Photos.Count>0)
                    {
                        var photosPaths = Report.Photos.Select(e=> e.FilePath).ToList();
                        result = await _apiService.PostAsync<IssueReport>("api/IssueReports/form", Report,photosPaths, TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        result = await _apiService.PostAsync<IssueReport>("api/IssueReports/json", Report, TimeSpan.FromSeconds(5)); 
                    }
                    if (result)
                    {
                        Application.Current.MainPage.DisplayAlert("Información", "Datos enviados al servidor", "Ok");
                    }
                    else
                    {
                        Application.Current.MainPage.DisplayAlert("Información", "Error al enviar los datos\nIntentelo más tarde", "Ok");
                    }

                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al generar o enviar el reporte: {ex.Message}");
            }
            finally
            {
                SendingData = false;
            }
        }
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
