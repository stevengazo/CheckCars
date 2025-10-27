using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReviCar.Data;
using ReviCar.Models;
using ReviCar.Utilities;
using ReviCar.Views;

namespace ReviCar.ViewModels
{
    /// <summary>
    /// ViewModel for managing the vehicle returns page.
    /// </summary>
    public class ReturnsPageVM : INotifyPropertyChangedAbst
    {
        public ObservableCollection<VehicleReturn> Returns { get; set; } = new();

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand ViewAddReturn { get; }
        public ICommand ViewReport { get; }

        public ReturnsPageVM()
        {
            try
            {
                LoadDataCommand = new Command(async () =>
                {
                    IsRefreshing = true;
                    await LoadDataAsync();
                    IsRefreshing = false;
                });

                ViewAddReturn = new Command(async () =>
                    await Application.Current.MainPage.Navigation.PushAsync(new AddReturn(), true));

                ViewReport = new Command(async (e) =>
                {
                    if (e is string reportId)
                    {
                        Data.StaticData.ReportId = reportId;
                        await Application.Current.MainPage.Navigation.PushAsync(new ViewReturn(), true);
                    }
                });

                _ = LoadDataAsync(); // carga inicial

            }
            catch (Exception c)
            {
               MessageUtilities.ShowInfoMessageAsync("Error", $"No se pudo inicializar la vista de devoluciones. {c.Message}").Wait();
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    using var db = new ReportsDBContextSQLite();
                    var d = db.Returns.OrderByDescending(e => e.Created).ToList();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Returns.Clear();
                        foreach (var v in d)
                            Returns.Add(v);
                    });
                });
            }
            catch
            {
                await MessageUtilities.ShowInfoMessageAsync("Error", "No se pudo cargar la información de devoluciones.");
            }
        }
    }

}
