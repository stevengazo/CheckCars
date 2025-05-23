using System;
using System.Collections.Generic;
using Plugin.Maui.Calendar.Models;
using CheckCars.Models;
using System.ComponentModel;
using System.Collections;
using CheckCars.Views;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class BookingVM : INotifyPropertyChangedAbst
    {

        #region Properties
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

        private EventCollection Bookings()
        {
            var dates = GetSampleBookings().Select(e => e.Startdate).ToList();

            var eventos = new EventCollection();

            var bookingsByDate = GetSampleBookings()
                .GroupBy(b => b.Startdate.Date);

            foreach (var group in bookingsByDate)
            {
                eventos.Add(group.Key, group.ToList());
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
                Events = Bookings();
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
