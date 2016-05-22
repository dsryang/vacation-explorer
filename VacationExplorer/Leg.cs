namespace VacationExplorer
{
    public class Leg
    {
        public string Id { get; set; }
        public int[] SegmentIds { get; set; }
        public int OriginStation { get; set; }
        public int DestinationStation { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public int Duration { get; set; }
        public int[] Stops { get; set; }
        public int[] Carriers { get; set; }
        public int[] OperatingCarriers { get; set; }
        public string Directionality { get; set; }
    }
}