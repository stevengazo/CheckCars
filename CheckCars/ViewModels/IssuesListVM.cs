using System;
using System.Collections.Generic;
using System.ComponentModel;
using CheckCars.Views;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Models;
using System.Collections.ObjectModel;
using CheckCars.Data;

namespace CheckCars.ViewModels
{
    public class IssuesListVM : INotifyPropertyChangedAbst
    {

        #region General
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public IssuesListVM()
        {

        }
        public ICommand AddIssueReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddIssuesReport()));
        public ICommand ViewIssue { get; }
        public ICommand UpdateIssues => new Command(() => LoadData());
        private ObservableCollection<IssueReport> _Issues = new();
        public ObservableCollection<IssueReport> Issues
        {
            get { return _Issues; }
            set
            {
                _Issues = value;
                if (_Issues != null)
                {
                    OnPropertyChanged(nameof(Issues));
                }
            }
        }
        public async Task LoadData()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Issues.Clear();
                    var data = db.IssueReports.ToList();
                    foreach (var entry in data)
                    {
                        Issues.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                // Puedes registrar o manejar la excepción aquí si es necesario
                Console.WriteLine(e.Message);
            }
        }

    }
}
