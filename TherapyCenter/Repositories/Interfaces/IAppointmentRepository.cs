using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment> AddAsync(Appointment appointment);
        Task<List<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(Appointment appointment);



        Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date);
    }
}