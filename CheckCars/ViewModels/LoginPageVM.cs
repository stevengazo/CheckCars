using CommunityToolkit.Maui;
using Newtonsoft.Json;
using ReviCar.Data;
using ReviCar.Models;
using ReviCar.Services;
using ReviCar.Utilities;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReviCar.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing the login process, including user credentials,
    /// server connection, token validation, and navigation.
    /// </summary>
    public class LoginPageVM : INotifyPropertyChangedAbst
    {
        #region Fields

        private readonly APIService _apiService = new APIService();
        private string _UserName;
        private string _Password;
        private string _Server = "https://checarsv2.stevengazo.co.cr/";
        private string _ErrorMessage;
        private bool _IsBusy = false;
        private bool _IsErrorVisible = false;
        private bool _ShowServerEntry = false;

        #endregion

        #region Properties

        public bool ShowServerEntry
        {
            get => _ShowServerEntry;
            set => SetProperty(ref _ShowServerEntry, value);
        }
        public string UserName
        {
            get => _UserName;
            set => SetProperty(ref _UserName, value);
        }

        public string Password
        {
            get => _Password;
            set => SetProperty(ref _Password, value);
        }

        public string Server
        {
            get => _Server;
            set => SetProperty(ref _Server, value);
        }

        public string ErrorMessage
        {
            get => _ErrorMessage;
            set => SetProperty(ref _ErrorMessage, value);
        }

        public bool IsBusy
        {
            get => _IsBusy;
            set => SetProperty(ref _IsBusy, value);
        }

        public bool IsErrorVisible
        {
            get => _IsErrorVisible;
            set => SetProperty(ref _IsErrorVisible, value);
        }

        // Lista observable para vehículos cargados
        public ObservableCollection<CarModel> Cars { get; } = new ObservableCollection<CarModel>();

        #endregion

        #region Constructor

        public LoginPageVM()
        {
            try
            {
                Login = new Command(async () => await SignInAsync());
                _ = LoadTokenAsync();

                if (!string.IsNullOrEmpty(StaticData.URL))
                {
                    StaticData.URL = "https://checarsv2.stevengazo.co.cr/";
                    Server = StaticData.URL;
                }
            }
            catch (Exception ex)
            {
                MessageUtilities.ShowInfoMessageAsync("Error", ex.Message);
            }
        }

        #endregion

        #region Commands

        public ICommand Login { get; set; }
        public ICommand ToggleServerEntryCommand => new Command(() =>
        {
            ShowServerEntry = !ShowServerEntry;
        }); 

        #endregion

        #region Methods

        public async Task SignInAsync()
        {
            try
            {
                await _apiService.UpdateUrl(Server);

                if (!EstaConectado())
                    throw new HttpRequestException("No hay conexión a internet");

                IsBusy = true;

                var data = new DataSignIn { UserName = UserName, password = Password };
                await ValidateAndAssignServerUrl();

                (bool sucess, string response) respon = await _apiService.PostAsync<DataSignIn>("api/Account/login", data);

                dynamic jSonData = JsonConvert.DeserializeObject(respon.response);
                string token = jSonData.token;

                if (respon.sucess)
                {
                    IsErrorVisible = false;
                    SecureStorage.Remove("token");

                    StaticData.User = new UserProfile();
                    Preferences.Set(nameof(UserProfile.UserName), UserName);
                    StaticData.User.UserName = UserName;

                    await SecureStorage.SetAsync("token", token);

                    // Cargar vehículos en hilo separado sin bloquear UI
                    _ = Task.Run(async () => await GetCarsAsync());

                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    IsErrorVisible = true;
                    ErrorMessage = "Usuario o contraseña incorrectos";
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al iniciar sesión: " + e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GetCarsAsync()
        {
            try
            {
                var carsFromServer = await _apiService.GetAsync<List<CarModel>>("api/Cars", TimeSpan.FromSeconds(30));
                if (carsFromServer == null)
                {
                    throw new NullReferenceException("No fue posible obtener vehículos desde el servidor");
                }

                await Task.Run(() =>
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        foreach (var item in carsFromServer)
                        {
                            if (!db.Cars.Any(c => c.Plate == item.Plate))
                            {
                                Cars.Add(item);
                                db.Cars.Add(item);
                            }
                        }
                        db.SaveChanges();
                    }
                });
            }
            catch (NullReferenceException ef)
            {
                await MessageUtilities.ShowLongToast("Advertencia. " + ef.Message);
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al obtener vehículos: " + e.Message);
            }
        }

        private async Task ValidateAndAssignServerUrl()
        {
            try
            {
                if (!Uri.TryCreate(Server, UriKind.Absolute, out Uri serverUri))
                {
                    var parts = Server.Split(':');
                    if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out _))
                        throw new ArgumentException("URL del servidor no válida.");
                }

                StaticData.URL = Server;
                StaticData.Port = "";
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowLongToast("URL del servidor no válida. Se asignarán valores por defecto.");

                StaticData.URL = "localhost";
                StaticData.Port = "8080";
            }
        }

        private async Task LoadTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("token");
                if (!string.IsNullOrEmpty(token) && await IsTokenValid(token))
                {
                    Application.Current.MainPage = new AppShell();
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Sesión expirada, vuelva a iniciar sesión";
                    IsErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowLongToast("Error al cargar token: " + ex.Message);  
            }
        }

        private async Task<bool> IsTokenValid(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken;

                return jwtToken != null && jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowLongToast("Token inválido: " + ex.Message);
                return false;
            }
        }

        public bool EstaConectado() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        #endregion

        #region Helper Methods

        private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            try
            {
                if (!EqualityComparer<T>.Default.Equals(backingStore, value))
                {
                    backingStore = value;
                    OnPropertyChanged(propertyName);
                }
            }
            catch (Exception f)
            {
                MessageUtilities.ShowLongToast("Error al establecer propiedad: " + f.Message);
            }
        }

        #endregion

        #region Nested Classes

        public class DataSignIn
        {
            public string UserName { get; set; }
            public string password { get; set; }
        }

        #endregion
    }
}
