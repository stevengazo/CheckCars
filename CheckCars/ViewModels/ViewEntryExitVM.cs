using CheckCars.Data;
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

        /// <summary>
        /// API service used to send reports to the server.
        /// </summary>
        private readonly APIService _apiService;

        /// <summary>
        /// Thread used for handling report operations.
        /// </summary>
        private Thread reportThread = new Thread(() => { });

        private bool _SendingDataCheck;

        /// <summary>
        /// Flag to indicate if data is currently being sent (for SendServer).
        /// </summary>
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

        /// <summary>
        /// Indicates if a report is being processed or sent.
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

        private double _FuelLevel;

        /// <summary>
        /// Fuel level of the vehicle represented as a percentage (0–1).
        /// </summary>
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

        /// <summary>
        /// The current entry/exit report being displayed or edited.
        /// </summary>
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

        #region Commands

        /// <summary>
        /// Command to download the current report as PDF.
        /// </summary>
        public ICommand DownloadReportCommand { get; }

        /// <summary>
        /// Command to share the report as a JSON file.
        /// </summary>
        public ICommand ISendReport { get; }

        /// <summary>
        /// Command to delete the current report.
        /// </summary>
        public ICommand IDeleteReport { get; }

        /// <summary>
        /// Command to share a photo from the report.
        /// </summary>
        public ICommand IShareImage { get; }

        /// <summary>
        /// Command to send the report data to the server.
        /// </summary>
        public ICommand ISendServerReport { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewEntryExitVM"/> class.
        /// Loads the report data from the local database.
        /// </summary>
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
        /// Sends the report to the server (with or without images).
        /// </summary>
        public async Task SendServer()
        {
            try
            {
                SendingDataCheck = true;

                if (!Report.isUploaded && !SendingData)
                {
                    TimeSpan tp = TimeSpan.FromSeconds(100);
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

        /// <summary>
        /// Deletes the current report and its associated photos.
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

                        db.EntryExitReports.Remove(Report);
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
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "Ok");
                throw;
            }
        }

        /// <summary>
        /// Serializes the report as JSON and shares it.
        /// </summary>
        public async Task SendPDFReport()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(Report, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                string fileName = "reporte.json";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(filePath, jsonContent);

                await ShareFile(filePath, "Compartir Reporte de Entrada y Salida");
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", $"No se pudo compartir el archivo: {ex.Message}", "OK");
                Console.WriteLine($"Error al generar o enviar el reporte: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a list of photo files from the file system.
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
        /// Generates a PDF from the report and shares it.
        /// </summary>
        private void DownloadReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Report != null)
                    {
                        PDFGenerate d = new PDFGenerate();
                        byte[] pdfBytes = await d.EntryExitReport(Report);
                        var filePath = Path.Combine(FileSystem.CacheDirectory, $"Reporte Entrada {Report.CarPlate} {DateTime.Now:yy-MM-dd hh-mm-ss}.pdf");
                        File.WriteAllBytes(filePath, pdfBytes);
                        ShareFile(filePath, "Compartir Reporte de Entrada y Salida");
                    }
                }
                catch (Exception e)
                {
                    Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
                    Console.WriteLine($"Error: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Shares a file using the native sharing feature.
        /// </summary>
        private async Task ShareFile(string filePath, string title)
        {
            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = title,
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo compartir el archivo: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Shares a specific photo file.
        /// </summary>
        private void SharePhotoAsync(string obj)
        {
            ShareFile(obj, "Imagen");
        }

        /// <summary>
        /// Updates the local database to mark the report as uploaded.
        /// </summary>
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
