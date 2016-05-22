namespace VacationExplorer
{
    public class Venue
    {
        public string id { get; set; }
        public string name { get; set; }
        public Contact contact { get; set; }
        public Location location { get; set; }
        public Category[] categories { get; set; }
        public bool verified { get; set; }
        public double rating { get; set; }
        public string ratingColor { get; set; }
        public string canonicalUrl { get; set; }
        public string url { get; set; }
        public string description { get; set; }
    }
}