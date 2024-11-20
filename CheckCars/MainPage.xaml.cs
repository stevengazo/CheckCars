using CheckCars.ViewModels;
using Microsoft.Maui.ApplicationModel;

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
                Console.WriteLine("Permiso de almacenamiento denegado.");
            }

            var MANAGE_EXTERNAL_STORAGE = await Permissions.RequestAsync<Permissions.Media>();
            if (MANAGE_EXTERNAL_STORAGE != PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine("Permiso de Media denegado.");
            }

            var photos = await Permissions.RequestAsync<Permissions.Photos>();
            if (photos!= PermissionStatus.Granted)
            {
                // Mostrar mensaje si el permiso no fue concedido
                Console.WriteLine("Permiso de Photos denegado.");
            }



        }


    }

}
