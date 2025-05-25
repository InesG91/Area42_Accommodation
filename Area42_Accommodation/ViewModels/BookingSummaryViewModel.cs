namespace Area42_Accommodation.ViewModels
{
    public class BookingSummaryViewModel
    {
        public string AccommodationType { get; set; } = string.Empty;  // "Hotel" or "Chalet"
        public string RoomType { get; set; } = string.Empty;           // "Small Hotel Room", "Medium Chalet", etc.
        public string CheckInDate { get; set; } = string.Empty;        // "2024-12-25"
        public string CheckOutDate { get; set; } = string.Empty;       // "2024-12-27"
        public int TotalGuests { get; set; } = 1;                      // Number of people
        public decimal PricePerNight { get; set; }                     // €150.00
        public int NumberOfNights { get; set; }                        // 2 nights
        public decimal TotalAmount { get; set; }                       // €300.00
    }
}
