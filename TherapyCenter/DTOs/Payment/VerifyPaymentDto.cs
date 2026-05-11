namespace TherapyCenter.DTOs.Payment
{
    public class VerifyPaymentDto
    {
        public int AppointmentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }
}
