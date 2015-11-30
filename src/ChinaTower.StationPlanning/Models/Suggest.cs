using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTower.StationPlanning.Models
{
    public class Suggest
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
        public double Radius { get; set; }
        public TowerStatus Status{ get; set; }
    }
}
