using CheckCars.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class MainPageVM : INotifyPropertyChangedAbst
    {
        public ICommand ViewEntryExitList { get { return new Command(async () => await ViewEntryExits());  } private set { } }

        private async Task ViewEntryExits()
        {
            Application.Current.MainPage.Navigation.PushAsync( new EntryExitReportList() , true);
        }
    }
}
