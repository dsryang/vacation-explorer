namespace VacationExplorer
{
    public class Itinerary
    {
        public string OutboundLegId { get; set; }
        public string InboundLegId { get; set; }
        public PricingOption[] PricingOptions { get; set; }
        public BookingLink BookingDetailsLink { get; set; }
    }

    public class BookingLink
    {
        public string Uri { get; set; }
        public string Body { get; set; }
    }

    public class PricingOption
    {
        public double Price { get; set; }
    }
}