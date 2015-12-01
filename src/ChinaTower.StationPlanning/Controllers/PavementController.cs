using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Controllers
{
    public class PavementController : BaseController
    {
        public IActionResult Index()
        {
            var ret = DB.RxLevLines.Select(x => x.Time).DistinctBy(x => new DateTime(x.Year, x.Month, 1)).ToList();
            return View(ret);
        }

        public IActionResult Import(IFormFile file)
        {
            var src = new List<RxLevPoint>();
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
                                src.Add(new RxLevPoint
                                {
                                    Lon = Convert.ToDouble(reader[0]),
                                    Lat = Convert.ToDouble(reader[1]),
                                    Signal = Convert.ToInt32(reader[2])
                                });
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            var ret = Algorithms.p2l.Handle(src);
            DB.RxLevLines.AddRange(ret);
            DB.SaveChanges();
            return Prompt(x =>
            {
                x.Title = "导入成功";
                x.Details = "RxLev信息导入成功";
            });
        }

        public IActionResult Remove (int year, int month)
        {
            var lines = DB.RxLevLines.Where(x => x.Time.Year == year && x.Time.Month == month).ToList();
            foreach (var x in lines)
                DB.RxLevLines.Remove(x);
            DB.SaveChanges();
            return Prompt(x =>
            {
                x.Title = "删除成功";
                x.Details = "已经删除路测信号";
            });
        }
    }
}