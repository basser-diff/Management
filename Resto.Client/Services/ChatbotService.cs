namespace Resto.Client.Services;

/// <summary>
/// Rule-based chatbot service.
/// Answers questions ONLY about the RESTO Management System.
/// No external API needed — all responses are local.
/// </summary>
public class ChatbotService
{
    // ── Knowledge base ─────────────────────────────────────────────────────────
    // Each entry: list of trigger keywords → response text

    private static readonly List<(string[] Keywords, string Response)> KnowledgeBase = new()
    {
        // ── System overview ────────────────────────────────────────────────────
        (new[] { "what", "system", "resto", "about", "overview" },
            "RESTO is a Hybrid Resto Ordering and Payment Management System. " +
            "It has two parts: a mobile app for customers and a web dashboard for the manager. " +
            "Customers can browse the menu, place orders, and pay online. " +
            "The manager can monitor orders, inventory, payments, and sales."),

        // ── Login ──────────────────────────────────────────────────────────────
        (new[] { "login", "log in", "password", "access", "credentials" },
            "To log in as a Manager, go to the Manager Login page and enter your email and password. " +
            "Customers log in through the mobile app using their registered email and password. " +
            "If you forgot your password, contact your system administrator."),

        // ── Registration ───────────────────────────────────────────────────────
        (new[] { "register", "sign up", "create account", "new account" },
            "Customers can register through the mobile app by providing their name, " +
            "email address, and password. Once registered, they can log in and start ordering."),

        // ── Menu ───────────────────────────────────────────────────────────────
        (new[] { "menu", "food", "items", "products", "browse" },
            "Customers can browse the full menu from the mobile app. " +
            "Items are organized by category. Each item shows its name, price, and available portion sizes (half/full). " +
            "The manager can add, edit, or delete menu items from the Products page in the dashboard."),

        // ── Order ──────────────────────────────────────────────────────────────
        (new[] { "order", "place order", "ordering", "cart", "checkout" },
            "To place an order: browse the menu → add items to cart → select portion size → " +
            "review cart → proceed to checkout → confirm payment. " +
            "After ordering, you can track your order status in the My Orders section of the mobile app."),

        // ── Cart ───────────────────────────────────────────────────────────────
        (new[] { "cart", "basket", "selected items" },
            "The Cart page shows all your selected food items including category, name, portion size, and price. " +
            "You can modify quantities or remove items before checking out. " +
            "Tap the Checkout button when you are ready to place your order."),

        // ── Payment ────────────────────────────────────────────────────────────
        (new[] { "payment", "pay", "quickpay", "credit card", "debit", "cash", "cop" },
            "RESTO supports two payment methods:\n" +
            "1. Credit/Debit Card — processed securely via QuickPay payment gateway.\n" +
            "2. Cash on Pick-Up (COP) — pay when you pick up your order.\n" +
            "Enter your card number, expiry date, security code, and cardholder name to complete a card payment."),

        // ── Order status ───────────────────────────────────────────────────────
        (new[] { "status", "track", "ready", "pickup", "pick up", "notification" },
            "You will receive a push notification on your mobile app when your order is:\n" +
            "• Confirmed by the manager\n" +
            "• Ready for pick-up\n" +
            "You can also check your order status anytime in the My Orders section of the app."),

        // ── My Orders ──────────────────────────────────────────────────────────
        (new[] { "my order", "past order", "order history", "completed order", "cancelled" },
            "The My Orders page shows all your orders — current, past, completed, and cancelled. " +
            "Each order shows the food category, menu name, portion, and status. " +
            "Completed orders are listed in the Complete Orders section with the amount paid."),

        // ── Dashboard ──────────────────────────────────────────────────────────
        (new[] { "dashboard", "overview", "sales", "profit", "summary" },
            "The Manager Dashboard shows:\n" +
            "• Total Sales — total revenue collected\n" +
            "• Total Expenses — operational costs\n" +
            "• Total Profit — sales minus expenses\n" +
            "• Recent Orders — latest customer orders with status\n" +
            "• Sales Chart — visual performance over time\n" +
            "All figures update in real-time as orders come in."),

        // ── Inventory ──────────────────────────────────────────────────────────
        (new[] { "inventory", "stock", "restock", "out of stock", "available" },
            "The Inventory page lets the manager monitor all food item stock levels. " +
            "Items show their status: In Stock, Add Stock, or Out of Stock. " +
            "Use the Add Items & Restock button to add new products or replenish stock. " +
            "The system automatically prevents customers from ordering items that are out of stock."),

        // ── Products ───────────────────────────────────────────────────────────
        (new[] { "product", "add product", "edit product", "delete product", "manage product" },
            "On the Products page, the manager can:\n" +
            "• Add new menu items (name, price, description, availability)\n" +
            "• Edit existing items\n" +
            "• Delete items from the menu\n" +
            "• Toggle item availability (show/hide from customer menu)"),

        // ── Manage orders (manager) ────────────────────────────────────────────
        (new[] { "confirm order", "process order", "manage order", "order table", "order id" },
            "The Orders page shows all incoming customer orders in a table format. " +
            "Each row displays: Order ID, Product Name, Category, Quantity, Portion, " +
            "Order Date, Confirmation Status, Payment Status, and Pick-Up status. " +
            "The manager can confirm orders and update their status from this page."),

        // ── Payments (manager) ─────────────────────────────────────────────────
        (new[] { "transaction", "payment record", "paid", "failed payment" },
            "The Payments page shows all customer payment transactions. " +
            "You can filter by: All, Paid, Failed, or Previous. " +
            "Each record shows: Transaction Number, Order Number, Customer Name, " +
            "Payment Method, Amount, and Status. Click View for full transaction details."),

        // ── Notifications (manager) ────────────────────────────────────────────
        (new[] { "notification sound", "alert sound", "chime", "volume", "notification settings" },
            "On the Notification Settings page, the manager can:\n" +
            "• Toggle notification sounds ON or OFF\n" +
            "• Select the notification sound type (e.g. chime)\n" +
            "• Adjust the notification volume with a slider\n" +
            "This ensures the manager is always alerted about new orders."),

        // ── Peak hours / order limit ───────────────────────────────────────────
        (new[] { "peak", "busy", "limit", "too many orders", "overload" },
            "RESTO has an order limit control feature. " +
            "When inventory is low or the kitchen is busy, the manager can limit the number of orders accepted. " +
            "This prevents over-ordering during peak hours and ensures quality service."),

        // ── Mobile app ────────────────────────────────────────────────────────
        (new[] { "mobile", "app", "android", "maui", "phone" },
            "The RESTO mobile app is built with .NET MAUI and runs on Android and iOS. " +
            "Customers use it to register, log in, browse the menu, place orders, " +
            "track order status, and receive push notifications."),

        // ── Web app ───────────────────────────────────────────────────────────
        (new[] { "web", "blazor", "browser", "web app", "desktop" },
            "The RESTO web application is built with Blazor Server and is used by the manager. " +
            "It is accessed through a web browser. " +
            "It provides the full management dashboard including products, orders, inventory, and payments."),

        // ── Technology ────────────────────────────────────────────────────────
        (new[] { "technology", "tech", "framework", "built with", "stack" },
            "RESTO is built using:\n" +
            "• Blazor Server — manager web dashboard\n" +
            "• .NET MAUI — customer mobile app (Android/iOS)\n" +
            "• ASP.NET Core — backend API\n" +
            "• Microsoft SQL Server — database\n" +
            "• Bootstrap — responsive web UI\n" +
            "• QuickPay — online payment processing\n" +
            "• Firebase Cloud Messaging — push notifications"),

        // ── Help / greeting ───────────────────────────────────────────────────
        (new[] { "hello", "hi", "hey", "good morning", "good afternoon", "help" },
            "Hello! 👋 I'm the RESTO Assistant. I can answer questions about the RESTO Management System.\n\n" +
            "You can ask me about:\n" +
            "• How to place an order\n" +
            "• Payment methods\n" +
            "• Managing inventory\n" +
            "• Dashboard and reports\n" +
            "• Products and menu management\n" +
            "• Order tracking and notifications\n\n" +
            "What would you like to know?"),

        // ── Goodbye ───────────────────────────────────────────────────────────
        (new[] { "bye", "goodbye", "thank", "thanks", "exit" },
            "You're welcome! If you have more questions about RESTO, feel free to ask anytime. 😊"),
    };

    // ── Public method ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a response based on keyword matching against the user's message.
    /// </summary>
    public string GetResponse(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Please type a question about the RESTO system.";

        var lower = userMessage.ToLowerInvariant();

        // Find the best matching rule (most keyword hits)
        var best = KnowledgeBase
            .Select(rule => (
                rule.Response,
                Hits: rule.Keywords.Count(k => lower.Contains(k))
            ))
            .Where(x => x.Hits > 0)
            .OrderByDescending(x => x.Hits)
            .FirstOrDefault();

        if (best.Hits > 0)
            return best.Response;

        // No match found
        return "I'm sorry, I can only answer questions about the RESTO Management System. " +
               "Try asking about: orders, menu, payment, inventory, dashboard, login, or notifications.";
    }
}
