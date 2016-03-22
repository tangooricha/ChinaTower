using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Newtonsoft.Json;

namespace ChinaTower.StationPlanning.Controllers
{
    public class FormController : BaseController
    {
        public static string[] HeaderOfA = new string[] 
        {
            "项目编号",
            "省份",
            "地市",
            "区县",
            "站点名称",
            "站点地址",
            "经度",
            "纬度",
            "所属场景",
            "资源类型",
            "储备站类别",
            "电信需求系统数（套）",
            "移动需求系统数（套）",
            "联通需求系统数（套）",
            "天线数量(副)"
        };

        public static string[] HeaderOfB = new string[]
        {
            "序号",
            "省份",
            "地市",
            "区县",
            "难点站名称",
            "详细地址",
            "运营商",
            "区域类型",
            "所属场景",
            "其它场景",
            "经度",
            "纬度",
            "入库时间",
            "主动规划",
            "详细难点原因描述",
            "是否存在协调问题",
            "协调问题分类",
            "是否存在建设问题",
            "建设问题分类",
            "是否存在其它问题",
            "其他问题",
            "难点负责人",
            "电话",
            "备注",
            "难点站址图片",
            "难点拟解决方案",
            "难点是否解决",
            "本周难点进度"
        };

        public static string[] HeaderOfC = new string[]
        {
            "项目编号",
            "站点编号",
            "省份",
            "地市",
            "区县",
            "站点名称",
            "站点地址",
            "经度",
            "纬度",
            "归属场景",
            "铁塔类型",
            "铁塔高度（米）",
            "机房类型	机房面积（平米）",
            "电信系统数（套）",
            "移动系统数（套）",
            "联通系统数（套）",
            "剩余平台数目（个）",
            "剩余抱杆",
            "配套改造投资（万元）",
            "其他投资（万元）",
            "投资合计（万元）",
            "年平均收入（万元）"
        };

        public static string[] HeaderOfD = new string[]
        {
            "项目编号",
            "省份",
            "地市",
            "区县",
            "站点名称",
            "站点地址",
            "经度",
            "纬度",
            "所属场景",
            "电信需求编号",
            "移动需求编号",
            "联通需求编号",
            "系统数量（套）",
            "天线挂高（米）",
            "天线数量(副)",
            "铁塔类型",
            "塔高（米）",
            "机房类型",
            "机房面积（平米）",
            "外电引入方式",
            "铁塔投资（万元）",
            "机房投资（万元）",
            "配套投资（万元）",
            "外电引入投资（万元）",
            "其他投资（万元）",
            "总投资（万元）",
            "年平均收入（万元）"
        };

        public static string[] HeaderOfE = new string[]
        {
            "序号",
            "省份",
            "地市",
            "区县",
            "难点站名称",
            "详细地址",
            "运营商",
            "区域类型",
            "所属场景",
            "其它场景",
            "经度",
            "纬度",
            "入库时间",
            "需求批次",
            "详细难点原因描述",
            "是否存在协调问题",
            "协调问题分类",
            "是否存在建设问题",
            "建设问题分类",
            "是否存在其它问题",
            "其他问题",
            "难点负责人",
            "电话",
            "备注",
            "难点站址图片",
            "难点拟解决方案",
            "难点是否解决",
            "本周难点进度"
        };

        public static string[] HeaderOfF = new string[] 
        {
            "运营商","省份","地市","区县","当年项目批次","站点名称","场景划分","交付时间要求(年月日)","经度","纬度","覆盖范围描述","允许偏离半径(米)","系统数（个）","系统类型及频段","天线挂高范围(m)","天线数(个)","RRU是否拉远","需求提出时间(年月日)","入库原因","客户是否确认","入库时间","站点优先级","当前进度","详细选址信息","预计完成时间"
        };

        public static string[] HeaderOfG1 = new string[] 
        {
            "城市", "区县", "站点名称", "来源", "经纬度", "覆盖范围描述", "站点优先级", "建档日期", "场景划分", "挂高范围(米)", "详细地址", "经纬度", "业主", "业主意向", "区域经理", "是否确认难点", "建设主任", "是否确认难点", "主管建设领导", "是否确认难点"
        };

        public static string[] HeaderOfG2 = new string[] 
        {
            "选址时间","选址未成功原因","选址人","本次业主联系人","选址日志"
        };

        [HttpGet]
        public IActionResult A(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.储备库).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult A(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 1; i <= HeaderOfA.Count(); i++)
                                    text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.储备库
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("A", "Form", null);
        }

        [HttpGet]
        public IActionResult B(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.在建难点库).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult B(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 0; i <= HeaderOfB.Count(); i++)
                                    if (i!=25)
                                        text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.在建难点库
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("B", "Form", null);
        }

        [HttpGet]
        public IActionResult C(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.存量资源).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult C(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 1; i <= HeaderOfC.Count(); i++)
                                    text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.存量资源
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("C", "Form", null);
        }

        [HttpGet]
        public IActionResult D(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.新建站址).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult D(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 1; i <= HeaderOfD.Count(); i++)
                                    text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.新建站址
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("D", "Form", null);
        }

        [HttpGet]
        public IActionResult E(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.潜在难点库).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult E(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 0; i < HeaderOfE.Count(); i++)
                                    text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.潜在难点库
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("E", "Form", null);
        }

        [HttpGet]
        public IActionResult F(bool? raw)
        {
            var ret = DB.Forms.Where(x => x.Type == Models.FormType.疑难站点库).OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult F(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
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
                    flag = reader.Read();
                    flag = reader.Read();
                    if (flag)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var text = new List<string>();
                                for (var i = 0; i < HeaderOfE.Count(); i++)
                                    text.Add(reader[i].ToString());
                                DB.Forms.Add(new Models.Form
                                {
                                    City = city,
                                    Content = JsonConvert.SerializeObject(text),
                                    Time = DateTime.Now,
                                    Type = Models.FormType.疑难站点库
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        DB.SaveChanges();
                    }
                }
            }
            return RedirectToAction("F", "Form", null);
        }

        [HttpGet]
        public IActionResult G(bool? raw)
        {
            var ret = DB.Forms
                .Include(x => x.Children)
                .Where(x => x.Type == Models.FormType.疑难站址档案)
                .OrderByDescending(x => x.Time);
            if (raw.HasValue && raw.Value == true)
                return XlsView(ret.ToList(), "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return PagedView(ret);
        }

        [HttpPost]
        public IActionResult G(IFormFile file, string city)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();
                var schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                var rows = schemaTable.Rows;
                foreach (DataRow r in rows)
                {
                    if (r["TABLE_NAME"].ToString() == "_xlnm#_FilterDatabase")
                        continue;
                    var cmd = new OleDbCommand($"select * from [{r["TABLE_NAME"].ToString()}]", conn);
                    var adpt = new OleDbDataAdapter(cmd);
                    var dt = new DataTable();
                    adpt.Fill(dt);
                    var text = new List<string>();
                    text.Add(dt.Rows[1][3].ToString());
                    text.Add(dt.Rows[1][8].ToString());
                    text.Add(dt.Rows[2][3].ToString());
                    text.Add(dt.Rows[2][8].ToString());
                    text.Add(dt.Rows[3][3].ToString());
                    text.Add(dt.Rows[3][8].ToString());
                    text.Add(dt.Rows[4][3].ToString());
                    text.Add(dt.Rows[4][8].ToString());
                    text.Add(dt.Rows[5][3].ToString());
                    text.Add(dt.Rows[5][8].ToString());
                    text.Add(dt.Rows[6][3].ToString());
                    text.Add(dt.Rows[6][8].ToString());
                    text.Add(dt.Rows[7][3].ToString());
                    text.Add(dt.Rows[7][8].ToString());
                    text.Add(dt.Rows[8][3].ToString());
                    text.Add(dt.Rows[8][8].ToString());
                    text.Add(dt.Rows[9][3].ToString());
                    text.Add(dt.Rows[9][8].ToString());
                    text.Add(dt.Rows[10][3].ToString());
                    text.Add(dt.Rows[10][8].ToString());
                    var form = new Models.Form
                    {
                        City = city,
                        Content = JsonConvert.SerializeObject(text),
                        Time = DateTime.Now,
                        Type = Models.FormType.疑难站址档案
                    };
                    DB.Forms.Add(form);

                    for (var i = 11; i + 3 < dt.Rows.Count; i += 3)
                    {
                        var text2 = new List<string>();
                        text2.Add(dt.Rows[i][3].ToString());
                        text2.Add(dt.Rows[i][8].ToString());
                        text2.Add(dt.Rows[i + 1][3].ToString());
                        text2.Add(dt.Rows[i + 1][8].ToString());
                        text2.Add(dt.Rows[i + 2][3].ToString());
                        DB.Forms.Add(new Models.Form
                        {
                            City = city,
                            Time = DateTime.Now,
                            Type = Models.FormType.疑难站址档案2,
                            ParentId = form.Id,
                            Content = JsonConvert.SerializeObject(text2),
                        });
                    }

                    DB.SaveChanges();
                }
            }
            return RedirectToAction("G", "Form", null);
        }

        [HttpPost]
        public IActionResult Insert(string[] text, Models.FormType type, string city)
        {

            DB.Forms.Add(new Models.Form
            {
                City = city,
                Content = JsonConvert.SerializeObject(text),
                Type = type,
                Time = DateTime.Now
            });
            DB.SaveChanges();
            return Prompt(x =>
            {
                x.Title = "添加成功";
                x.Details = "表单信息添加成功";
            });
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            return View(DB.Forms.Where(x => x.Id == id).Single());
        }

        [HttpPost]
        public IActionResult Edit(Guid id, string[] text)
        {
            var form = DB.Forms.Where(x => x.Id == id).Single();
            form.Content = JsonConvert.SerializeObject(text);
            DB.SaveChanges();
            return Prompt(x =>
            {
                x.Title = "修改成功";
                x.Details = "表单信息已保存！";
            });
        }

        [HttpGet]
        public IActionResult Detail(Guid id, bool? raw)
        {
            var form = DB.Forms
                .Include(x => x.Children)
                .Single(x => x.Id == id);
            if (raw.HasValue && raw.Value == true)
                return XlsView(form.Children, "form.xls", "~/Views/Form/Xls.cshtml");
            else
                return View(form);
        }
    }
}
