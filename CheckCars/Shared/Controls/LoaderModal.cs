using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Shared.Controls
{
    public class LoaderModal : ContentView
    {
        public static readonly BindableProperty IsVisibleProperty =
              BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(LoaderModal), false, propertyChanged: OnIsVisibleChanged);

        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(LoaderModal), string.Empty);

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public LoaderModal()
        {
            // Definir el diseño del modal
            Content = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                IsVisible = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Frame
                    {
                        Padding = 20,
                        CornerRadius = 15,
                        BackgroundColor = Colors.White,
                        HeightRequest = 150,
                        WidthRequest = 180,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Content = new VerticalStackLayout
                        {
                            Children =
                            {
                                new ActivityIndicator
                                {
                                    IsRunning = true,
                                    Color = Colors.Blue
                                },
                                new Label
                                {
                                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    TextColor = Colors.Black,
                                    Text = "Enviando información"
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LoaderModal modal)
            {
                modal.Content.IsVisible = (bool)newValue;
            }
        }
    }
}
