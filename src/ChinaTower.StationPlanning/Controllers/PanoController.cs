using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Configuration;
using ChinaTower.StationPlanning.Models;
using ChinaTower.StationPlanning.Algorithms;

namespace ChinaTower.StationPlanning.Controllers
{
    public class PanoController : BaseController
    {
        public IActionResult Index(string name, string city, string district, TowerType? type, TowerStatus? status, Provider? provider)
        {
            IEnumerable<Tower> towers = DB.Towers;
            if (!string.IsNullOrEmpty(name))
                towers = towers.Where(x => x.Name.Contains(name) || name.Contains(x.Name));
            if (!string.IsNullOrEmpty(city))
                towers = towers.Where(x => x.City.Contains(city) || city.Contains(x.City));
            if (!string.IsNullOrEmpty(district))
                towers = towers.Where(x => x.District.Contains(district) || district.Contains(x.District));
            if (type.HasValue)
                towers = towers.Where(x => x.Type == type.Value);
            if (status.HasValue)
                towers = towers.Where(x => x.Status == status.Value);
            if (provider.HasValue)
                towers = towers.Where(x => x.Provider == provider.Value);
            return PagedView(towers);
        }
    }
}
