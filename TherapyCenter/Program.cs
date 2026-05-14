using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TherapyCenter.Data;
using TherapyCenter.Helpers;
using TherapyCenter.Middleware;
using TherapyCenter.Repositories.Implementations;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Services.PaymentService;
using TherapyCenter.Services.PaymentService.Repo;
using TherapyCenter.Seeding;

namespace TherapyCenter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var razorpaySettings = builder.Configuration
                .GetSection("Razorpay")
                .Get<RazorpaySettings>() ?? throw new Exception("Razorpay settings missing");

            builder.Services.AddSingleton(razorpaySettings);


            var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
            builder.Services.AddSingleton(emailSettings);
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<PaymentService>();



            var jwtSettings = builder.Configuration
                .GetSection("Jwt")
                .Get<JwtSettings>() ?? throw new Exception("JWT settings missing");

            builder.Services.AddSingleton(jwtSettings);

            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173") 
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });


            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IJwtHelper, JwtHelper>();

            builder.Services.AddScoped<ITherapyRepository, TherapyRepository>();
            builder.Services.AddScoped<ITherapyService, TherapyService>();

            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();

            builder.Services.AddScoped<ISlotRepository, SlotRepository>();
            builder.Services.AddScoped<ISlotService, SlotService>();
            builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();

            builder.Services.AddScoped<IDoctorFindingService, DoctorFindingService>();
            builder.Services.AddScoped<IDoctorFindingRepository, DoctorFindingRepository>();

            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IPatientService, PatientService>();

            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<IReportService, ReportService>();

            var redisConfiguration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfiguration;
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            field = e.Key,
                            message = e.Value!.Errors.First().ErrorMessage
                        })
                        .ToList();

                    return new BadRequestObjectResult(new
                    {
                        success = false,
                        message = "Validation failed.",
                        statusCode = 400,
                        errors
                    });
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddOpenApi();

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();
            builder.Host.UseSerilog();

            var app = builder.Build();

            await AdminSeeder.EnsureDefaultAdminAsync(app.Services);
            await DemoDataSeeder.SeedIfNeededAsync(app.Services);

            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");


            app.UseMiddleware<LoggingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}