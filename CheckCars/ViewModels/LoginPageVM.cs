using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class LoginPageVM : INotifyPropertyChangedAbst
    {
        public ICommand Login { get; } = new Command(async () =>
        {
            Application.Current.MainPage = new AppShell();
        });
    }
}
