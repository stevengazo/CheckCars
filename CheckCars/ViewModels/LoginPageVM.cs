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
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (_UserName != value)
                {
                    _UserName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }
        public string Password
        {
            get { return _Password; }
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string Server
        {
            get { return _Server; }
            set
            {
                if (_Server != value)
                {
                    _Server = value;
                    OnPropertyChanged(nameof(Server));
                }
            }
        }
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (_ErrorMessage != value)
                {
                    _ErrorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }
        public bool IsErrorVisible
        {
            get { return _IsErrorVisible; }
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

        public LoginPageVM()
        {
            Login = new Command(async () => await SignInAsync());
LoadToken();
            if (!string.IsNullOrEmpty(StaticData.URL))
            {
                Server = StaticData.URL;
            }
        }

        #region Commands
        public ICommand Login { get; set; }
        #endregion

        #region Methods
        public async Task SignInAsync()
        {
            try
            {
                var isConected = EstaConectado();
                if (!isConected)
                {
                    throw new HttpRequestException("No hay conexión a internet");
                }


                IsBusy = true;
                var data = new DataSignIn { email = UserName, password = Password };
                ValidateAndAssignServerUrl();

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
                    //    Application.Current.MainPage.DisplayAlert("Error", "Usuario o contraseña incorrectos", "Aceptar");  
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

        private async Task ValidateAndAssignServerUrl()
        {
            // Asignar valores por defecto
            string url = string.Empty;
            int port = 0;

            try
            {
                // Si la URL contiene un puerto, se separa y valida
                Uri serverUri;
                if (Uri.TryCreate(Server, UriKind.Absolute, out serverUri))
                {
                    // Asignar URL y puerto de la URI
                    url = serverUri.Host; // Esto toma la parte del dominio o IP


                }
                else
                {
                    // Si no es una URL válida, asumir que es una IP local con puerto
                    var parts = Server.Split(':');
                    if (parts.Length == 2 && IPAddress.TryParse(parts[0], out IPAddress ip))
                    {
                        url = parts[0]; // IP
                        port = int.Parse(parts[1]); // Puerto
                    }
                    else
                    {
                        throw new ArgumentException("URL del servidor no válida.");
                    }
                }
                // Asignar los valores a las variables globales o estáticas
                CheckCars.Data.StaticData.URL = Server;
                CheckCars.Data.StaticData.Port = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar la URL del servidor: {ex.Message}");
                // Manejo de errores o valores por defecto si es necesario
                CheckCars.Data.StaticData.URL = "localhost";
                CheckCars.Data.StaticData.Port = 8080.ToString();
            }
        }

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
                throw;
            }
        }

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
            catch (Exception)
            {
                // Si ocurre un error al procesar el token, lo tratamos como inválido
                return false;
            }

            return false;
        }

        public bool EstaConectado()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }
        public class DataSignIn
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        #endregion
    }
}
