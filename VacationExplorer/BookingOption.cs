namespace VacationExplorer
{
    public class BookingOption
    {
        public BookingItem[] BookingItems { get; set; }
    }

    public class BookingItem
    {
        public int AgentID { get; set; }
        public string Status { get; set; }
        public double Price { get; set; } 
        public string Deeplink { get; set; }
    }
}