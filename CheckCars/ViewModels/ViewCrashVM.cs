using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing crash reports.
    /// </summary>
    public class ViewCrashVM : INotifyPropertyChangedAbst
    {
        #region Properties

        private readonly APIService _apiService = new APIService();

        private CrashReport _Report = new();

        /// <summary>
        /// Gets or sets the crash report being viewed.
        /// </summary>
        public CrashReport Report
        {
            get => _Report;
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

        /// <summary>
        /// Gets or sets a value indicating whether the report is currently being sent.
        /// </summary>
        public bool SendingDataF
        {
            get => _SendingDataF;
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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewCrashVM"/> class.
        /// Loads the crash report and sets up commands.
        /// </summary>
        public ViewCrashVM()
        {
            try
            {
                var Id = Data.StaticData.ReportId;

                IDeleteReport = new Command(async () => await DeleteReport());
                DownloadReportCommand = new Command(async () => await DownloadReport());
                ISendReportCommand = new Command(async () => await SendDataAsync());
                IShareImage = new Command<string>(SharePhotoAsync);

                using (var dbo = new ReportsDBContextSQLite())
                {
                    Report = dbo.CrashReports.Include(e => e.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                throw;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to delete the crash report.
        /// </summary>
        public ICommand IDeleteReport { get; }

        /// <summary>
        /// Command to download the crash report as a PDF.
        /// </summary>
        public ICommand DownloadReportCommand { get; }

        /// <summary>
        /// Command to send the crash report to the server.
        /// </summary>
        public ICommand ISendReportCommand { get; }

        /// <summary>
        /// Command to share an image associated with the report.
        /// </summary>
        public ICommand IShareImage { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the crash report and associated photos from the database and filesystem.
        /// </summary>
        public async Task DeleteReport()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirmación",
                "¿Deseas borrar este reporte?",
                "Sí",
                "No"
            );

            if (confirm)
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    db.Photos.RemoveRange(Report.Photos);
                    db.SaveChanges();

                    db.CrashReports.Remove(Report);
                    db.SaveChanges();

                    var pageToRemove = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

                    var paths = Report.Photos.Select(e => e.FilePath).ToList();
                    if (paths.Any())
                    {
                        new Thread(() => DeletePhotos(paths)).Start();
                    }

                    Application.Current.MainPage.Navigation.RemovePage(pageToRemove);
                }
            }
        }

        /// <summary>
        /// Deletes a list of image files from the filesystem.
        /// </summary>
        private void DeletePhotos(List<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    Console.WriteLine($"Error al eliminar el archivo {path}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generates and downloads a PDF of the crash report, then shares it.
        /// </summary>
        private async Task DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate generator = new PDFGenerate();
                        byte[] pdfBytes = await generator.CrashReports(Report);

                        string filePath = Path.Combine(
                            FileSystem.CacheDirectory,
                            $"Accidente {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf"
                        );

                        File.WriteAllBytes(filePath, pdfBytes);
                        ShareFile(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Shares a PDF file using the device's native share dialog.
        /// </summary>
        private async Task ShareFile(string filePath)
        {
            try
            {
                var request = new ShareFileRequest
                {
                    Title = "Enviar Reporte",
                    File = new ShareFile(filePath)
                };

                await Share.Default.RequestAsync(request);
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                throw;
            }
        }

        /// <summary>
        /// Shares a single photo file.
        /// </summary>
        private void SharePhotoAsync(string filePath)
        {
            ShareFile(filePath);
        }

        /// <summary>
        /// Sends the crash report and its photos to the server.
        /// </summary>
        private async Task SendDataAsync()
        {
            try
            {
                SendingDataF = true;

                bool result;
                TimeSpan timeout = TimeSpan.FromSeconds(30);

                if (Report.Photos.Count > 0)
                {
                    List<string> paths = Report.Photos.Select(e => e.FilePath).ToList();
                    result = await _apiService.PostAsync("api/CrashReports/form", Report, paths, timeout);
                }
                else
                {
                    result = await _apiService.PostAsync("api/CrashReports/json", Report, timeout);
                }

                if (result)
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "Reporte enviado correctamente", "Ok");
                    await UpdateReport(result);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al enviar el reporte. " + ex.Message, "OK");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                SendingDataF = false;
            }
        }

        /// <summary>
        /// Updates the local report to indicate it has been uploaded.
        /// </summary>
        private async Task UpdateReport(bool uploaded)
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Report.isUploaded = uploaded;
                    db.CrashReports.Update(Report);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
                throw;
            }
        }

        #endregion
    }
}
