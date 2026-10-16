using Resto.App.Models;
using Resto.App.Services;

namespace Resto.App.Pages;

public partial class MenuPage : ContentPage
{
    private readonly AppDataService _data;
    private string _selectedCategory = "All";
    private string _search = string.Empty;

    public MenuPage(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildCategoryChips();
        ApplyFilter();
    }

    private void BuildCategoryChips()
    {
        FilterChips.Children.Clear();

        var allCategories = new[] { "All" }.Concat(_data.Categories).ToList();
        foreach (var cat in allCategories)
        {
            var isSelected = cat == _selectedCategory;
            var chip = new Border
            {
                BackgroundColor = isSelected ? Color.FromArgb("#b71c1c") : Colors.White,
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#b71c1c"),
                Padding = new Thickness(14, 8),
            };
            chip.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 };
            chip.Content = new Label
            {
                Text      = cat,
                FontSize  = 13,
                TextColor = isSelected ? Colors.White : Color.FromArgb("#b71c1c"),
                VerticalOptions = LayoutOptions.Center,
            };
            var catCopy = cat;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                _selectedCategory = catCopy;
                BuildCategoryChips();
                ApplyFilter();
            };
            chip.GestureRecognizers.Add(tap);
            FilterChips.Children.Add(chip);
        }
    }

    private void ApplyFilter()
    {
        var items = _data.Menu.AsEnumerable();
        if (_selectedCategory != "All")
            items = items.Where(m => m.Category == _selectedCategory);
        if (!string.IsNullOrWhiteSpace(_search))
            items = items.Where(m => m.Name.Contains(_search, StringComparison.OrdinalIgnoreCase));
        MenuList.ItemsSource = items.ToList();
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _search = e.NewTextValue ?? string.Empty;
        ApplyFilter();
    }

    private async void OnAddHalfClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FoodItem item)
        {
            if (item.PriceHalf <= 0)
            {
                await DisplayAlert("Not Available", "This item is not available in half portion.", "OK");
                return;
            }
            _data.AddToCart(item, "Half");
            await DisplayAlert("Added! 🛒", $"{item.Name} (Half) added to cart.", "OK");
        }
    }

    private async void OnAddFullClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FoodItem item)
        {
            _data.AddToCart(item, "Full");
            await DisplayAlert("Added! 🛒", $"{item.Name} (Full) added to cart.", "OK");
        }
    }
}
