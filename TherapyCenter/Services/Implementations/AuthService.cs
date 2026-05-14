using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using TherapyCenter.DTOs.Auth;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const string GuardianRole = "Guardian";
        private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

        /// <summary>
        /// After a correct OTP, the OTP key is removed (one-time use). This key grants permission to call reset-password.
        /// </summary>
        private static readonly TimeSpan VerifiedResetWindowTtl = TimeSpan.FromMinutes(15);

        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;
        private readonly IDistributedCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtHelper jwtHelper,
            IDistributedCache cache,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("User already exists");

            var allowedRoles = new[] { "Receptionist", "Doctor", "Guardian" };

            if (!allowedRoles.Contains(request.Role))
                throw new Exception("Invalid role selection");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber
            };

            var createdUser = await _userRepository.AddUserAsync(user);
            var token = _jwtHelper.GenerateToken(createdUser);

            return new AuthResponseDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                Role = createdUser.Role,
                Token = token
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                return null;

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return null;
            var token = _jwtHelper.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }

        /// <inheritdoc />
        public async Task ForgotPasswordForGuardianAsync(ForgotPasswordRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _userRepository.GetByEmailIgnoreCaseAsync(email);

            if (user == null || !string.Equals(user.Role, GuardianRole, StringComparison.Ordinal))
            {
                _logger.LogWarning("Forgot password requested for unknown or non-guardian email {Email}", email);
                throw new ArgumentException("No guardian account found for this email.");
            }

            // New reset flow: drop any prior "OTP verified" window so the user must confirm the fresh OTP.
            await _cache.RemoveAsync(PwdResetVerifiedCacheKey(email));

            // 6-digit numeric OTP (100000–999999 inclusive).
            var otp = Random.Shared.Next(100000, 1000000).ToString();
            var otpKey = OtpCacheKey(email);

            // --- Redis OTP (cache-aside style): store only the OTP string; TTL enforces 5-minute expiry ---
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = OtpTtl
            };
            await _cache.SetStringAsync(otpKey, otp, cacheOptions);

            var body =
                $"Your Therapy Center password reset code is: {otp}\n\n" +
                "This code expires in 5 minutes. If you did not request a reset, ignore this email.";

            await _emailService.SendEmailAsync(user.Email, "Password reset verification code", body);
            _logger.LogInformation("Guardian password reset OTP stored in Redis for key {OtpKey} and email sent to {Email}", otpKey, user.Email);
        }

        /// <inheritdoc />
        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            var email = dto.Email.Trim();
            var otpKey = OtpCacheKey(email);

            var stored = await _cache.GetStringAsync(otpKey);
            if (string.IsNullOrEmpty(stored))
            {
                _logger.LogInformation("OTP verify failed (missing or expired) for key {OtpKey}", otpKey);
                return new VerifyOtpResponseDto
                {
                    Success = false,
                    Status = "expired",
                    Message = "OTP expired or not found. Request a new code from forgot password."
                };
            }

            var submitted = dto.Otp.Trim();
            if (!string.Equals(stored, submitted, StringComparison.Ordinal))
            {
                _logger.LogWarning("OTP verify failed (mismatch) for key {OtpKey}", otpKey);
                return new VerifyOtpResponseDto
                {
                    Success = false,
                    Status = "invalid",
                    Message = "Invalid OTP."
                };
            }

            // One-time OTP: remove immediately after a successful match so the same code cannot be reused.
            await _cache.RemoveAsync(otpKey);

            // Short-lived flag so reset-password can run without sending the OTP again in the body.
            var verifiedKey = PwdResetVerifiedCacheKey(email);
            await _cache.SetStringAsync(
                verifiedKey,
                "1",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = VerifiedResetWindowTtl });

            _logger.LogInformation("OTP verified for {Email}; Redis verified flag set at {VerifiedKey}", email, verifiedKey);
            return new VerifyOtpResponseDto
            {
                Success = true,
                Status = "valid",
                Message = "OTP verified. You can now set a new password."
            };
        }

        /// <inheritdoc />
        public async Task ResetPasswordForGuardianAsync(ResetPasswordRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _userRepository.GetByEmailIgnoreCaseAsync(email);

            if (user == null || !string.Equals(user.Role, GuardianRole, StringComparison.Ordinal))
                throw new ArgumentException("No guardian account found for this email.");

            var verifiedKey = PwdResetVerifiedCacheKey(email);
            var verified = await _cache.GetStringAsync(verifiedKey);
            if (string.IsNullOrEmpty(verified))
            {
                _logger.LogWarning("Password reset blocked: no verified OTP session for {Email}", email);
                throw new ArgumentException("Please verify your OTP first, or request a new code — your verification session expired.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            await _cache.RemoveAsync(verifiedKey);
            _logger.LogInformation("Guardian password updated for user {UserId}; Redis verified flag removed", user.UserId);
        }

        private static string NormalizeEmailForCache(string email) => email.Trim().ToLowerInvariant();

        /// <summary>Redis key for the raw OTP (e.g. <c>otp:user@example.com</c> after normalization).</summary>
        private static string OtpCacheKey(string email) => $"otp:{NormalizeEmailForCache(email)}";

        private static string PwdResetVerifiedCacheKey(string email) =>
            $"pwd_reset_verified:{NormalizeEmailForCache(email)}";
    }
}
