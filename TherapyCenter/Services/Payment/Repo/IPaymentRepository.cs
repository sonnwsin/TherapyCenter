using TherapyCenter.Models;

namespace TherapyCenter.Services.PaymentService.Repo
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId);

        Task<Payment> AddAsync(Payment payment);

        Task UpdateAsync(Payment payment);
    }
}