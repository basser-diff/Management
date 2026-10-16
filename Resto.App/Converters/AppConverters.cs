using System.Globalization;
using Resto.App.Models;

namespace Resto.App.Converters;

// ── InverseBoolConverter ──────────────────────────────────────────────────────
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

// ── ReadToBgConverter — white if unread, light grey if read ──────────────────
public class ReadToBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool isRead && isRead
            ? Color.FromArgb("#FFFFFF")
            : Color.FromArgb("#FFF8F8");
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}

// ── TypeToEmojiConverter ──────────────────────────────────────────────────────
public class TypeToEmojiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "OrderConfirmed" => "✅",
            "OrderReady"     => "🛍️",
            "PaymentConfirmed"=> "💳",
            "LowStock"       => "⚠️",
            _                => "🔔",
        };
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}

// ── TypeToIconBgConverter — background color for notification icon ────────────
public class TypeToIconBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "OrderConfirmed"  => Color.FromArgb("#E8F5E9"),
            "OrderReady"      => Color.FromArgb("#E0F7FA"),
            "PaymentConfirmed"=> Color.FromArgb("#E3F2FD"),
            "LowStock"        => Color.FromArgb("#FFF8E1"),
            _                 => Color.FromArgb("#F3E5F5"),
        };
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}

// ── ItemsSummaryConverter — summarises order items into a readable string ─────
public class ItemsSummaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not System.Collections.IList list || list.Count == 0)
            return "No items";
        var items = list.Cast<CartItem>().ToList();
        var parts = items.Take(3).Select(i => $"{i.Name} ({i.Portion}) x{i.Quantity}");
        var summary = string.Join(", ", parts);
        if (items.Count > 3) summary += $" +{items.Count - 3} more";
        return summary;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => null;
}
