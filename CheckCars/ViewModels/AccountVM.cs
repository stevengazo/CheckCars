using CheckCars.Data;
using CheckCars.Models;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AccountVM : INotifyPropertyChangedAbst
    {


        public AccountVM()
        {
            StaticData.User = new UserProfile();
            StaticData.User.UserName = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
            UseAPI = StaticData.UseAPI;
            User.UserName = StaticData.User.UserName;
            URL = StaticData.URL;
            Port = StaticData.Port;
        }

        #region Commands
        public ICommand CleanReports
        {
            get
            {
                return new Command(async () => await DeleteReportsAsync());
            }
            private set { }
        }
        public ICommand CleanPdfs
        {
            get
            {
                return new Command(async () => await DeletePdfAsync());
            }
            private set { }
        }
        public ICommand UpdateUser
        {
            get
            {
                return new Command(async () => await UpdateUserProfileAsync());
            }
            private set { }
        }

        #endregion

        #region Properties
        private bool _UseAPI;

        public bool UseAPI
        {
            get { return _UseAPI; }
            set { _UseAPI = value; }
        }



        private string _url;

        public string URL
        {
            get { return _url; }
            set { if(_url != value){
                    _url = value;
                    OnPropertyChanged(nameof(URL));
                }
            }
        }
        private string _Port;

        public string Port
        {
            get { return _Port; }
            set
            {
                if (_Port != value)
                {
                    _Port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }




        private UserProfile _User = new();
        public UserProfile User
        {
            get { return _User; }
            set
            {
                if (_User != value)
                {
                    _User = value;
                    OnPropertyChanged(nameof(User));  // Notificamos el cambio de lista
                }
            }
        }
        #endregion

        #region Methods
        private async Task DeleteReportsAsync()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                  "Confirmación",
                  "¿Deseas borrar todos los reportes?",
                  "Sí",
                  "No"
              );
                if (answer)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        var photosPaths = db.Photos.Select(e => e.FilePath).ToList();
                        DeletePhotosAsync(photosPaths);
                        db.Photos.RemoveRange(db.Photos.ToList());
                        db.Reports.RemoveRange(db.Reports.ToList());
                        db.IssueReports.RemoveRange(db.IssueReports.ToList());
                        db.CrashReports.RemoveRange(db.CrashReports.ToList());
                        db.SaveChanges();
                        await Application.Current.MainPage.DisplayAlert("Información", "Base de Datos Borrada", "Ok");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        private async Task DeletePhotosAsync(List<string> photos)
        {
            foreach (var photo in photos)
            {
                File.Delete(photo);
            }
        }
        private async Task DeletePdfAsync()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                  "Confirmación",
                  "¿Deseas limpiar cache?",
                  "Sí",
                  "No"
              );
            if (answer)
            {
                var path = FileSystem.CacheDirectory;
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                Application.Current.MainPage.DisplayAlert("Información", "Cache Borrada", "Ok");
            }
        }

        private async Task UpdateUserProfileAsync()
        {
            StaticData.User.UserName = User.UserName;
            Preferences.Set(nameof(UserProfile.UserName), User.UserName);
            Application.Current.MainPage.DisplayAlert("Información", "Usuario Actualizado", "Ok");
        }

        #endregion
    }
}
