using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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
                       JsonSerializerSettings? options = new JsonSerializerSettings(){
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                         
                        };
                        string? JsonObj = JsonConvert.SerializeObject(data, options);
                        var jsonContent = new StringContent(JsonObj, Encoding.UTF8, "application/json");
                        Form.Add(jsonContent, "EntryExitReport");
                        // Send the request
                        HttpResponseMessage? response = await _httpClient.PostAsync(endpoint, Form, cts?.Token ?? CancellationToken.None);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Error al enviar la información. " + response.StatusCode);
                            return false;
                        }
                    }   
            }
            catch (Exception r)
            {
                return false;
            }
        }



    }

}
