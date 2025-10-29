using CheckCar.Views;

namespace CheckCar
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
