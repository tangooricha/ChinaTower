using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Algorithms
{
    public static class Combine
    {
        public static IEnumerable<Tower> CombineTowers(IEnumerable<Tower> towers, double left, double right, double top, double bottom)
        {
            if (towers.Count() <= 529)
                return towers;
            var result = new List<Tower>();
            for (var i = left; i <= right; i += (right - left) / 23.0)
            {
                for (var j = bottom; j <= top; j += (top - bottom) / 23.0)
                {
                    var tmp = towers.Where(x => x.Lon >= i && x.Lon <= i + (right - left) / 23 && x.Lat >= j && x.Lat <= j + (top - bottom) / 23.0).ToList();
                    if (tmp.Count() > 0)
                        result.Add(tmp[0]);
                }
            }
            return result;
        }
    }
}
