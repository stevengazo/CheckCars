using ReviCar.Views;

namespace ReviCar
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

           //MainPage = new AppShell();

             MainPage =   new LoginPage();
        }
    }
}
