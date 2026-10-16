using Resto.App.Models;
using Resto.App.Services;

namespace Resto.App.Pages;

public partial class HomePage : ContentPage
{
    private readonly AppDataService _data;

    public HomePage(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void LoadData()
    {
        CategoryChips.Children.Clear();
        foreach (var cat in _data.Categories)
        {
            var chip = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#b71c1c"),
                Padding = new Thickness(14, 8),
            };
            chip.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 };
            chip.Content = new Label
            {
                Text      = cat,
                FontSize  = 13,
                TextColor = Color.FromArgb("#b71c1c"),
                VerticalOptions = LayoutOptions.Center,
            };
            var catCopy = cat;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => Shell.Current.GoToAsync("//MenuPage");
            chip.GestureRecognizers.Add(tap);
            CategoryChips.Children.Add(chip);
        }

        FeaturedList.ItemsSource = _data.Menu
            .Where(m => m.IsAvailable)
            .Take(6)
            .ToList();
    }

    private void OnBrowseMenuClicked(object sender, EventArgs e)
        => Shell.Current.GoToAsync("//MenuPage");

    private void OnQuickAddClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FoodItem item)
        {
            _data.AddToCart(item, "Full");
            Shell.Current.DisplayAlert("Added to Cart",
                $"{item.Name} (Full) added to your cart.", "OK");
        }
    }
}
