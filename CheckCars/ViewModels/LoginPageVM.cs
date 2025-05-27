using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Windows.Input;
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using CommunityToolkit.Maui;
using Newtonsoft.Json;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing the login process, including user credentials,
    /// server connection, token validation, and navigation.
    /// </summary>
    public class LoginPageVM : INotifyPropertyChangedAbst
    {
        #region Properties

        private readonly APIService _apiService = new APIService();

        private string _UserName;
        private string _Password;
        private string _Server;
        private string _ErrorMessage;
        private bool _IsBusy = false;
        private bool _IsErrorVisible = false;

        /// <summary>
        /// Gets or sets the username for login.
        /// </summary>
        public string UserName
        {
            get => _UserName;
            set
            {
                if (_UserName != value)
                {
                    _UserName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        /// <summary>
        /// Gets or sets the password for login.
        /// </summary>
        public string Password
        {
            get => _Password;
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        /// <summary>
        /// Gets or sets the server URL to which the application connects.
        /// </summary>
        public string Server
        {
            get => _Server;
            set
            {
                if (_Server != value)
                {
                    _Server = value;
                    OnPropertyChanged(nameof(Server));
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message shown in the UI if login fails.
        /// </summary>
        public string ErrorMessage
        {
            get => _ErrorMessage;
            set
            {
                if (_ErrorMessage != value)
                {
                    _ErrorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a login operation is in progress.
        /// </summary>
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the error message is visible in the UI.
        /// </summary>
        public bool IsErrorVisible
        {
            get => _IsErrorVisible;
            set
            {
                if (_IsErrorVisible != value)
                {
                    _IsErrorVisible = value;
                    OnPropertyChanged(nameof(IsErrorVisible));
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageVM"/> class.
        /// Sets up commands and attempts to load an existing token.
        /// </summary>
        public LoginPageVM()
        {
            try
            {
                Login = new Command(async () => await SignInAsync());
                LoadToken();
                if (!string.IsNullOrEmpty(StaticData.URL))
                {
                    Server = StaticData.URL;
                }
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "Aceptar");
                Console.WriteLine(d.Message);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command executed to attempt user login.
        /// </summary>
        public ICommand Login { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to sign in the user asynchronously using the provided credentials.
        /// Handles connection, token retrieval, and navigation on success.
        /// </summary>
        /// <returns>A task representing the asynchronous login operation.</returns>
        public async Task SignInAsync()
        {
            try
            {
                await _apiService.UpdateUrl(Server);
                var isConnected = EstaConectado();
                if (!isConnected)
                {
                    throw new HttpRequestException("No hay conexión a internet");
                }

                IsBusy = true;
                var data = new DataSignIn { email = UserName, password = Password };
                await ValidateAndAssignServerUrl();

                (bool sucess, string response) respon = await _apiService.PostAsync<DataSignIn>("api/Account/login", data);

                // Deserialize the response to a dynamic object
                dynamic jSonData = JsonConvert.DeserializeObject(respon.response);

                // Access the "token" property directly
                string token = jSonData.token;

                if (respon.sucess)
                {
                    IsErrorVisible = false;
                    SecureStorage.Remove("token");

                    StaticData.User = new UserProfile();
                    Preferences.Set(nameof(UserProfile.UserName), UserName);
                    StaticData.User.UserName = UserName;
                    await SecureStorage.SetAsync("token", token);
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
                Console.WriteLine(e.Message);
                Application.Current.MainPage.DisplayAlert("Error", e.Message, "Aceptar");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Validates the server URL and assigns it to static variables.
        /// Handles URLs with ports or IP addresses.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ValidateAndAssignServerUrl()
        {
            string url = string.Empty;
            int port = 0;

            try
            {
                Uri serverUri;
                if (Uri.TryCreate(Server, UriKind.Absolute, out serverUri))
                {
                    url = serverUri.Host;
                }
                else
                {
                    var parts = Server.Split(':');
                    if (parts.Length == 2 && IPAddress.TryParse(parts[0], out IPAddress ip))
                    {
                        url = parts[0];
                        port = int.Parse(parts[1]);
                    }
                    else
                    {
                        throw new ArgumentException("URL del servidor no válida.");
                    }
                }
                CheckCars.Data.StaticData.URL = Server;
                CheckCars.Data.StaticData.Port = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar la URL del servidor: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "URL del servidor no válida. Se asignarán valores por defecto.", "Aceptar");
                CheckCars.Data.StaticData.URL = "localhost";
                CheckCars.Data.StaticData.Port = 8080.ToString();
            }
        }

        /// <summary>
        /// Loads the token from secure storage and navigates to the main page if valid.
        /// Displays error message if the token is expired or invalid.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync("token");

                if (!string.IsNullOrEmpty(token))
                {
                    var isTokenValid = IsTokenValid(token);
                    if (isTokenValid)
                    {
                        Application.Current.MainPage = new AppShell();
                    }
                    else
                    {
                        ErrorMessage = "Sesión expirada, vuelva a iniciar sesión";
                        IsErrorVisible = true;
                    }
                }
            }
            catch (Exception ec)
            {
                Application.Current.MainPage.DisplayAlert("Info", ec.Message, "ok");
                Console.WriteLine(ec.Message + ec.InnerException);
            }
        }

        /// <summary>
        /// Validates the JWT token expiration.
        /// </summary>
        /// <param name="token">JWT token string.</param>
        /// <returns>True if token is valid (not expired), false otherwise.</returns>
        private bool IsTokenValid(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    var expiration = jwtToken.ValidTo;
                    return expiration > DateTime.UtcNow;
                }
            }
            catch (Exception ed)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Token inválido +" + ed.Message, "Aceptar");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Checks if the device has an active internet connection.
        /// </summary>
        /// <returns>True if connected to the internet; otherwise false.</returns>
        public bool EstaConectado()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }

        #endregion

        /// <summary>
        /// Data class representing the login credentials payload.
        /// </summary>
        public class DataSignIn
        {
            /// <summary>
            /// Gets or sets the email for login.
            /// </summary>
            public string email { get; set; }

            /// <summary>
            /// Gets or sets the password for login.
            /// </summary>
            public string password { get; set; }
        }
    }
}
