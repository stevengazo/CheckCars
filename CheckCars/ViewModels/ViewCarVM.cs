using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.ViewModels
{
    public class ViewCarVM : INotifyPropertyChangedAbst
    {
        private readonly APIService _apiService = new();

        public ViewCarVM() 
        {

    
            var id = StaticData.CarId;
            using (var db =  new ReportsDBContextSQLite ())
            {
                Vehicle = db.Cars.FirstOrDefault(e => e.CarId == id);
            }

            RequestExists();


        }

       

        private CarModel _Vehicle;

        public CarModel Vehicle
        {
            get { return _Vehicle; }
            set
            {
                if (_Vehicle != value)
                {
                    _Vehicle = value;
                    OnPropertyChanged(nameof(Vehicle));
                }
            }
        }

        private ObservableCollection<EntryExitReport> _ExistsReports = new() ;

        public ObservableCollection<EntryExitReport> ExistsReports
        {
            get { return _ExistsReports; }
            set
            {
                if (_ExistsReports != value)
                {
                    _ExistsReports = value;
                    OnPropertyChanged(nameof(ExistsReports));
                }
            }
        }
     


        private async void RequestExists()
        {

            try
            {
                ExistsReports.Clear();

                // Today
                var info = await _apiService.GetAsync<List<EntryExitReport>>($"api/EntryExitReports/search?date={DateTime.Today.ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if(info != null)
                {
                    foreach (var i in info)
                    {
                        ExistsReports.Add(i);
                    }
                }

                // Yesterday
                var dV = await _apiService.GetAsync<List<EntryExitReport>>($"api/EntryExitReports/search?date={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
               if(dV != null)
                {
                    foreach(var i in dV){
                        ExistsReports.Add(i);
                    }
                }                
            }
            catch (Exception ef)
            {

                throw;
            }
        }


    }
}
