using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CheckCars.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for handling issue reports in the CheckCars application.
    /// Provides functionality to view, send, delete, and share reports.
    /// </summary>
    public class ViewIssueVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewIssueVM"/> class.
        /// Loads the issue report from the local database and sets up commands.
        /// </summary>
        public ViewIssueVM()
        {
            var Id = Data.StaticData.ReportId;
            _apiService = new APIService();

            IDeleteReport = new Command(async () => await DeleteReport());
            ISendReport = new Command(async () => await SendReport());
            IShareImage = new Command<string>(SharePhotoAsync);
            DownloadReportCommand = new Command(async () => await DownloadReport());

            using (var dbo = new ReportsDBContextSQLite())
            {
                Report = dbo.IssueReports.Include(E => E.Photos).FirstOrDefault(e => e.ReportId.Equals(Id));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// API service used to send data to the server.
        /// </summary>
        private readonly APIService _apiService;

        private bool _SendingData = false;

        /// <summary>
        /// Indicates whether the report is currently being sent to the server.
        /// </summary>
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

        /// <summary>
        /// The issue report currently loaded in the view.
        /// </summary>
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

        #endregion

        #region Commands

        /// <summary>
        /// Command to download the report as a PDF file.
        /// </summary>
        public ICommand DownloadReportCommand { get; }

        /// <summary>
        /// Command to delete the current issue report.
        /// </summary>
        public ICommand IDeleteReport { get; }

        /// <summary>
        /// Command to send the issue report to the server.
        /// </summary>
        public ICommand ISendReport { get; }

        /// <summary>
        /// Command to share a photo from the report.
        /// </summary>
        public ICommand IShareImage { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes photo files from local storage.
        /// </summary>
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
                    Console.WriteLine($"Error al eliminar el archivo {item}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Deletes the current report and its associated photos from the local database.
        /// </summary>
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
                        db.IssueReports.Remove(Report);
                        db.SaveChanges();

                        var paths = Report.Photos.Select(e => e.FilePath).ToList();
                        if (paths.Any())
                        {
                            new Thread(() => DeletePhotos(paths)).Start();
                        }

                        var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                        Application.Current.MainPage.Navigation.RemovePage(d);
                    }
                }
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "OK");
                throw;
            }
        }

        /// <summary>
        /// Downloads the issue report as a PDF and shares the file.
        /// </summary>
        private async Task DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        byte[] pdfBytes = await d.IssueReport(Report);
                        var filePath = Path.Combine(FileSystem.CacheDirectory, $"Problema {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf");
                        File.WriteAllBytes(filePath, pdfBytes);
                        ShareFile(filePath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Sends the issue report to the server via API with or without photos.
        /// </summary>
        public async Task SendReport()
        {
            try
            {
                SendingData = true;

                var result = false;
                if (Report.Photos.Count > 0)
                {
                    var photosPaths = Report.Photos.Select(e => e.FilePath).ToList();
                    result = await _apiService.PostAsync<IssueReport>("api/IssueReports/form", Report, photosPaths, TimeSpan.FromSeconds(10));
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
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
                Console.WriteLine($"Error al generar o enviar el reporte: {ex.Message}");
            }
            finally
            {
                SendingData = false;
            }
        }

        /// <summary>
        /// Shares a file using the native share feature.
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
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "OK");
            }
        }

        /// <summary>
        /// Shares a photo by file path using the Share API.
        /// </summary>
        /// <param name="obj">File path of the image to share.</param>
        private void SharePhotoAsync(string obj)
        {
            ShareFile(obj);
        }

        #endregion
    }
}
