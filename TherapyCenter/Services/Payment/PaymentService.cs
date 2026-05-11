using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Razorpay.Api;
using TherapyCenter.DTOs.Payment;
using TherapyCenter.Exceptions;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using PaymentModel = TherapyCenter.Models.Payment;
using TherapyCenter.Services.PaymentService.Repo;

namespace TherapyCenter.Services.PaymentService
{
    public class PaymentService
    {
        private readonly RazorpaySettings _settings;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITherapyRepository _therapyRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            RazorpaySettings settings,
            IAppointmentRepository appointmentRepository,
            ITherapyRepository therapyRepository,
            IPaymentRepository paymentRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings;
            _appointmentRepository = appointmentRepository;
            _therapyRepository = therapyRepository;
            _paymentRepository = paymentRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> CreateOrderAsync(CreateOrderDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found");

            EnsureCurrentUserCanPayForAppointment(appointment);

            var therapy = await _therapyRepository.GetByIdAsync(appointment.TherapyId);

            if (therapy == null)
                throw new Exception("Therapy not found");

            var amount = therapy.Cost;

            var existingPayment = await _paymentRepository.GetByAppointmentIdAsync(dto.AppointmentId);

            if (existingPayment != null)
            {
                if (existingPayment.Status == "Paid")
                    throw new Exception("Already paid for this appointment");

                if (existingPayment.Status == "Pending")
                    throw new Exception("Payment already initiated");
            }

            RazorpayClient client = new RazorpayClient(_settings.Key, _settings.Secret);

            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },
                { "currency", "INR" },
                { "receipt", $"receipt_{dto.AppointmentId}" }
            };

            Order order = client.Order.Create(options);

            var payment = new PaymentModel
            {
                AppointmentId = dto.AppointmentId,
                Amount = amount,
                Status = "Pending",
                TransactionId = order["id"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            return new
            {
                orderId = order["id"].ToString(),
                amount,
                currency = "INR"
            };
        }

        public async Task VerifyPaymentAsync(VerifyPaymentDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found");

            EnsureCurrentUserCanPayForAppointment(appointment);

            var payment = await _paymentRepository.GetByAppointmentIdAsync(dto.AppointmentId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Payment already completed.");

            if (!string.Equals(payment.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Payment cannot be verified in the current state.");

            var storedOrderId = payment.TransactionId?.Trim() ?? string.Empty;
            var dtoOrderId = dto.RazorpayOrderId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(storedOrderId) ||
                !string.Equals(storedOrderId, dtoOrderId, StringComparison.Ordinal))
                throw new Exception("Order id does not match the pending payment for this appointment.");

            var dtoPaymentId = dto.RazorpayPaymentId?.Trim() ?? string.Empty;
            var dtoSignature = dto.RazorpaySignature?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(dtoPaymentId) || string.IsNullOrEmpty(dtoSignature))
                throw new Exception("Payment id and signature are required.");

            if (!VerifySignature(dtoOrderId, dtoPaymentId, dtoSignature))
                throw new Exception("Invalid payment signature");

            payment.Status = "Paid";
            payment.PaymentMethod = "Razorpay";
            payment.TransactionId = dto.RazorpayPaymentId;
            payment.PaidAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
        }

        private void EnsureCurrentUserCanPayForAppointment(Appointment appointment)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Authentication required.");

            if (user.IsInRole("Admin"))
                return;

            if (user.IsInRole("Guardian"))
            {
                var guardianId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (appointment.Patient?.GuardianId != guardianId)
                    throw new ForbiddenException("You are not allowed to pay for this appointment.");

                return;
            }

            throw new ForbiddenException("You are not allowed to perform this payment operation.");
        }

        private bool VerifySignature(string orderId, string paymentId, string signature)
        {
            if (string.IsNullOrWhiteSpace(orderId) ||
                string.IsNullOrWhiteSpace(paymentId) ||
                string.IsNullOrWhiteSpace(signature))
                return false;

            var payload = $"{orderId}|{paymentId}";
            var secret = _settings.Secret;

            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);

            var generatedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(generatedSignature),
                Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()));
        }
    }
}
