using iText.Forms.Fields.Merging;
using Newtonsoft.Json;
using System.Text;

namespace CheckCars.Services
{
    public class APIService
    {
        #region Properties
        public string Token { get; set; }
        private readonly HttpClient _httpClient;
        #endregion

        #region Constructor
        public APIService(TimeSpan? timeout = null)
        {
            try
            {
                 CheckCars.Data.StaticData.URL.ToString().TrimEnd('/');

                if (string.IsNullOrEmpty(CheckCars.Data.StaticData.Port))
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri($"{CheckCars.Data.StaticData.URL}"),
                        Timeout = timeout ?? TimeSpan.FromSeconds(100) // Configuración predeterminada de tiempo de espera
                    };
                }
                else
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri($"{CheckCars.Data.StaticData.URL}:{CheckCars.Data.StaticData.Port}/"),
                        Timeout = timeout ?? TimeSpan.FromSeconds(100) // Configuración predeterminada de tiempo de espera
                    };
                }
            }
            catch (Exception we)
            {
                throw;
            }
        }
        #endregion

        #region Methods   

        public async Task<T?> GetAsync<T>(string endpoint, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                Token = await GetJwtTokenAsync();

                // Añadir el token al encabezado Authorization si existe
                if (!string.IsNullOrEmpty(Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }

                // Log para inspeccionar el endpoint
                var S = $"Making GET request to: {_httpClient.BaseAddress}{endpoint}";

                var response = await _httpClient.GetAsync(endpoint, cts?.Token ?? CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else if( response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Application.Current.MainPage.DisplayAlert("Información","No posee autorización","Ok");
                }
                return default;
            }
            catch (Exception e)
            {
                // Puedes registrar el error también para depuración
                Console.WriteLine($"Exception occurred: {e.Message}");
                return default;
            }
        }
 
        public async Task<(bool sucess, string response)> PostAsync<T>(string endpoint, T data )
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var json = JsonConvert.SerializeObject(data);

                Token = await GetJwtTokenAsync();

                // Añadir el token al encabezado Authorization si existe
                if (!string.IsNullOrEmpty(Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cts?.Token ?? CancellationToken.None);
                var responseContent = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.RequestTimeout:
                        Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                        break;

                    case System.Net.HttpStatusCode.Conflict:
                        Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                        break;
                    default:
                        break;
                }
                return (response.IsSuccessStatusCode, responseContent);
            }
            catch (Exception r)
            {
                return (false, r.Message);
            }
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                var json = JsonConvert.SerializeObject(data);

                Token = await GetJwtTokenAsync();

                // Añadir el token al encabezado Authorization si existe
                if (!string.IsNullOrEmpty(Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cts?.Token ?? CancellationToken.None);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.RequestTimeout:
                        Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                        break;

                    case System.Net.HttpStatusCode.Conflict:
                        Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                        break;
                    default:
                        break;
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception r)
            {
                return false;
            }
        }
    
        public async Task<bool> PostAsync<T>(string endpoint, T data, List<string> files = null, TimeSpan? timeout = null)
        {
            try
            {
                if (!await CheckConnectionAsync())
                {
                    Application.Current.MainPage.DisplayAlert("Información", "El dispositivo no posee internet", "ok");
                    return false;
                }

                Token = await GetJwtTokenAsync();

                // Añadir el token al encabezado Authorization si existe
                if (!string.IsNullOrEmpty(Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }

                var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;

                using (MultipartFormDataContent Form = new())
                {
                    // Proccess the images
                    foreach (var file in files)
                    {
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(file));
                        var extension = Path.GetExtension(file).TrimStart('.');
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue($"image/{extension}"); // Construye correctamente el tipo MIME
                        Form.Add(fileContent, "file", Path.GetFileName(file));
                    }

                    // Convert the Objetc to JSON
                    JsonSerializerSettings? options = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    string? JsonObj = JsonConvert.SerializeObject(data, options);
                    var jsonContent = new StringContent(JsonObj, Encoding.UTF8, "application/json");
                    var type = data.GetType().ToString().Split('.').LastOrDefault();

                    Form.Add(jsonContent, type);
                    // Send the request
                    HttpResponseMessage? response = await _httpClient.PostAsync(endpoint, Form, cts?.Token ?? CancellationToken.None);
                    ShowStatusAsync(response);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception w)
            {
                return false;
            }
        }

        #endregion

        #region Internal Methods

        private async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var currenT = Connectivity.Current;
                var networkAccess = currenT.NetworkAccess;
                return networkAccess == NetworkAccess.Internet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        private async Task ShowStatusAsync(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.RequestTimeout:
                    Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                    break;

                default:

                    break;
            }
        }

        public async Task<string> GetJwtTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync("token");
            }
            catch (Exception ex)
            {
                // Manejar errores, por ejemplo, si no se encuentra el token.
                Console.WriteLine($"Error obteniendo el token: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
