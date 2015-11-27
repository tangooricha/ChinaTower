using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace ChinaTower.StationPlanning.Controllers
{
    public class HomeController : BaseController
    {
        // GET: /
        public IActionResult Index()
        {
            return View();
        }
    }
}
