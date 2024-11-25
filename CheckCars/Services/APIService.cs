using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace vehiculosmecsa.Services
{
    public class APIService
    {
        public string Toker {  get; set; }
        private readonly HttpClient _httpClient;
        public APIService()
        {
             _httpClient = new HttpClient();
        }
        private void ConfigurarEncabezados()
        {
            if(!string.IsNullOrEmpty(Toker))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Toker}");
            }
        }
        // Metodo generico para envio de datos,
        public async Task<bool> PostJsonAsync<T>(string url, T data, List<byte[]> images = null)
        {
            try
            {
                ConfigurarEncabezados();

                using (var formData = new MultipartFormDataContent())
                {
                    // Agregar el contenido JSON
                    string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    formData.Add(jsonContent, "data");

                    // Agregar las imágenes si las hay
                    if (images != null && images.Any())
                    {
                        for (int i = 0; i < images.Count; i++)
                        {
                            var imageContent = new ByteArrayContent(images[i]);
                            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                            formData.Add(imageContent, $"images[{i}]", $"image{i}.jpg");
                        }
                    }

                    // Enviar la solicitud
                    var response = await _httpClient.PostAsync(url, formData);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en POST: {e.Message}");
                return false;
            }
        }


        public async Task<T> GetTAsync<T>(string url)
        {
            try
            {
                ConfigurarEncabezados();
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(json)) 
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
                return default;
            }
            catch (Exception ef)
            {
                Console.WriteLine($"Error en GET: {ef.Message}");
                return default;
            }
        }
    }
}
