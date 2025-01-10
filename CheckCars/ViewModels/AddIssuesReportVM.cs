using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddIssuesReportVM : INotifyPropertyChangedAbst
    {
        public AddIssuesReportVM()
        {
            DeletePhotoCommand = new Command<Photo>(DeletePhoto);
            CarsInfo = GetCarsInfoAsync().Result;
            Task.Run(() => LoadUbicationAsync());
        }

        #region Properties
        private CheckCars.Utilities.SensorManager SensorManager = new();
        private ObservableCollection<Photo> _imgs = new();
        private IssueReport _newIssueReport = new()
        {
            Created = DateTime.Now,
        };
        private string[] _CarsInfo;
        public string[] CarsInfo
        {
            get { return _CarsInfo; }
            set
            {
                if (_CarsInfo != value)
                {
                    _CarsInfo = value;
                    OnPropertyChanged(nameof(CarsInfo));
                }
            }
        }
        public IssueReport newIssueReport
        {
            get { return _newIssueReport; }
            set
            {
                _newIssueReport = value;
                if (_newIssueReport != value)
                {
                    OnPropertyChanged(nameof(newIssueReport));
                }
            }
        }
        public ObservableCollection<Photo> ImgList
        {
            get { return _imgs; }
            set
            {
                if (_imgs != value)
                {
                    _imgs = value;
                    OnPropertyChanged(nameof(ImgList));  // Notificamos el cambio de lista
                }
            }
        }

        #endregion

        #region Commands
        public ICommand AddReport
        {
            get
            {
                return new Command(() => AddReportEntryAsync());
            }
            private set { }
        }
        public ICommand DeletePhotoCommand { get; }
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhotosAsync));
            }
            private set { }
        }
        #endregion

        #region Methods
        private async Task TakePhotosAsync()
        {
            Photo photo = await SensorManager.TakePhoto();
            if (photo != null)
            {
                ImgList.Add(photo);
            }
        }
        private async Task AddReportEntryAsync()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas continuar?",
                    "Sí",
                    "No"
                );
                bool valid = await ValidateDataAsync();

                if (answer && valid)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        newIssueReport.Author = string.IsNullOrWhiteSpace(StaticData.User.UserName) ? "Default" : StaticData.User.UserName;

                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newIssueReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = Guid.NewGuid().ToString();
                            return photo;
                        }).ToList();

                        db.IssueReports.Add(newIssueReport);
                        db.SaveChanges();
                        CloseAsync();
                    }
                }
                else if (answer && !valid)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Verifique la información", "ok");
                }
            }
            catch (Exception rf)
            {
                Console.WriteLine(rf.ToString());

            }
        }
        private async Task CloseAsync()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }
        private void DeletePhoto(Photo photo)
        {
            if (photo == null) return; // Evitar argumentos nulos

            try
            {
                if (File.Exists(photo.FilePath))
                {
                    File.Delete(photo.FilePath);
                }
                ImgList.Remove(photo); // Eliminar de la lista
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, loguearla)
                Console.WriteLine($"Error al eliminar la foto: {ex.Message}");
            }
        }
        private async Task<bool> ValidateDataAsync()
        {

            if (string.IsNullOrEmpty(newIssueReport.CarPlate))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newIssueReport.Details))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newIssueReport.Type))
            {
                return false;
            }
            return true;

        }
        private async Task<string[]> GetCarsInfoAsync()
        {
            using (var db = new ReportsDBContextSQLite())
            {
                return (from C in db.Cars
                        select $"{C.Brand}-{C.Model}-{C.Plate}"
                            ).ToArray();
            }
        }
        private async Task LoadUbicationAsync()
        {
            try
            {
                double[] location = await SensorManager.GetCurrentLocation();

                if (location != null)
                {
                    newIssueReport.Latitude = location[0];
                    newIssueReport.Longitude = location[1];
                }
                else
                {
                    newIssueReport.Latitude = 0;
                    newIssueReport.Longitude = 0;
                }
            }
            catch (Exception)
            {
                SensorManager.CancelRequest();

            }
        }
        #endregion
    }
}
