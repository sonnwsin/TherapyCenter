using TherapyCenter.DTOs.Appointment;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto);

        Task<AppointmentResponseDto> CreateWalkInAsync(CreateWalkInAppointmentDto dto);



        Task<List<AppointmentResponseDto>> GetAllAsync();
        Task<AppointmentResponseDto> GetByIdAsync(int id);
        Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto);
        Task DeleteAsync(int id);

        Task CompleteAsync(int id);
        Task CancelAsync(int id);


        Task<List<AppointmentResponseDto>> GetByDoctorAsync(int doctorId);



        Task<AppointmentResponseDto> BookOnlineAsync(BookOnlineAppointmentDto dto);

        Task<List<AppointmentResponseDto>> GetAppointmentsForGuardianAsync();
    }
}