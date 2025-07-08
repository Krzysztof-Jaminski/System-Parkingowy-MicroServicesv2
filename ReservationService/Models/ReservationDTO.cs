namespace ReservationService.Models
{
    public class ReservationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? ParkingSpot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? PromotionCode { get; set; }
        public decimal Cost { get; set; }
    }
} 