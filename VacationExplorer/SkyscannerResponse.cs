namespace VacationExplorer
{
    public class SkyscannerResponse
    {
        public string SessionKey { get; set; }
        public Query Query { get; set; }
        public string Status { get; set; }
        public Itinerary[] Itineraries { get; set; }
        public Leg[] Legs { get; set; }
        public Carrier[] Carriers { get; set; }
        public Agent[] Agents { get; set; }
        public TripPlace[] Places { get; set; }
    }
}
