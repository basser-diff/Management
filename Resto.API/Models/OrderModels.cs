namespace Resto.API.Models;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    ReadyForPickup = 3,
    Completed = 4,
    Cancelled = 5
}

public class OrderResponse
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class UpdateOrderPaymentStatusRequest
{
    public PaymentStatus PaymentStatus { get; set; }
}