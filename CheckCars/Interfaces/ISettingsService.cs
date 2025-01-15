namespace CheckCars.Interfaces;

public interface ISettingsService
{

    bool UseAPI { get; set; }
    string APIURL { get; set; }
    string APIPort { get; set; }

}
