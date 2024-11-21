using CheckCars.Data;
using CheckCars.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AccountVM : INotifyPropertyChangedAbst
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

        public ICommand CleanReports
        {
            get
            {
                return new Command(async() => await DeleteReports());
            }
            private set { }
        }
        public ICommand CleanPdfs
        {
            get
            {
                return new Command(async() => await DeletePdf());
            }
            private set { }
        }
        public ICommand UpdateUser
        {
            get
            {
                return new Command(async () => await UpdateUserProfile());
            }
            private set { }
        }
        private async Task UpdateUserProfile()
        {
            StaticData.User.UserName = User.UserName;
            Preferences.Set(nameof(UserProfile.UserName), User.UserName);
            Application.Current.MainPage.DisplayAlert("Información", "Usuario Actualizado", "Ok");

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
        public AccountVM()
        {
            StaticData.User = new UserProfile();
            StaticData.User.UserName = Preferences.Get(nameof(UserProfile.UserName), "Default User");
            User.UserName = StaticData.User.UserName;   


            
        }
        private async Task DeleteReports()
        {
            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                  "Confirmación",
                  "¿Deseas borrar todos los reportes?",
                  "Sí",
                  "No"
              );
                if(answer)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        var photosPaths = db.Photos.Select(e=>e.FilePath).ToList();
                        DeletePhotos(photosPaths);
                        db.Photos.RemoveRange(db.Photos.ToList());
                        db.Reports.RemoveRange(db.Reports.ToList());
                        db.IssueReports.RemoveRange(db.IssueReports.ToList());
                        db.CrashReports.RemoveRange(db.CrashReports.ToList());
                        db.SaveChanges();
                        Application.Current.MainPage.DisplayAlert("Información", "Base de Datos Borrada", "Ok");
                    }
                }
            }
            catch (Exception e)
            {
              
                throw;
            }

        }
        private async Task DeletePhotos(List<string> photos)
        {
            foreach (var photo in photos)
            {
                File.Delete(photo);
            }
        }
        private async Task DeletePdf()
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
                foreach ( var file in files )
                {
                    File.Delete( file );
                }
                Application.Current.MainPage.DisplayAlert("Información", "Cache Borrada", "Ok");
            }
        }
    }
}
