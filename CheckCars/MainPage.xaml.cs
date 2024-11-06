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

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
