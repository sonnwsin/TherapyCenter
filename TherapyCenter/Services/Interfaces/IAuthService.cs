using TherapyCenter.DTOs.Auth;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);

        Task ForgotPasswordForGuardianAsync(ForgotPasswordRequestDto dto);
        Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task ResetPasswordForGuardianAsync(ResetPasswordRequestDto dto);
    }
}
