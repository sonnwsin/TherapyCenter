namespace TherapyCenter.DTOs.Therapy
{
    /// <summary>
    /// Lightweight read model for therapy pricing (used for list + Redis cache payload).
    /// </summary>
    public class TherapyPriceDto
    {
        public int Id { get; set; }
        public string TherapyName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
