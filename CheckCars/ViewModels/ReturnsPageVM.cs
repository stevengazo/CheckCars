using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;

namespace CheckCars.ViewModels
{
    public class ReturnsPageVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        public ReturnsPageVM()
        {
            loadData();
        }


        #endregion

        #region Commands
        public ICommand ViewAddReturn { get; } = new Command(async () => 
        await Application.Current.MainPage.Navigation.PushAsync(new AddReturn(), true));

        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewReturn(), true);
            }
        });

        public ICommand UpdateReports => new Command(() => LoadReports());
       
        #endregion

        #region Properties

        private ObservableCollection<VehicleReturn> _Returns=new();

        public ObservableCollection<VehicleReturn> Returns
        {
            get { return _Returns; }
            set
            {
                _Returns = value;
                if (_Returns != null)
                {
                    OnPropertyChanged(nameof(Returns));
                }

            }
        }

        #endregion

        #region Methods
        public async Task LoadReports()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Returns.Clear();
                    var data = db.Returns.OrderByDescending(e => e.Created).ToList();

                    foreach (var entry in data)
                    {
                        Returns.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                // Puedes registrar o manejar la excepción aquí si es necesario
                Console.WriteLine(e.Message);
            }
        }
        private void loadData()
        {
            try
            {
                using (var db =new  ReportsDBContextSQLite())
                {
                    
                    var d = db.Returns.OrderByDescending(e => e.Created).ToList();
                    if (d.Any())
                    {
                        if (d.Any())
                        {
                            Returns.Clear();
                            foreach (var v in d)
                            {
                                Returns.Add(v);
                            }
                        }

                    }

                }

            }
            catch (Exception we)
            {

                throw;
            }
        }
        #endregion

    }
}
