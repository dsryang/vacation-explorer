﻿namespace VacationExplorer
{
    public class Location
    {
        public string address { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string[] formattedAddress { get; set; }
    }
}