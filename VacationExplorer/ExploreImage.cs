namespace VacationExplorer
{
    public class ExploreImage
    {
        public string name { get; set; }
        public string source { get; set; }
        public string query { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }

        public ExploreImage (string source, string name, string latitude, string longitude)
        {
            this.source = source;
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public ExploreImage(string source, string name, string query)
        {
            this.source = source;
            this.name = name;
            this.query = query;
        }
    }
}
