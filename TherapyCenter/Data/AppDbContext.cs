using Microsoft.EntityFrameworkCore;
using TherapyCenter.Models;

namespace TherapyCenter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Therapy> Therapies { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DoctorFinding> DoctorFindings { get; set; }
        public DbSet<Slot> Slots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<Doctor>(d => d.UserId);


            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Guardian)
                .WithMany(u => u.Patients)
                .HasForeignKey(p => p.GuardianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Receptionist)
                .WithMany(u => u.AppointmentsBooked)
                .HasForeignKey(a => a.ReceptionistId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Appointment)
                .WithOne(a => a.Payment)
                .HasForeignKey<Payment>(p => p.AppointmentId);


            modelBuilder.Entity<DoctorFinding>()
                .HasOne(df => df.Appointment)
                .WithOne(a => a.DoctorFinding)
                .HasForeignKey<DoctorFinding>(df => df.AppointmentId);
        }
    }
}