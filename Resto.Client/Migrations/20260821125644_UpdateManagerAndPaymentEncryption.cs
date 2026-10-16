using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resto.Client.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManagerAndPaymentEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ================================================================
            // MANAGERS
            // ================================================================

            migrationBuilder.DropIndex(
                name: "IX_Managers_Email",
                table: "Managers");

            // Do NOT drop old Password yet if you need existing data.
            // We will add PasswordHash first.
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // ================================================================
            // PAYMENTS - ENCRYPTED FIELDS
            // ================================================================

            // Add encrypted columns first.
            migrationBuilder.AddColumn<string>(
                name: "CustomerNameEnc",
                table: "Payments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionRefEnc",
                table: "Payments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            // IMPORTANT:
            // Do NOT drop CustomerName and TransactionRef here.
            //
            // Existing values must first be encrypted by the application.
            // After encryption has been confirmed, the old columns can be
            // removed in a future migration.

            // ================================================================
            // PAYMENTS STATUS
            // ================================================================

            // Existing database:
            // Status = "Paid"
            // Status = "Pending"
            // Status = "Failed"
            //
            // New model:
            // Paid    = 1
            // Pending = 0
            // Failed  = 2
            //
            // We cannot directly AlterColumn<string -> int>.
            // Therefore create temporary integer column.

            migrationBuilder.AddColumn<int>(
                name: "Status_New",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE Payments
                SET Status_New =
                    CASE
                        WHEN Status = 'Paid' THEN 1
                        WHEN Status = 'Pending' THEN 0
                        WHEN Status = 'Failed' THEN 2
                        ELSE 0
                    END
            """);

            // Remove old string Status.
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");

            // Rename Status_New -> Status.
            migrationBuilder.RenameColumn(
                name: "Status_New",
                table: "Payments",
                newName: "Status");

            // ================================================================
            // PAYMENTS METHOD
            // ================================================================

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // ================================================================
            // PRODUCTS
            // ================================================================

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceHalf",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceFull",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "Products",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // ================================================================
            // ORDERS
            // ================================================================

            // ================================================================
            // ORDERS STATUS
            // ================================================================

            // Old database:
            // Pending
            // Confirmed
            // Preparing
            // ReadyForPickup
            // Completed
            // Cancelled
            //
            // New enum:
            // Pending        = 0
            // Confirmed      = 1
            // Preparing      = 2
            // ReadyForPickup = 3
            // Completed      = 4
            // Cancelled      = 5

            migrationBuilder.AddColumn<int>(
                name: "Status_New",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
    UPDATE Orders
    SET Status_New =
        CASE
            WHEN Status = 'Pending' THEN 0
            WHEN Status = 'Confirmed' THEN 1
            WHEN Status = 'Preparing' THEN 2
            WHEN Status = 'ReadyForPickup' THEN 3
            WHEN Status = 'Completed' THEN 4
            WHEN Status = 'Cancelled' THEN 5
            ELSE 0
        END
""");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Status_New",
                table: "Orders",
                newName: "Status");

            // ================================================================
            // ORDERS PAYMENT STATUS
            // ================================================================

            // Old database:
            // Pending
            // Paid
            // Failed
            //
            // New enum:
            // Pending = 0
            // Paid    = 1
            // Failed  = 2

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus_New",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
    UPDATE Orders
    SET PaymentStatus_New =
        CASE
            WHEN PaymentStatus = 'Pending' THEN 0
            WHEN PaymentStatus = 'Paid' THEN 1
            WHEN PaymentStatus = 'Failed' THEN 2
            ELSE 0
        END
""");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus_New",
                table: "Orders",
                newName: "PaymentStatus");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // ================================================================
            // ORDER ITEMS
            // ================================================================

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Portion",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // ================================================================
            // NOTIFICATIONS
            // ================================================================

            // ================================================================
            // NOTIFICATIONS TYPE
            // ================================================================

            // Old database values:
            // NewOrder
            // PaymentConfirmed
            // LowStock
            // OrderReady
            // General
            //
            // New NotificationType enum:
            // NewOrder         = 0
            // PaymentConfirmed = 1
            // LowStock         = 2
            // OrderReady       = 3
            // General          = 4

            migrationBuilder.AddColumn<int>(
                name: "Type_New",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
    UPDATE Notifications
    SET Type_New =
        CASE
            WHEN Type = 'NewOrder' THEN 0
            WHEN Type = 'PaymentConfirmed' THEN 1
            WHEN Type = 'LowStock' THEN 2
            WHEN Type = 'OrderReady' THEN 3
            WHEN Type = 'General' THEN 4
            ELSE 4
        END
""");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Type_New",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            // ================================================================
            // MANAGER OTHER FIELDS
            // ================================================================

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            // ================================================================
            // INVENTORY
            // ================================================================

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Inventory",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Inventory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "LowStockThreshold",
                table: "Inventory",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Inventory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove new encrypted columns.
            migrationBuilder.DropColumn(
                name: "CustomerNameEnc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionRefEnc",
                table: "Payments");

            // Remove password hash.
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Managers");

            // Restore Password.
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Managers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Restore payment status as string.
            migrationBuilder.AddColumn<string>(
                name: "Status_Old",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql("""
                UPDATE Payments
                SET Status_Old =
                    CASE
                        WHEN Status = 1 THEN 'Paid'
                        WHEN Status = 2 THEN 'Failed'
                        ELSE 'Pending'
                    END
            """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Status_Old",
                table: "Payments",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            // Restore unique email index.
            migrationBuilder.CreateIndex(
                name: "IX_Managers_Email",
                table: "Managers",
                column: "Email",
                unique: true);
        }
    }
}