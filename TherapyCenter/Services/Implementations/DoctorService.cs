using TherapyCenter.DTOs.Doctor;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;

        public DoctorService(IDoctorRepository doctorRepository, IUserRepository userRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
        }

        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto)
        {

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already exists");


            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);


            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = "Doctor",
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.Now
            };

            var createdUser = await _userRepository.AddUserAsync(user);

            var doctor = new Doctor
            {
                UserId = createdUser.UserId,  
                Specialization = dto.Specialization,
                Bio = dto.Bio,
                AvailableDays = dto.AvailableDays,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            var createdDoctor = await _doctorRepository.AddAsync(doctor);

            return Map(createdDoctor, createdUser);
        }

        public async Task<List<DoctorResponseDto>> GetAllDoctorsAsync()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return doctors.Select(d => Map(d, d.User)).ToList();
        }

        public async Task<DoctorResponseDto> GetDoctorByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid doctor id.");

            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            return Map(doctor, doctor.User);
        }

        public async Task<DoctorResponseDto> UpdateDoctorAsync(int id, UpdateDoctorDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid doctor id.");

            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            var user = doctor.User;

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            doctor.Specialization = dto.Specialization;
            doctor.Bio = dto.Bio;
            doctor.AvailableDays = dto.AvailableDays;
            doctor.StartTime = dto.StartTime;
            doctor.EndTime = dto.EndTime;

            await _doctorRepository.UpdateAsync(doctor);

            return Map(doctor, user);
        }

        public async Task DeleteDoctorAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid doctor id.");

            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            await _doctorRepository.DeleteAsync(doctor);
        }

        private static DoctorResponseDto Map(Doctor doctor, User user)
        {
            return new DoctorResponseDto
            {
                DoctorId = doctor.DoctorId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Specialization = doctor.Specialization,
                Bio = doctor.Bio,
                AvailableDays = doctor.AvailableDays,
                StartTime = doctor.StartTime ?? default,
                EndTime = doctor.EndTime ?? default
            };
        }



        public async Task<List<DoctorListItemDto>> GetDoctorsForPatientAsync()
        {
            var doctors = await _doctorRepository.GetAllWithUserAsync();

            return doctors.Select(d => new DoctorListItemDto
            {
                DoctorId = d.DoctorId,
                FullName = $"{d.User.FirstName} {d.User.LastName}",
                Email = d.User.Email,
                Specialization = d.Specialization,
                AvailableDays = d.AvailableDays,
                StartTime = d.StartTime.HasValue
                ? d.StartTime.Value.ToString("HH:mm")
                : "-",
                EndTime = d.EndTime.HasValue
                ? d.EndTime.Value.ToString("HH:mm")
                : "-"
            }).ToList();
        }
    }
}