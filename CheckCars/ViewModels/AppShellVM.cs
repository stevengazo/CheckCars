using CheckCars.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AppShellVM : INotifyPropertyChangedAbst
    {
        // Comando para cerrar sesión
        public ICommand CerrarSesionCommand { get; }

        public AppShellVM()
        {
            // Inicializa el comando
            CerrarSesionCommand = new Command(CerrarSesion);
        }

        // Lógica para cerrar sesión
        private async void CerrarSesion()
        {
            // Aquí puedes agregar la lógica para cerrar sesión.
            // Por ejemplo, limpiar datos de usuario.
            SecureStorage.Remove("token");

            // Cambiar la raíz de la aplicación para redirigir al LoginPage
            // Esto asegura que se navegue a la página de login y no haya retroceso al AppShell.
            Application.Current.MainPage = new NavigationPage(new LoginPage());

            // Si prefieres usar Shell, puedes hacer lo siguiente:
            // await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
