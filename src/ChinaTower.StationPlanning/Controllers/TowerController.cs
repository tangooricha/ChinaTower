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
    }
}
