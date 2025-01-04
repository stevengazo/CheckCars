using Newtonsoft.Json;
using System.Text;

namespace CheckCars.Services
{
    public class APIService
    {
        public string Toker { get; set; }
        private readonly HttpClient _httpClient;
        public APIService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"{CheckCars.Data.StaticData.URL}:{CheckCars.Data.StaticData.Port}/")
            };
        }


        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                return default;
            }
            catch (Exception e)
            {
                return default;
            }
        }


        private async Task<bool> ComplexPost<T>(string endpoint, T data)
        {
            return false;

        }
        private async Task<bool> SimplePost<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                var m = response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode;
            }
            catch (Exception r)
            {
                return false;
            }
        }


        public async Task<bool> PostAsync<T>(string endpoint, T data, bool hasFiles = false)
        {
            if (hasFiles)
            {
                await ComplexPost<T>(endpoint, data);
            }
            else
            {
                await SimplePost<T>(endpoint, data);
            }
            return default;
        }


    }
}
