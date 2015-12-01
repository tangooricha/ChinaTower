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
    [Authorize]
    public class TowerController : BaseController
    {
        // GET: /Tower/
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

        public IActionResult Export(string name, string city, string district, TowerType? type, TowerStatus? status, Provider? provider)
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
            return XlsView(towers.ToList());
        }

        public IActionResult Tower(double? left, double? right, double? top, double? bottom)
        {
            IEnumerable<Tower> towers = DB.Towers;
            if (left.HasValue)
                towers = towers.Where(x => x.Lon >= left.Value);
            if (right.HasValue)
                towers = towers.Where(x => x.Lon <= right.Value);
            if (top.HasValue)
                towers = towers.Where(x => x.Lat <= top.Value);
            if (bottom.HasValue)
                towers = towers.Where(x => x.Lat >= bottom.Value);
            return Json(towers.ToList());
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

        public IActionResult Position(double? left, double? right, double? top, double? bottom, int? year, int? month, [FromServices]IConfiguration Config)
        {
            IEnumerable<Tower> towers = DB.Towers;
            if (right.HasValue && right - left > 1.188078574995771)
            {
                return Json(new List<RxLevLine>());
            }
            else
            {
                if (year.HasValue)
                    towers = towers.Where(x => x.Time.Year == year.Value);
                if (month.HasValue)
                    towers = towers.Where(x => x.Time.Month == month.Value);
                if (left.HasValue)
                    towers = towers.Where(x => x.Lon >= left.Value);
                if (right.HasValue)
                    towers = towers.Where(x => x.Lon <= right.Value);
                if (top.HasValue)
                    towers = towers.Where(x => x.Lat <= top.Value);
                if (bottom.HasValue)
                    towers = towers.Where(x => x.Lat >= bottom.Value);
                if (User.IsInRole("Member"))
                {
                    var claims = User.Claims
                        .Where(x => x.Type == "有权限访问地市数据")
                        .Select(x => x.Value)
                        .ToList();
                    towers = towers.Where(x => claims.Contains(x.City));
                }

                towers = towers.ToList();
                var ret = new List<Models.Suggest>();
                foreach (var x in towers)
                    x.Radius = GetShareRadius(x.Scene, Config);
                foreach (var x in towers.GroupBy(x => x.District).Select(x => x.ToList()))
                    ret.AddRange(Algorithms.Suggest.SuggestPositions(x.Select(y => new Position { Lat = y.Lat, Lon = y.Lon, Radius = GetRadius(y.Scene, Config) }).ToList()).Select(y => new Models.Suggest { Lat = y.Lat, Lon = y.Lon, Radius = y.Radius, Status = TowerStatus.预选 }));
                (towers as List<Tower>).AddRange(ret.Select(x => new Tower { Status = TowerStatus.预选, Lon = x.Lon, Lat = x.Lat, Radius = x.Radius }));
                return Json(towers);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(IFormFile file, TowerStatus status)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();
                var schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                var rows = schemaTable.Rows;
                foreach (System.Data.DataRow r in rows)
                {
                    if (r["TABLE_NAME"].ToString() == "_xlnm#_FilterDatabase")
                        continue;
                    var cmd = new OleDbCommand($"select * from [{r["TABLE_NAME"].ToString()}]", conn);
                    var reader = cmd.ExecuteReader();
                    var flag = reader.Read();
                    var cities = User.Claims.Where(x => x.Type == "有权限访问地市数据").Select(x => x.Value).ToList();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(reader[0].ToString()))
                                    goto end;
                                if (User.IsInRole("Member") && !cities.Contains(reader[0].ToString()))
                                    continue;
                                var tower = new Tower
                                {
                                    City = reader[0].ToString(),
                                    District = reader[1].ToString(),
                                    Provider = (Provider)Enum.Parse(typeof(Provider), reader[2].ToString()),
                                    Name = reader[3].ToString(),
                                    Scene = (TowerScene)Enum.Parse(typeof(TowerScene), reader[4].ToString()),
                                    Lon = Convert.ToDouble(reader[5]),
                                    Lat = Convert.ToDouble(reader[6]),
                                    Address = reader[7].ToString(),
                                    Type = (TowerType)Enum.Parse(typeof(TowerType), reader[8].ToString()),
                                    Height = Convert.ToDouble(reader[9]),
                                    Time = DateTime.Now
                                };
                                DB.Towers.Add(tower);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        end:
                        DB.SaveChanges();
                    }
                }
            }
            return Prompt(x => 
            {
                x.Title = "导入成功";
                x.Details = "铁塔信息已经成功导入数据库！";
            });
        }

        [HttpPost]
        public string DeleteMulti(string ids)
        {
            var tmp = ids.Split(' ');
            foreach(var y in tmp)
            {
                var id = Guid.Parse(y);
                var tower = DB.Towers.Where(x => x.Id == id).Single();
                DB.Towers.Remove(tower);
                DB.SaveChanges();
            }
            return "ok";
        }

        [HttpPost]
        public string Delete(Guid id)
        {
            var tower = DB.Towers.Where(x => x.Id == id).Single();
            DB.Towers.Remove(tower);
            DB.SaveChanges();
            return "ok";
        }

        [HttpPost]
        public IActionResult Edit(Guid id, Tower Model, IFormFile file)
        {
            var tower = DB.Towers.Where(x => x.Id == id).Single();
            tower.Name = Model.Name;
            tower.Lat = Model.Lat;
            tower.Lon = Model.Lon;
            tower.Address = Model.Address;
            tower.City = Model.City;
            tower.District = Model.District;
            tower.Height = Model.Height;
            tower.Type = Model.Type;
            tower.Status = Model.Status;
            tower.Url = Model.Url;
            tower.Provider = Model.Provider;
            tower.Scene = Model.Scene;
            if (file != null)
            {
                var blob = new Blob
                {
                    ContentLength = file.Length,
                    ContentType = file.ContentType,
                    FileName = file.GetFileName(),
                    Time = DateTime.Now,
                    File = file.ReadAllBytes()
                };
                DB.Blobs.Add(blob);
                tower.BlobId = blob.Id;
            }
            DB.SaveChanges();
            return Prompt(x =>
            {
                x.Title = "修改成功";
                x.Details = "铁塔信息已修改成功！";
            });
        }

        public IActionResult Create(Tower Model, IFormFile file)
        {
            Model.Time = DateTime.Now;
            if (file != null)
            {
                var blob = new Blob
                {
                    ContentLength = file.Length,
                    ContentType = file.ContentType,
                    FileName = file.GetFileName(),
                    Time = DateTime.Now,
                    File = file.ReadAllBytes()
                };
                DB.Blobs.Add(blob);
                Model.BlobId = blob.Id;
            }
            DB.Towers.Add(Model);
            return RedirectToAction("Index", "Map", new { lon = Model.Lon, lat = Model.Lat });
        }

        public IActionResult Sharing(double left, double right, double top, double bottom, [FromServices] WgsDis WgsDis)
        {
            var towers = DB.Towers
                .Where(x => x.Lon >= left)
                .Where(x => x.Lon <= right)
                .Where(x => x.Lat >= bottom)
                .Where(x => x.Lat <= top)
                .GroupBy(x => x.District)
                .ToList();
            var ret = new List<KeyValuePair<Tower, Tower>>();
            foreach (var x in towers)
                ret.AddRange(WgsDis.Solve(x.ToList()));
            return Json(ret.Select(x => new
            {
                BeginLat = x.Key.Lat,
                EndLat = x.Value.Lat,
                BeginLon = x.Key.Lon,
                EndLon = x.Value.Lon,
                Status = x.Key.Status
            }));
        }

        public IActionResult Share([FromServices] WgsDis WgsDis, bool? raw)
        {
            var towers = DB.Towers
                .GroupBy(x => x.District)
                .ToList();
            var ret = new List<KeyValuePair<Tower, Tower>>();
            foreach (var x in towers)
                ret.AddRange(WgsDis.Solve(x.ToList()));
            ret = ret.Where(x => x.Key.Status != TowerStatus.预选 && x.Value.Status != TowerStatus.预选).ToList();
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret, "export.xls", "~/Views/Tower/ExportShare.cshtml");
            else
                return View(ret); 
        }

        public IActionResult Relation([FromServices] WgsDis WgsDis, bool? raw)
        {
            var towers = DB.Towers
                .GroupBy(x => x.District)
                .ToList();
            var ret = new List<KeyValuePair<Tower, Tower>>();
            foreach (var x in towers)
                ret.AddRange(WgsDis.Solve(x.ToList()));
            ret = ret.Where(x => x.Key.Status == TowerStatus.难点 || x.Value.Status != TowerStatus.难点).ToList();
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret, "export.xls", "~/Views/Tower/ExportRelation.cshtml");
            else
                return View(ret);
        }

        public IActionResult NewAP([FromServices] WgsDis WgsDis, bool? raw)
        {
            var towers = DB.Towers
                .GroupBy(x => x.District)
                .ToList();
            var ret = new List<KeyValuePair<Tower, Tower>>();
            foreach (var x in towers)
                ret.AddRange(WgsDis.Solve(x.ToList()));
            ret = ret.Where(x => x.Key.Status == TowerStatus.预选 || x.Value.Status == TowerStatus.预选).ToList();
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret, "export.xls", "~/Views/Tower/ExportNewAP.cshtml");
            else
                return View(ret);
        }

        public IActionResult Suggest([FromServices]IConfiguration Config)
        {
            var source = DB.Towers
                .GroupBy(x => x.District)
                .ToList();
            var ret = new List<Models.Suggest>();
            foreach (var x in source)
                ret.AddRange(Algorithms.Suggest.SuggestPositions(x.Select(y => new Position { Lat = y.Lat, Lon = y.Lon, Radius = GetRadius(y.Scene, Config) }).ToList()).Select(y => new Models.Suggest { Lat = y.Lat, Lon = y.Lon, Radius = y.Radius, Status = TowerStatus.预选 }));
            return View(ret);
        }

        private double GetRadius(TowerScene scene, IConfiguration Config)
        {
            switch (scene)
            {
                case TowerScene.一般城区:
                    return Convert.ToDouble(Config["Settings:Plan:City"]);
                case TowerScene.密集城区:
                    return Convert.ToDouble(Config["Settings:Plan:Crowded"]);
                case TowerScene.郊区:
                    return Convert.ToDouble(Config["Settings:Plan:Suburb"]);
                case TowerScene.农村:
                    return Convert.ToDouble(Config["Settings:Plan:Village"]);
                default:
                    return 0;
            }
        }

        private double GetShareRadius(TowerScene scene, IConfiguration Config)
        {
            switch (scene)
            {
                case TowerScene.一般城区:
                    return Convert.ToDouble(Config["Settings:Share:City"]);
                case TowerScene.密集城区:
                    return Convert.ToDouble(Config["Settings:Share:Crowded"]);
                case TowerScene.郊区:
                    return Convert.ToDouble(Config["Settings:Share:Suburb"]);
                case TowerScene.农村:
                    return Convert.ToDouble(Config["Settings:Share:Village"]);
                default:
                    return 0;
            }
        }

        public IActionResult InitPosition()
        {
            if (User.IsInRole("Member"))
            {
                var cities = User.Claims.Where(x => x.Type == "有权限访问地市数据").Select(x => x.Value).ToList();
                if (cities.Count == 0)
                    goto findfirst;
                var ret = DB.Towers.Where(x => x.City == cities.First()).First();
                return Json(new { Lat = ret.Lat, Lon = ret.Lon });
            }
            findfirst:
            var tower = DB.Towers.FirstOrDefault();
            if (tower != null)
                return Json(new { Lat = tower.Lat, Lon = tower.Lon });
            else
                return Json(new { Lat = 123, Lon = 45 });
        }
    }
}
