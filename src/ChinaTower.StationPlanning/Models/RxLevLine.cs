using System;
using System.ComponentModel.DataAnnotations;

namespace ChinaTower.StationPlanning.Models
{
    public class RxLevLine
    {
        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public double BeginLon { get; set; }
        
        public double BeginLat { get; set; }
        
        public double EndLon { get; set; }

        public double EndLat { get; set; }

        [MaxLength(32)]
        public string City { get; set; }
    }
}
