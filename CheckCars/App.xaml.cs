using vehiculosmecsa.Views;

namespace vehiculosmecsa
{
    public partial class App : Application
    {
       
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

           // MainPage =   new LoginPage();
        }
    }
}
