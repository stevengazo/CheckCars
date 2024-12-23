using System;
using CheckCars.Interfaces;

namespace CheckCars.Services;

public sealed class SettingsService : ISettingsService
{
    public bool UseAPI
    {
        get => Preferences.Get(nameof(UseAPI), false);
        set => Preferences.Set(APIURL, value.ToString());
    }
    public string APIURL
    {
        get => Preferences.Get(nameof(APIURL), string.Empty);
        set => Preferences.Set(nameof(APIURL), value.ToString());
    }
    public string APIPort
    {
        get => Preferences.Get(nameof(APIPort), string.Empty);
        set => Preferences.Set(nameof(APIPort), value);
    }
}
