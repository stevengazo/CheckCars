using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Views;

namespace CheckCars.ViewModels
{
    public class ReturnsPageVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        public ReturnsPageVM()
        {

        }

        #endregion

        #region Commands
        public ICommand ViewAddReturn { get; } = new Command(async () => 
        await Application.Current.MainPage.Navigation.PushAsync(new AddReturn(), true));

        #endregion

        #region Properties

        #endregion

        #region Methods

        #endregion

    }
}
