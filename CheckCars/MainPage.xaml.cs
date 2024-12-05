using CheckCars.Data;
using CheckCars.Models;
using CheckCars.ViewModels;

namespace CheckCars
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(MainPageVM vm)
        {
            InitializeComponent();
            BindingContext = vm;
            RequestPermissions();

            LoadData();
        }

        private void LoadData()
        {
            StaticData.User = new UserProfile();

            StaticData.User.UserName = Preferences.Get(nameof(UserProfile.UserName), "Default User");
            StaticData.User.DNI = int.Parse(Preferences.Get(nameof(UserProfile.DNI), "00000000"));
        }

        private async void RequestPermissions()
        {
            // Solicitar permiso para la cámara
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine("Permiso de cámara denegado.");
            }

            // Solicitar permiso para la ubicación en uso
            var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine("Permiso de ubicación denegado.");
            }

            var READ_EXTERNAL_STORAGE = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (READ_EXTERNAL_STORAGE != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine($"Permiso de {nameof(Permissions.StorageRead)} denegado.");
            }

            var write_EXTERNAL_STORAGE = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (write_EXTERNAL_STORAGE != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine($"Permiso de {nameof(Permissions.StorageWrite)} denegado.");
            }

            var MANAGE_EXTERNAL_STORAGE = await Permissions.RequestAsync<Permissions.Media>();
            if (MANAGE_EXTERNAL_STORAGE != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine($"Permiso de {nameof(Permissions.Media)} denegado.");
            }

            var photos = await Permissions.RequestAsync<Permissions.Photos>();
            if (photos != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine($"Permiso de {nameof(Permissions.Media)} denegado.");
            }
        }
    }
}
