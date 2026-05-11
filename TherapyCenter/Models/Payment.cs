using System;

namespace TherapyCenter.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }


        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}