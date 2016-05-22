namespace VacationExplorer
{
    public class Response
    {
        public string headerLocation { get; set; }
        public string headerFullLocation { get; set; }
        public string headerLocationGranularity { get; set; }
        public int totalResults { get; set; }
        public Group[] groups { get; set; }
        public Geocode geocode { get; set; }
        public Venue venue { get; set; }
    }
}