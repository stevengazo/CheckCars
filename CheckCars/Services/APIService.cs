using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Linq;

namespace CheckCars.Services
{
    public class APIService
    {
        #region Properties

        /// <summary>
        /// Gets or sets the current JWT authentication token.
        /// </summary>
        public string Token { get; set; }

        private HttpClient _httpClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="APIService"/> class.
        /// Sets the base URL and timeout for HTTP requests.
        /// </summary>
        /// <param name="timeout">Optional timeout for HTTP requests. Defaults to 100 seconds.</param>
        public APIService(TimeSpan? timeout = null)
        {
            try
            {
                var baseUrl = CheckCars.Data.StaticData.URL?.TrimEnd('/') ?? "";

                if (string.IsNullOrEmpty(CheckCars.Data.StaticData.Port))
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri($"{baseUrl}"),
                        Timeout = timeout ?? TimeSpan.FromSeconds(100)
                    };
                }
                else
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri($"{baseUrl}:{CheckCars.Data.StaticData.Port}/"),
                        Timeout = timeout ?? TimeSpan.FromSeconds(100)
                    };
                }
            }
            catch (Exception we)
            {
                // Log exception as needed
            }
        }

        #endregion

        #region Methods  

        /// <summary>
        /// Updates the base URL of the HTTP client.
        /// </summary>
        /// <param name="url">The new base URL.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateUrl(string url)
        {
            try
            {
                _httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(url),
                    Timeout = TimeSpan.FromSeconds(100)
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Performs a GET request to the specified endpoint and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
        /// <param name="endpoint">The API endpoint.</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <returns>The deserialized response object, or default if the request failed.</returns>
        public async Task<T?> GetAsync<T>(string endpoint, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                Token = await GetJwtTokenAsync();

                if (!string.IsNullOrEmpty(Token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                var response = await _httpClient.GetAsync(endpoint, cts?.Token ?? CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "No posee autorización", "Ok");
                }
                return default;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// Performs a POST request to the specified endpoint sending JSON data, returning status and response content.
        /// </summary>
        /// <typeparam name="T">The type of the data to send.</typeparam>
        /// <param name="endpoint">The API endpoint.</param>
        /// <param name="data">The data object to serialize and send.</param>
        /// <returns>A tuple indicating success and the response content.</returns>
        public async Task<(bool success, string response)> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var json = JsonConvert.SerializeObject(data);
                Token = await GetJwtTokenAsync();

                if (!string.IsNullOrEmpty(Token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.RequestTimeout:
                        await Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                        break;
                    case HttpStatusCode.Conflict:
                        await Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                        break;
                    case HttpStatusCode.Unauthorized:
                        await Application.Current.MainPage.DisplayAlert("Error", "No posee autorización", "Ok");
                        break;
                    case HttpStatusCode.BadRequest:
                        await Application.Current.MainPage.DisplayAlert("Error", $"Error en la solicitud: {responseContent}", "Ok");
                        break;
                }
                return (response.IsSuccessStatusCode, responseContent);
            }
            catch (Exception r)
            {
                return (false, r.Message);
            }
        }

        /// <summary>
        /// Performs a POST request to the specified endpoint sending JSON data.
        /// </summary>
        /// <typeparam name="T">The type of the data to send.</typeparam>
        /// <param name="endpoint">The API endpoint.</param>
        /// <param name="data">The data object to serialize and send.</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <returns>True if the request was successful; otherwise false.</returns>
        public async Task<bool> PostAsync<T>(string endpoint, T data, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                var json = JsonConvert.SerializeObject(data);
                Token = await GetJwtTokenAsync();

                if (!string.IsNullOrEmpty(Token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cts?.Token ?? CancellationToken.None);

                var responseBody = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.RequestTimeout:
                        await Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                        await Application.Current.MainPage.DisplayAlert("Respuesta", responseBody, "Ok");
                        break;
                    case HttpStatusCode.Conflict:
                        await Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                        await Application.Current.MainPage.DisplayAlert("Respuesta", responseBody, "Ok");
                        break;
                    case HttpStatusCode.Unauthorized:
                        await Application.Current.MainPage.DisplayAlert("Error", "No posee autorización", "Ok");
                        break;
                    case HttpStatusCode.BadRequest:
                        await Application.Current.MainPage.DisplayAlert("Error", $"Error en la solicitud: {responseBody}", "Ok");
                        break;
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception r)
            {
                await Application.Current.MainPage.DisplayAlert("Excepción", r.Message, "Ok");
                return false;
            }
        }

        /// <summary>
        /// Performs a POST request sending JSON data along with one or more files.
        /// </summary>
        /// <typeparam name="T">The type of the data to send.</typeparam>
        /// <param name="endpoint">The API endpoint.</param>
        /// <param name="data">The data object to serialize and send.</param>
        /// <param name="files">A list of file paths to include in the request.</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <returns>True if the request was successful; otherwise false.</returns>
        public async Task<bool> PostAsync<T>(string endpoint, T data, List<string> files = null, TimeSpan? timeout = null)
        {
            try
            {
                if (!await CheckConnectionAsync())
                {
                    await Application.Current.MainPage.DisplayAlert("Información", "El dispositivo no posee internet", "Ok");
                    return false;
                }

                Token = await GetJwtTokenAsync();

                if (!string.IsNullOrEmpty(Token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;

                using var form = new MultipartFormDataContent();

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var fileBytes = File.ReadAllBytes(file);
                        var fileContent = new ByteArrayContent(fileBytes);
                        var extension = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();

                        var mimeType = extension switch
                        {
                            "jpg" or "jpeg" => "image/jpeg",
                            "png" => "image/png",
                            "gif" => "image/gif",
                            _ => "application/octet-stream"
                        };

                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                        form.Add(fileContent, "file", Path.GetFileName(file));
                    }
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var jsonObj = JsonConvert.SerializeObject(data, jsonSettings);
                var jsonContent = new StringContent(jsonObj, Encoding.UTF8, "application/json");

                var typeName = data.GetType().Name;
                form.Add(jsonContent, typeName);

                var response = await _httpClient.PostAsync(endpoint, form, cts?.Token ?? CancellationToken.None);

                await ShowStatusAsync(response);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Checks if the device has an active internet connection.
        /// </summary>
        /// <returns>True if internet is available; otherwise false.</returns>
        private async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var networkAccess = Connectivity.Current.NetworkAccess;
                return networkAccess == NetworkAccess.Internet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Shows alerts based on the HTTP response status code.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowStatusAsync(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.RequestTimeout:
                    await Application.Current.MainPage.DisplayAlert("Error", "Tiempo de espera agotado", "Ok");
                    break;
                case HttpStatusCode.Conflict:
                    await Application.Current.MainPage.DisplayAlert("Error", "El reporte ya se encuentra en el servidor", "Ok");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Retrieves the JWT token stored securely.
        /// </summary>
        /// <returns>The JWT token string if available; otherwise null.</returns>
        public async Task<string> GetJwtTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync("token");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo el token: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
