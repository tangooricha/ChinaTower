using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Controllers
{
    public class TowerController : BaseController
    {
        // GET: /Tower/
        public IActionResult Index(string city, string district, TowerType? type, TowerStatus? status, Provider? provider)
        {
            IEnumerable<Tower> towers = DB.Towers;
            if (!string.IsNullOrEmpty(city))
                towers = towers.Where(x => x.City.Contains(city) || city.Contains(x.City));
            if (!string.IsNullOrEmpty(district))
                towers = towers.Where(x => x.District.Contains(district) || district.Contains(x.District));
            if (type.HasValue)
                towers = towers.Where(x => x.Type == type.Value);
            if (status.HasValue)
                towers = towers.Where(x => x.Status == status.Value);
            if (type.HasValue)
                towers = towers.Where(x => x.Provider == provider.Value);
            return AjaxPagedView(towers, "lst-towers");
        }

        public IActionResult RxLev(double? left, double? right, double? top, double? bottom, int? year, int? month)
        {
            IEnumerable<RxLevLine> signals = DB.RxLevLines;
            if (right.HasValue && right - left > 1.188078574995771)
            {
                return Json(new List<RxLevLine>());
            }
            else
            {
                if (year.HasValue)
                    signals = signals.Where(x => x.Time.Year == year.Value);
                if (month.HasValue)
                    signals = signals.Where(x => x.Time.Month == month.Value);
                if (left.HasValue)
                    signals = signals.Where(x => x.BeginLon >= left.Value || x.EndLon >= left.Value);
                if (right.HasValue)
                    signals = signals.Where(x => x.BeginLon <= right.Value || x.EndLon <= right.Value);
                if (top.HasValue)
                    signals = signals.Where(x => x.BeginLat <= top.Value || x.EndLat <= top.Value);
                if (bottom.HasValue)
                    signals = signals.Where(x => x.BeginLat >= bottom.Value || x.EndLat >= bottom.Value);
                if (User.IsInRole("Member"))
                {
                    var claims = User.Claims
                        .Where(x => x.Type == "有权限访问地市数据")
                        .Select(x => x.Value)
                        .ToList();
                    signals = signals.Where(x => claims.Contains(x.City));
                }
                return Json(signals.ToList());
            }
        }
    }
}
