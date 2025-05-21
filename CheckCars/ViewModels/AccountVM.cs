using CheckCars.Data;
using CheckCars.Models;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AccountVM : INotifyPropertyChangedAbst
    {
        #region Constructor 

        public AccountVM()
        {
            StaticData.User = new UserProfile();
            var name = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
           
            LocalUser.UserName = name;
            URL = StaticData.URL;
            Port = StaticData.Port;
        }
        #endregion

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


        public ICommand IOnChangeUseAPI;

        #endregion

        #region Properties
    


        private string _url;

        public string URL
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {

                    _url = value.TrimEnd('/');
                    
                            StaticData.URL = value;
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
                    StaticData.Port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }




        private UserProfile _User = new();
        public UserProfile LocalUser
        {
            get { return _User; }
            set
            {
                if (_User != value)
                {
                    _User = value;
                    OnPropertyChanged(nameof(LocalUser));  // Notificamos el cambio de lista
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
            catch (Exception ec)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al borrar la base de datos", "Ok");
                Console.Write(ec.Message);
            }
        }
        private async Task DeletePhotosAsync(List<string> photos)
        {
            try
            {
                foreach (var photo in photos)
                {
                    File.Delete(photo);
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al borrar las fotos", "Ok");  
            }
        }
        private async Task DeletePdfAsync()
        {
            try
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
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al borrar el cache", "Ok");
            }
        }
        private async Task UpdateUserProfileAsync()
        {
            try
            {
                Preferences.Set(nameof(UserProfile.UserName), LocalUser.UserName);
                StaticData.User.UserName = LocalUser.UserName;
                Application.Current.MainPage.DisplayAlert("Información", "Usuario Actualizado", "Ok");
            }
            catch (Exception ed)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al actualizar el usuario", "Ok");
            }
        }

        #endregion
    }
}
