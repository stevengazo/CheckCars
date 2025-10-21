using ReviCar.Utilities;
using ReviCar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReviCar.ViewModels
{
    public class AppShellVM : INotifyPropertyChangedAbst
    {
        // Comando para cerrar sesión
        public ICommand CerrarSesionCommand { get; }

        public AppShellVM()
        {
            try
            {
                // Inicializa el comando
                CerrarSesionCommand = new Command(CerrarSesion);
            }
            catch (Exception v)
            {
                MessageUtilities.ShowInfoMessage("AppShellVM", v.Message).Wait();
            }
        }

        // Lógica para cerrar sesión
        private async void CerrarSesion()
        {
            try
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
            catch (Exception f)
            {
                await MessageUtilities.ShowInfoMessage("Error", "Error: " + f.Message);
            }
        }
    }
}
