using CheckCar.Data;
using CheckCar.Models;
using System.Windows.Input;
using CheckCar.Utilities;

namespace CheckCar.ViewModels
{
    public class AccountVM : INotifyPropertyChangedAbst
    {
        #region Constructor 

        /// <summary>
        /// Initializes the AccountVM instance,
        /// sets default user profile, and loads saved preferences for username, URL, and port.
        /// </summary>
        public AccountVM()
        {
            // Initialize commands
            UpdateUser = new Command(async () => await UpdateUserAsync());
            CleanPdfs = new Command(async () => await DeletePdfAsync());
            CleanReports = new Command(async () => await DeleteReportsAsync());

            // Load user profile and settings
            StaticData.User = new UserProfile();
            var name = Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");

            // Set local user profile
            LocalUser.UserName = name;
            URL = StaticData.URL;
            Port = StaticData.Port;
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command to delete all reports asynchronously after user confirmation.
        /// </summary>
        public ICommand CleanReports   {  get;}

        /// <summary>
        /// Command to delete cached PDFs asynchronously after user confirmation.
        /// </summary>
        public ICommand CleanPdfs  { get;  }

        /// <summary>
        /// Command to update the user profile asynchronously.
        /// </summary>
        public ICommand UpdateUser  { get; }

        /// <summary>
        /// Command to handle changes in API usage (unimplemented).
        /// </summary>
        public ICommand IOnChangeUseAPI;

        #endregion



        #region Properties

        private string _url;

        /// <summary>
        /// Gets or sets the API base URL.
        /// Trims trailing slashes and updates StaticData.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the port.
        /// Updates StaticData on change.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the local user profile.
        /// Notifies property changes.
        /// </summary>
        public UserProfile LocalUser
        {
            get { return _User; }
            set
            {
                if (_User != value)
                {
                    _User = value;
                    OnPropertyChanged(nameof(LocalUser));  // Notify change
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes all reports asynchronously after user confirmation.
        /// Deletes related photos and clears related database tables.
        /// </summary>
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
                        await MessageUtilities.ShowLongToast("Reportes Borrados");
                    }
                }
            }
            catch (Exception ec)
            {
                await MessageUtilities.ShowInfoMessageAsync("Error", "Error al borrar los reportes. Error:" + ec.Message);
            }
        }

        /// <summary>
        /// Deletes a list of photo files asynchronously.
        /// </summary>
        /// <param name="photos">List of file paths to delete.</param>
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
                await MessageUtilities.ShowInfoMessageAsync("Error", "Error al borrar las fotos. Error:" + e.Message);
            }
        }

        /// <summary>
        /// Deletes cached PDF files asynchronously after user confirmation.
        /// </summary>
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
                    await MessageUtilities.ShowLongToast("Cache Borrada");
                }
            }
            catch (Exception e)
            {

                await MessageUtilities.ShowInfoMessageAsync("Error", "Error al borrar el cache. Error:" + e.Message);
            }
        }

        /// <summary>
        /// Updates the user profile and saves the username in preferences asynchronously.
        /// </summary>
        private async Task UpdateUserProfileAsync()
        {
            try
            {
                Preferences.Set(nameof(UserProfile.UserName), LocalUser.UserName);
                StaticData.User.UserName = LocalUser.UserName;
                await MessageUtilities.ShowLongToast("Usuario Actualizado");
            }
            catch (Exception ed)
            {
                await MessageUtilities.ShowInfoMessageAsync("Error", "Error al actualizar el usuario. Error:" + ed.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task UpdateUserAsync()
        {
            try
            {
                await UpdateUserProfileAsync();
            }
            catch (Exception f)
            {
                MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error:" + f.Message);
            }
        }



        #endregion
    }
}
