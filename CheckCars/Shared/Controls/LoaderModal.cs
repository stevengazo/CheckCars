using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Shared.Controls
{
    /// <summary>
    /// Represents a modal loader control that displays a loading indicator and an optional message.
    /// </summary>
    public class LoaderModal : ContentView
    {
        /// <summary>
        /// Backing bindable property for <see cref="IsVisible"/>.
        /// Indicates whether the loader modal is visible.
        /// </summary>
        public static readonly BindableProperty IsVisibleProperty =
              BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(LoaderModal), false, propertyChanged: OnIsVisibleChanged);

        /// <summary>
        /// Backing bindable property for <see cref="Message"/>.
        /// Represents the message displayed inside the loader modal.
        /// </summary>
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(LoaderModal), string.Empty);

        /// <summary>
        /// Gets or sets a value indicating whether the loader modal is visible.
        /// </summary>
        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the message shown inside the loader modal.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoaderModal"/> class,
        /// defining the visual structure of the modal loader.
        /// </summary>
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

        /// <summary>
        /// Called when the <see cref="IsVisible"/> property changes.
        /// Updates the visibility of the modal content accordingly.
        /// </summary>
        /// <param name="bindable">The bindable object where the property changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LoaderModal modal)
            {
                modal.Content.IsVisible = (bool)newValue;
            }
        }
    }
}
