using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CheckCar.Utilities
{
    public static class MessageUtilities
    {
        public const string TitleError = "Error";
        public const string TitleInfo = "Información";
        public const string TitleWarning = "Advertencia";

        public static async Task ShowInfoMessageAsync(string title, string message)
        {
            await App.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        public static async Task ShowToast(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Short, 14);
            await toast.Show();
        }

        public static async Task ShowLongToast(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 16);
            await toast.Show();
        }
    }
}
