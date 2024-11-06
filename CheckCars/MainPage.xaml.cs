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
        }

       
    }

}
