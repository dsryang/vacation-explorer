namespace VacationExplorer
{
    public class Segment
    {
        public int OriginStation { get; set; }
        public int DestinationStation { get; set; }
        public string DepartureDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public int OperatingCarrier { get; set; }
        public int Duration { get; set; }
        public string Directionality { get; set; }
    }
}