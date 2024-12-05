using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class CrashListVM : INotifyPropertyChangedAbst
    {

        #region Properties
        private ObservableCollection<CrashReport> _crashReports = new();
        public ObservableCollection<CrashReport> CrashReports
        {
            get
            {
                return _crashReports;
            }
            set
            {
                _crashReports = value;
                if (_crashReports != null)
                {
                    OnPropertyChanged(nameof(CrashReports));
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddCrashReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddCrash()));
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewCrash(), true);
            }
        });
        public ICommand Update => new Command(() => LoadDataAsync());
        #endregion
        #region Methods
        public async Task LoadDataAsync()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    CrashReports.Clear();
                    var data = db.CrashReports.ToList();
                    foreach (var entry in data)
                    {
                        CrashReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                // Puedes registrar o manejar la excepción aquí si es necesario
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
