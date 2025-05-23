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

namespace CheckCars.ViewModels
{
    public class BookingVM : INotifyPropertyChangedAbst
    {

         #region Properties
        private readonly APIService aPIService = new APIService();
        private readonly ReportsDBContextSQLite _db = new();
        public EventCollection Events { get; set; } = new EventCollection();

        private DateTime _SelectedDate;

        public DateTime SelectedDat
        {
            get { return _SelectedDate; }
            set
            {
                if (_SelectedDate != value)
                {
                    _SelectedDate = value;
                    UpdateBookings(value);
                    OnPropertyChanged(nameof(SelectedDat));
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddBooking { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(  new AddBooking(),true ) );

        #endregion

        #region Methods

        private async Task<EventCollection> BookingsAsync()
        {
            var url = $"api/Bookings/Search?startDate={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}&endDate={DateTime.UtcNow.AddMonths(1):yyyy-MM-ddTHH:mm:ss}";

            var bookings = await aPIService.GetAsync<List<Booking>>(url, TimeSpan.FromSeconds(10));
            var eventos = new EventCollection();

            if (bookings != null)
            {
                var cars = _db.Cars.ToList();


                var grouped = bookings.GroupBy(b => b.Startdate.Date);
                foreach (var group in grouped)
                {
                    eventos.Add(group.Key, group.ToList());
                }
            }
            return eventos;
        }
        public List<Booking> GetSampleBookings()
        {
            return new List<Booking>
        {
            new Booking
            {
                BookingId = 1,
                Startdate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },          new Booking
            {
                BookingId = 2,
                Startdate = DateTime.Now.AddDays(3).AddHours(3),
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },            new Booking
            {
                BookingId = 3,
                Startdate = DateTime.Now.AddDays(3).AddHours(6),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },          new Booking
            {
                BookingId = 4,
                Startdate = DateTime.Now.AddDays(3).AddHours(2),
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },        new Booking
            {
                BookingId = 4,
                Startdate = DateTime.Now.AddDays(3).AddHours(2),
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },        new Booking
            {
                BookingId = 4,
                Startdate = DateTime.Now.AddDays(3).AddHours(2),
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Business Trip",
                Status = "Confirmed",
                UserId = "user123",
                Deleted = false,
                CarId = "101"
            },

        };
        }

        public async Task UpdateBookings(DateTime d)
        {
            // Update the list of bookings by the month and year
            throw new NotImplementedException();
        }

        #endregion

        #region Constructor
        public BookingVM()
        {
            try
            {
                Task.Run(async () => await Demo());
            }
            catch (Exception gt)
            {
                Application.Current.MainPage.DisplayAlert("Error", gt.Message, "OK");
                throw;
            }
        }

        private async Task Demo()
        {
            var api = new APIService();
            var Reservas = await api.GetAsync<List<Booking>>("api/Bookings", TimeSpan.FromSeconds(55));
            if(Reservas != null)
            {
                var eventos = new EventCollection();
                var grouped = Reservas.GroupBy(b => b.Startdate.Date);
                foreach (var group in grouped)
                {

                    eventos.Add(group.Key, group.ToList());
                }
                Events = eventos;

                Application.Current.MainPage.DisplayAlert("Info", " hay reservas", "OK");
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Error", "No hay reservas", "OK");
            }   
        }

        #endregion

    }
 
}
