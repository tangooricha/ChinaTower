using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Algorithms
{
    public static class Suggest
    {
        public static double EarthRadiusKm { get; } = 6378.137;
        private static double Mul(double a, double b, double c, double d) => a * d - b * c;
        private static bool CheckLine(IList<Position> t, Position a, Position b)
        {
            for (var i = 0; i < t.Count; i++)
                if (Mul(b.Lat - a.Lat, b.Lon - a.Lon, b.Lat - t[i].Lat, b.Lon - t[i].Lon) < 0)
                    return false;
            return true;
        }
        private static bool InCon(IList<Line> l, double lat, double lon)
        {
            for (var i = 0; i < l.Count; i++)
                if (Mul(l[i].End.Lat - l[i].Begin.Lat, l[i].End.Lon - l[i].Begin.Lon, l[i].End.Lat - lat, l[i].End.Lon - lon) < 0)
                    return false;
            return true;
        }
        private static IList<Line> Conclo(IList<Position> t)
        {
            var line = new List<Line>();
            for (var i = 0; i < t.Count; i++)
                for (var j = 0; j < t.Count; j++)
                    try
                    {
                        if (i != j && CheckLine(t, t[i], t[j])) line.Add(new Line { Begin = t[i], End = t[j] });
                    }
                    catch
                    {
                    }
            return line;
        }
        private static double Check(IList<Line> l, IList<Position> t, IList<Position> ans, double lat, double lon)
        {
            var mn = GetDistance(lat, lon, t[0].Lat, t[0].Lon);
            var R = t[0].Radius;
            for (var i = 0; i < ans.Count; i++)
                if (GetDistance(lat, lon, ans[i].Lat, ans[i].Lon) <= ans[i].Radius * 1000)
                    return 0;
            for (var i = 0; i < t.Count; i++)
            {
                var dis = GetDistance(lat, lon, t[i].Lat, t[i].Lon);
                if (dis <= t[i].Radius * 1000)
                    return 0;
                else if (dis < mn)
                    mn = dis;
                R = t[i].Radius;
            }
            if (mn > 500 || !InCon(l, lat, lon)) return 0;
            return R;
        }
        public static double GetDistance(double p1Lat, double p1Lng, double p2Lat, double p2Lng)
        {
            var dLat1InRad = p1Lat * (Math.PI / 180);
            var dLong1InRad = p1Lng * (Math.PI / 180);
            var dLat2InRad = p2Lat * (Math.PI / 180);
            var dLong2InRad = p2Lng * (Math.PI / 180);
            var dLongitude = dLong2InRad - dLong1InRad;
            var dLatitude = dLat2InRad - dLat1InRad;
            var a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var dDistance = EarthRadiusKm * c;
            return dDistance * 1000;
        }
        public static IList<Position> SuggestPositions(IList<Position> t)
        {
            var l = Conclo(t);
            var ans = new List<Position>();
            double x0, y0, x1, y1;
            x0 = x1 = t[0].Lat;
            y0 = y1 = t[0].Lon;
            for (var i = 1; i < t.Count; i++)
            {
                try
                {
                    x0 = Math.Min(x0, t[i].Lat);
                    y0 = Math.Min(y0, t[i].Lon);
                    x1 = Math.Max(x1, t[i].Lat);
                    y1 = Math.Max(y1, t[i].Lon);
                }
                catch
                {
                }
            }
            var dis = (x1 - x0) / 35;
            for (var i = x0; i <= x1; i += dis)
                for (var j = y0; j <= y1; j += dis)
                {
                    try
                    {
                        var rad = Check(l, t, ans, i, j);
                        if (rad != 0)
                            ans.Add(new Position { Lat = i, Lon = j, Radius = rad });
                    }
                    catch
                    {
                    }
                }
            return ans;
        }
    }
}
