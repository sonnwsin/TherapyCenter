using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTOs.Auth;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid credentials.",
                    statusCode = 401
                });
            }

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _authService.ForgotPasswordForGuardianAsync(request);
            return Ok(new
            {
                success = true,
                message = "A verification code has been sent to your email. It expires in 5 minutes."
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            var result = await _authService.VerifyOtpAsync(request);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    status = result.Status,
                    statusCode = 400
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                status = result.Status
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            await _authService.ResetPasswordForGuardianAsync(request);
            return Ok(new
            {
                success = true,
                message = "Password has been reset successfully. You can sign in with your new password."
            });
        }
    }
}
