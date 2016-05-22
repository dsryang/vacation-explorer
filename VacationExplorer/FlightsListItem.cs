namespace VacationExplorer
{
    public class FlightsListItem
    {
        public string outboundId { get; set; }
        public string inboundId { get; set; }

        public string outboundAirlines { get; set; }
        public string inboundAirlines { get; set; }

        public string outboundAirports { get; set; }
        public string inboundAirports { get; set; }

        public string outboundFlightTime { get; set; }
        public string inboundFlightTime { get; set; }
        public string outboundAddDays { get; set; }
        public string inboundAddDays { get; set; }

        public string outboundDuration { get; set; }
        public string inboundDuration { get; set; }

        public string outboundStops { get; set; }
        public string inboundStops { get; set; }

        public string lowestPrice { get; set; }

        public FlightsListItem()
        {
            outboundId = "";
            inboundId = "";
            outboundAirlines = "";
            inboundAirlines = "";
            outboundAirports = "";
            inboundAirports = "";
            outboundFlightTime = "";
            inboundFlightTime = "";
            outboundAddDays = "";
            inboundAddDays = "";
            outboundDuration = "";
            inboundDuration = "";
            outboundStops = "";
            inboundStops = "";
            lowestPrice = "";
        }
    }
}
