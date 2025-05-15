using CheckCars.Data;
using CheckCars.Models;
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
        public ViewCarVM() 
        {
            var id = StaticData.CarId;
            using (var db =  new ReportsDBContextSQLite ())
            {
                Vehicle = db.Cars.FirstOrDefault(e => e.CarId == id);
            }
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

        private ObservableCollection<EntryExitReport> _ExistsReports;

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

    }
}
