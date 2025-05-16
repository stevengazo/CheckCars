using System;
using System.Collections.Generic;
using Plugin.Maui.Calendar.Models;
using CheckCars.Models;
using System.ComponentModel;
using System.Collections;

namespace CheckCars.ViewModels
{
    public class BookingVM : INotifyPropertyChangedAbst
    {
        public EventCollection Events { get; set; } = new EventCollection();


        private EventCollection Bookings()
        {
            var dates = GetSampleBookings().Select(e=>e.Startdate).ToList();

            var eventos = new EventCollection();    

            foreach (var date in dates)
            {
                eventos.Add(date, GetSampleBookings().Where(e=>e.Startdate.Date == date.Date).ToList()); 
            }


            return eventos;
        }

        public  List<Booking> GetSampleBookings()
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
                CarId = 101
            },
            new Booking
            {
                BookingId = 2,
                Startdate = DateTime.Now.AddDays(3),
                EndDate = DateTime.Now.AddDays(5),
                Reason = "Vacation",
                Status = "Pending",
                UserId = "user456",
                Deleted = false,
                CarId = 102
            },
            new Booking
            {
                BookingId = 3,
                Startdate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                Reason = "Conference",
                Status = "Cancelled",
                UserId = "user789",
                Deleted = true,
                CarId = 103,
            }
        };
        }


        public BookingVM()
        {

            /* Events = new EventCollection
             {
                 [DateTime.Now] = new List<EventModel>
                 {
                     new EventModel { Name = "Cool event1", Description = "This is Cool event1's description!" },
                     new EventModel { Name = "Cool event2", Description = "This is Cool event2's description!" }
                 },
                 // 5 days from today
                 [DateTime.Now.AddDays(5)] = new List<EventModel>
                 {
                     new EventModel { Name = "Cool event3", Description = "This is Cool event3's description!" },
                     new EventModel { Name = "Cool event4", Description = "This is Cool event4's description!" }
                 },
                 // 3 days ago
                 [DateTime.Now.AddDays(-3)] = new List<EventModel>
                 {
                     new EventModel { Name = "Cool event5", Description = "This is Cool event5's description!" }
                 },
                 // custom date
                 [new DateTime(2020, 3, 16)] = new List<EventModel>
                 {
                     new EventModel { Name = "Cool event6", Description = "This is Cool event6's description!" }
                 }
             };*/

            Events = Bookings();

        }
    }






    internal class EventModel
    {
        public string Name { get; set; }    
        public string Description { get; set; }
    }
}
