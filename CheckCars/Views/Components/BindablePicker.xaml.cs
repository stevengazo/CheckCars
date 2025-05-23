using System.Collections;
using System.Reflection;

namespace CheckCars.Views.Components;

public partial class BindablePicker : ContentView
{
    public BindablePicker()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(BindablePicker), propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty DisplayMemberPathProperty =
        BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(BindablePicker));

    public static readonly BindableProperty SelectedValuePathProperty =
        BindableProperty.Create(nameof(SelectedValuePath), typeof(string), typeof(BindablePicker));

    public static readonly BindableProperty SelectedValueProperty =
        BindableProperty.Create(nameof(SelectedValue), typeof(object), typeof(BindablePicker), defaultBindingMode: BindingMode.TwoWay);

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        if (PickerControl.SelectedIndex == -1 || ItemsSource == null) return;

        var selectedItem = PickerControl.ItemsSource.Cast<object>().ElementAt(PickerControl.SelectedIndex);
        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            var value = selectedItem?.GetType().GetProperty(SelectedValuePath)?.GetValue(selectedItem);
            SelectedValue = value;
        }
        else
        {
            SelectedValue = selectedItem;
        }
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BindablePicker)bindable;
        if (newValue is IEnumerable items)
        {
            control.PickerControl.ItemsSource = items.Cast<object>()
                .Select(item => item?.GetType().GetProperty(control.DisplayMemberPath)?.GetValue(item)?.ToString())
                .ToList();
        }
    }
}
