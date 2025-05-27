using System;
using System.Collections.Generic;
using Plugin.Maui.Calendar.Models;
using CheckCars.Models;
using System.ComponentModel;
using System.Collections;
using CheckCars.Views;
using System.Windows.Input;
using CheckCars.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CheckCars.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckCars.ViewModels
{
    public class BookingVM : INotifyPropertyChangedAbst
    {

        #region Properties
        private readonly APIService aPIService = new APIService();
        private readonly ReportsDBContextSQLite _db = new();
        private EventCollection _events = new EventCollection();
        public EventCollection Events
        {
            get => _events;
            set
            {
                if (_events != value)
                {
                    _events = value;
                    OnPropertyChanged(nameof(Events));
                }
            }
        }

        private DateTime _SelectedDate;

        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set
            {
                if (_SelectedDate != value)
                {
                    _SelectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddBooking { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddBooking(), true));

        public ICommand UpdateReports => new Command(async () => await UpdateBookings());



        #endregion

        #region Methods
        private async Task UpdateBookings()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateBookings(DateTime d)
        {
            // Update the list of TmpBookings by the month and year
            throw new NotImplementedException();
        }

        private async Task SaveInDBAsync(List<Booking> bookings)
        {
            try
            {
                _db.Bookings.RemoveRange(_db.Bookings.ToList());
                if (bookings != null)
                {
                    var ExistingIds = _db.Bookings.Select(b => b.BookingId).ToHashSet();
                    var newBookings = bookings.Where(b => !ExistingIds.Contains(b.BookingId)).ToList();

                    if (newBookings.Count > 0)
                    {
                        _db.Bookings.AddRange(newBookings);
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task LoadDataAsync(List<Booking> bookings)
        {
            if (bookings != null)
            {
                var eventos = new EventCollection();

                foreach (var booking in bookings)
                {
                    // Recorremos todos los días desde Startdate hasta Enddate inclusive  
                    for (DateTime date = booking.Startdate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
                    {
                        if (!eventos.ContainsKey(date))
                            eventos[date] = new List<Booking>(); // Cambiado ICollection a List<Booking>  

                        ((List<Booking>)eventos[date]).Add(booking); // Realizamos un casting explícito a List<Booking>  
                    }
                }

                Events = eventos;
                await Application.Current.MainPage.DisplayAlert("Info", "Hay reservas", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No hay reservas", "OK");
            }
        }

        private async Task GetData()
        {
            var api = new APIService();
            var Reservas = await api.GetAsync<List<Booking>>("api/Bookings", TimeSpan.FromSeconds(55));
            await SaveInDBAsync(Reservas);
            List<Booking> TmpBookings = await _db.Bookings.Include(b => b.Car).ToListAsync();
            await LoadDataAsync(TmpBookings);
        }

        #endregion

        #region Constructor
        public BookingVM()
        {
            try
            {
                Task.Run(async () => await GetData());
            }
            catch (Exception gt)
            {
                Application.Current.MainPage.DisplayAlert("Error", gt.Message, "OK");
                throw;
            }
        }

        #endregion

    }

}
