using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;
using Resto.API.Services;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EncryptionService _encryption;
    private readonly CardValidationService _cardValidation;

    public PaymentController(
        AppDbContext db,
        EncryptionService encryption,
        CardValidationService cardValidation)
    {
        _db = db;
        _encryption = encryption;
        _cardValidation = cardValidation;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _db.Payments
            .AsNoTracking()
            .ToListAsync();

        var result = payments.Select(x =>
            new PaymentResponse
            {
                Id = x.Id,
                OrderId = x.OrderId,
                CustomerName =
                    _encryption.Decrypt(
                        x.CustomerNameEnc),

                Method = x.Method,

                Amount = x.Amount,

                Status = x.Status,

                TransactionDate =
                    x.TransactionDate,

                TransactionReference =
                    string.IsNullOrWhiteSpace(
                        x.TransactionRefEnc)
                    ? null
                    : _encryption.Decrypt(
                        x.TransactionRefEnc)
            }).ToList();

        return Ok(
            ApiResponse<List<PaymentResponse>>.Ok(
                result));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request)
    {
        var order =
            await _db.Orders.FindAsync(request.OrderId);

        if (order == null)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    "Order not found."));
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Payment method is required."));
        }

        if (request.Method.Equals(
                "Card",
                StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(
                    request.CardNumber))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Card number is required."));
            }

            if (!_cardValidation.ValidateCardNumber(
                    request.CardNumber))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Invalid card number."));
            }
        }

        var payment = new Payment
        {
            OrderId = request.OrderId,

            CustomerNameEnc =
                _encryption.Encrypt(
                    request.CustomerName),

            Method = request.Method,

            Amount = request.Amount,

            Status = PaymentStatus.Paid,

            TransactionDate = DateTime.Now,

            TransactionRefEnc =
                string.IsNullOrWhiteSpace(
                    request.TransactionReference)
                ? null
                : _encryption.Encrypt(
                    request.TransactionReference)
        };

        _db.Payments.Add(payment);

        order.PaymentStatus =
            PaymentStatus.Paid;

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<int>.Ok(
                payment.Id,
                "Payment created successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPayment(int id)
    {
        var payment =
            await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payment == null)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    "Payment not found."));
        }

        var result = new PaymentResponse
        {
            Id = payment.Id,

            OrderId = payment.OrderId,

            CustomerName =
                _encryption.Decrypt(
                    payment.CustomerNameEnc),

            Method = payment.Method,

            Amount = payment.Amount,

            Status = payment.Status,

            TransactionDate =
                payment.TransactionDate,

            TransactionReference =
                string.IsNullOrWhiteSpace(
                    payment.TransactionRefEnc)
                ? null
                : _encryption.Decrypt(
                    payment.TransactionRefEnc)
        };

        return Ok(
            ApiResponse<PaymentResponse>.Ok(
                result));
    }
}