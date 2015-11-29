using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using ChinaTower.StationPlanning.Models;

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
            var pi = ViewData["PagerInfo"] as PagerInfo;
            return PagedView(towers);
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
        public string Delete(Guid id)
        {
            var tower = DB.Towers.Where(x => x.Id == id).Single();
            DB.Towers.Remove(tower);
            DB.SaveChanges();
            return "ok";
        }
    }
}
