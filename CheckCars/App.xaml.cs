using CheckCars.Views;

namespace CheckCars
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
