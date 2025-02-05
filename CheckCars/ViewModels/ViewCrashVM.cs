using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class ViewCrashVM : INotifyPropertyChangedAbst
    {
        #region Properties
        private readonly APIService _apiService = new APIService();
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

        private bool _SendingDataF = false;
        public bool SendingDataF
        {
            get { return _SendingDataF; }
            set
            {
                if (_SendingDataF != value)
                {
                    _SendingDataF = value;
                    OnPropertyChanged(nameof(SendingDataF));
                }
            }
        }
        #endregion
        public ViewCrashVM()
        {
            var Id = Data.StaticData.ReportId;

            IDeleteReport = new Command(async () => await DeleteReport());
            DownloadReportCommand = new Command(async () => await DownloadReport());
            ISendReportCommand = new Command(async () => await SendDataAsync());
            IShareImage = new Command<string>(SharePhotoAsync);

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.CrashReports.Include(E => E.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));

            }
        }

        #region Commands   
        public ICommand IDeleteReport { get; }
        public ICommand DownloadReportCommand { get; }
        public ICommand ISendReportCommand { get; }
        public ICommand IShareImage { get; }
        #endregion
        #region Methods
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
        private async Task ShareFile(string filePath)
        {
            var request = new ShareFileRequest
            {
                Title = "Enviar Reporte",
                File = new ShareFile(filePath)
            };

            await Share.Default.RequestAsync(request);
        }
        private void SharePhotoAsync(string obj)
        {
            ShareFile(obj);
        }

        private async Task SendDataAsync()
        {
            try
            {
                SendingDataF = true;

                TimeSpan time = TimeSpan.FromSeconds(30);
                bool result;
                if (Report.Photos.Count > 0)
                {
                    List<string> paths = Report.Photos.Select(e => e.FilePath).ToList();
                    result = await _apiService.PostAsync("api/CrashReports/form", Report, paths, time);
                }
                else
                {
                    result = await _apiService.PostAsync("api/CrashReports/json", Report, time);
                }
                if (result)
                {
                    Application.Current?.MainPage.DisplayAlert("Información", "Reporte enviado correctamente", "Ok");
                    await UpdateReport(result);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                //throw;
            }
            finally
            {
                SendingDataF = false;
            }
        }
        private async Task UpdateReport(bool e)
        {
            using (var db = new ReportsDBContextSQLite())
            {
                Report.isUploaded = e;
                db.CrashReports.Update(Report);
                db.SaveChanges();
            }
        }
        #endregion

    }
}
