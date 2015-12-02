using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;

namespace ChinaTower.StationPlanning.Controllers
{
    public class SettingController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index([FromServices] IConfiguration Config, double city, double _city, double crowded, double _crowded, double village, double _village, double suburb, double _suburb)
        {
            Config["Settings:Share:Suburb"] = suburb.ToString();
            Config["Settings:Plan:Suburb"] = _suburb.ToString();
            Config["Settings:Share:City"] = city.ToString();
            Config["Settings:Plan:City"] = _city.ToString();
            Config["Settings:Share:Crowded"] = crowded.ToString();
            Config["Settings:Plan:Crowded"] = _crowded.ToString();
            Config["Settings:Share:Village"] = village.ToString();
            Config["Settings:Plan:Village"] = _village.ToString();
            return View();
        }
    }
}
