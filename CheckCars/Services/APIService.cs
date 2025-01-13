using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Networking;

namespace CheckCars.Services
{
    public class APIService
    {
        public string Token { get; set; }
        private readonly HttpClient _httpClient;

        public APIService(TimeSpan? timeout = null)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"{CheckCars.Data.StaticData.URL}:{CheckCars.Data.StaticData.Port}/"),
                Timeout = timeout ?? TimeSpan.FromSeconds(100) // Configuración predeterminada de tiempo de espera
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                var response = await _httpClient.GetAsync(endpoint, cts?.Token ?? CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }



        public async Task<bool> PostAsync<T>(string endpoint, T data, TimeSpan? timeout = null)
        {
            try
            {
                using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cts?.Token ?? CancellationToken.None);
                response.StatusCode = System.Net.HttpStatusCode.Conflict;
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
                if (!await CheckConnection())
                {
                    Application.Current.MainPage.DisplayAlert("Información", "El dispositivo no posee internet", "ok");
                    return false;
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

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception r)
            {
                return false;
            }
        }

        private async Task<bool> CheckConnection()
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
    }

}
