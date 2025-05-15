using CheckCars.Models;

namespace CheckCars.Data;


public static class StaticData
{
    private const string ReportIdKey = "ReportId";
    private const string UserProfileKey = "UserProfile";
    private const string URLKey = "URL";
    private const string PortKey = "Port";
    private const string UseAPIKey = "UseAPI";
    private const string _CarId = "";
    
    // Métodos para CarId
    public static string CarId
    {
        get => Preferences.Get(_CarId, string.Empty);
        set => Preferences.Set(_CarId, value);
    }

    // Métodos para ReportId
    public static string ReportId
    {
        get => Preferences.Get(ReportIdKey, string.Empty); // Valor predeterminado: ""
        set => Preferences.Set(ReportIdKey, value);
    }

    // Métodos para URL
    public static string URL
    {
        get => Preferences.Get(URLKey, ""); // Valor predeterminado
        set => Preferences.Set(URLKey, value);
    }

    // Métodos para Port
    public static string Port
    {
        get => Preferences.Get(PortKey, ""); // Valor predeterminado
        set => Preferences.Set(PortKey, value);
    }

    // Métodos para UseAPI
    public static bool UseA
    {
        get => Preferences.Get(UseAPIKey, true); // Valor predeterminado: true
        set => Preferences.Set(UseAPIKey, value);
    }

    // Guardar y recuperar objetos complejos (como UserProfile)
    public static UserProfile User
    {
        get
        {
            var json = Preferences.Get(UserProfileKey, string.Empty);
            return string.IsNullOrEmpty(json) ? null : System.Text.Json.JsonSerializer.Deserialize<UserProfile>(json);
        }
        set
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            Preferences.Set(UserProfileKey, json);
        }
    }
}
