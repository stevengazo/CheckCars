using CheckCar.Utilities;
using CheckCar.Views;
using System.ComponentModel;
using System.Windows.Input;


namespace CheckCar.ViewModels
{
 

    public class MainPageVM : INotifyPropertyChanged
    {
        #region Commands

        /// <summary>
        /// Command to navigate to the Entry/Exit report list page.
        /// </summary>
        public ICommand ViewEntryExitList { get; }

        /// <summary>
        /// Command to navigate to the crash reports list page.
        /// </summary>
        public ICommand CrashList { get; }

        /// <summary>
        /// Command to navigate to the issues list page.
        /// </summary>
        public ICommand IssuesList { get; }

        /// <summary>
        /// Command to navigate to the returns list page.
        /// </summary>
        public ICommand ReturnList { get; }


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageVM"/> class.
        /// </summary>
        public MainPageVM()
        {
            ViewEntryExitList = new Command(async () => await ViewEntryListReport());
            CrashList = new Command(async () => await ExecuteSafeNavigationAsync(new CrashList()));
            IssuesList = new Command(async () => await ExecuteSafeNavigationAsync(new IssuesList()));
            ReturnList = new Command(async () => await ExecuteSafeNavigationAsync(new ReturnsPage()));
          
        }

    

        #endregion

        #region Methods

        /// <summary>
        /// Handles navigation to the Entry/Exit report list page with error handling.
        /// </summary>
        private async Task ViewEntryListReport()
        {
            await ExecuteSafeNavigationAsync(new EntryExitReportList());
        }

        /// <summary>
        /// Helper method to safely navigate to a page, showing an error if something fails.
        /// </summary>
        private static async Task ExecuteSafeNavigationAsync(Page page)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(page, true);
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowInfoMessageAsync("Error", ex.Message);
            }
        }

        

        /// <summary>
        /// Triggers the PropertyChanged event for a given property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties



        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
