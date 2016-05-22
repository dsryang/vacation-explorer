namespace VacationExplorer
{
    public class SkyscannerBookingResponse
    {
        public Segment[] Segments { get; set; }
        public BookingOption[] BookingOptions { get; set; }
        public TripPlace[] Places { get; set; }
        public Carrier[] Carriers { get; set; }
    }
}