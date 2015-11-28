using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Algorithms
{
    public static class p2l
    {
        private static double L = 1;
        private static SignalColor GetColor(RxLevPoint point)
        {
            if (point.Signal >= -65 && point.Signal < 0)
                return SignalColor.Green;
            else if (point.Signal >= -75 && point.Signal <= -65)
                return SignalColor.Yellow;
            else
                return SignalColor.Red;
        }

        private static double Cal(RxLevPoint a, RxLevPoint b) => Math.Atan2(a.Lat - b.Lat, a.Lon - b.Lon);
        private static double Sqr(double a) => a * a;
        private static double Dis2(RxLevPoint a, RxLevPoint b) => Sqr(a.Lat - b.Lat) + Sqr(a.Lon - b.Lon);
        private static bool Equ(RxLevPoint a, RxLevPoint b) => Dis2(a, b) <= L / 150;
        private static bool Equ2(RxLevPoint a, RxLevPoint b) => Dis2(a, b) <= L;
        private static int Cut(IList<RxLevPoint> t, int l, int r)
        {
            var mx = 0.0;
            var c = l;
            for (var i = l + 1; i < r; i++)
                if (Math.Abs(Cal(t[l], t[i]) - Cal(t[i], t[r])) > mx)
                {
                    mx = Math.Abs(Cal(t[l], t[i]) - Cal(t[i], t[r]));
                    c = i;
                }
            if (mx < 0.35) return l;
            else return c;
        }
        private static IList<RxLevPoint> Pre(IList<RxLevPoint> t)
        {
            if (t.Count < 2) return t;
            var ans = new List<RxLevPoint>();
            for (var i = 1; i < t.Count; i++)
            {
                if (Equ(t[i], ans[ans.Count - 1]))
                    continue;
                ans.Add(t[i]);
            }
            return ans;
        }
        private static IList<RxLevLine> Merge(IList<RxLevLine> t)
        {
            var ans = new List<RxLevLine>();
            var now = Cal(new RxLevPoint { Lon = t[0].BeginLon, Lat = t[0].BeginLat }, new RxLevPoint { Lat = t[0].EndLat, Lon = t[0].EndLon });
            ans.Add(t[0]);
            for (var i = 1; i < t.Count; i++)
                if (Math.Abs(Cal(new RxLevPoint { Lon = t[i].BeginLon, Lat = t[i].BeginLat }, new RxLevPoint { Lat = t[i].EndLat, Lon = t[i].EndLon }) - now) < 0.3 && (Dis2(new RxLevPoint { Lon = t[i].BeginLon, Lat = t[i].BeginLat }, new RxLevPoint { Lat = t[i].EndLat, Lon = t[i].EndLon }) < L / 10) && (t[i].BeginLat == ans.Last().EndLat && t[i].BeginLon == ans.Last().EndLon))
                {
                    ans[ans.Count - 1].EndLon = t[i].EndLon;
                    ans[ans.Count - 1].EndLat = t[i].EndLat;
                }
                else
                {
                    ans.Add(t[i]);
                    now = Cal(new RxLevPoint { Lon = t[i].BeginLon, Lat = t[i].BeginLat }, new RxLevPoint { Lat = t[i].EndLat, Lon = t[i].EndLon });
                }
            return ans;
        }
        private static IList<RxLevLine> Solve(IList<RxLevPoint> t)
        {
            var ans = new List<RxLevLine>();
            var now = 0;
            var last = 0;
            var color = GetColor(t[now]);
            for (var i = 1; i < t.Count; i++)
            {
                if (Math.Abs(Cal(t[now], t[last]) - Cal(t[now], t[i])) > 0.2 || Dis2(t[now], t[last]) > L || GetColor(t[i]) != color)
                {
                    if (now < last - 1)
                    {
                        var c = Cut(t, now, last);
                        if (c != now)
                            ans.Add(new RxLevLine { BeginLat = t[now].Lat, BeginLon = t[now].Lon, EndLat = t[c].Lat, EndLon = t[c].Lon, Color = color });
                        ans.Add(new RxLevLine { BeginLat = t[c].Lat, BeginLon = t[c].Lon, EndLat = t[last].Lat, EndLon = t[last].Lon, Color = color });
                        now = last;
                        last = i;
                        color = GetColor(t[i]);
                    }
                    if (Dis2(t[now], t[i]) > L) now = i;
                }
                last = i;
                while (Dis2(t[now], t[last]) > 2 * L) now++;
            }
            if (now != last)
                ans.Add(new RxLevLine { BeginLat = t[now].Lat, BeginLon = t[now].Lon, EndLat = t[last].Lat, EndLon = t[last].Lon, Color = color });
            return ans;
        }
    }
}
