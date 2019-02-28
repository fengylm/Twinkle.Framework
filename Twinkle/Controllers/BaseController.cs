using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.Security.Authorization;
using Twinkle.Framework.Utils;
using Twinkle.Models;

namespace Twinkle.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected DatabaseManager Db = null;
        protected AuthUser Auth = null;
        protected ILogger Logger = null;

        public BaseController()
        {
            Db = TwinkleContext.GetRequiredService<DatabaseManager>();
            Auth = TwinkleContext.GetService<TokenAuthManager>().GetUser(TwinkleContext.UserToken) ?? new AuthUser();
            Auth.TenantId = "0000000000";// 暂时没有多租户模块 给予一个默认租户编码

            Logger = GetLogger((dynamic)this);
        }

        private ILogger GetLogger<T>(T controller)
        {
            return TwinkleContext.GetService<ILogger<T>>();
        }

        /// <summary>
        /// 分页查询,不支持带top,percent关键字的查询
        /// </summary>
        /// <param name="strSQL">查询脚本</param>
        /// <param name="orderBy">排序</param>
        /// <param name="clientModel">客户端参数</param>
        /// <param name="parameters">脚本参数</param>
        /// <param name="db">非默认DatabaseManager 缺省值</param>
        /// <returns></returns>
        public JsonResult Paging(string strSQL, string orderBy, ClientModel clientModel, object parameters = null, DatabaseManager db = null)
        {
            if (string.IsNullOrEmpty(strSQL))
            {
                throw new ArgumentNullException("查询SQL语句不能为空");
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                throw new ArgumentNullException("排序字段不能为空");
            }

            int? start = clientModel.GetInt("_start");
            int? limit = clientModel.GetInt("_limit");
            string sort = clientModel.GetString("_sort");
            if (!string.IsNullOrEmpty(sort))
            {
                orderBy = sort;
            }

            int bodyIndex = strSQL.IndexOf("select", StringComparison.CurrentCultureIgnoreCase) + "select".Length;

            int total = (db ?? Db).ExecuteInteger($"SELECT COUNT(1) FROM ({strSQL}) totalTable", parameters);

            if (total == 0)
            {
                return Json(new
                {
                    total = 0,
                    start = 0,
                    data = new object[] { }
                });
            }

            if (start != null && limit != null)
            {
                // 处理在改变数据后 当前页码大于实际页码 导致查询无数据的问题
                if (total < (start * limit) + 1)
                {
                    start = total == start * limit && start > 0 ? start - 1 : Convert.ToInt32(Math.Floor(total / limit.Value * 1m));
                }
            }

            StringBuilder pagingBuild = new StringBuilder();
            pagingBuild.AppendLine("SELECT * FROM (");
            pagingBuild.AppendLine($"SELECT ROW_NUMBER() OVER(ORDER BY {orderBy}) XROWINDEX,");
            pagingBuild.AppendLine(strSQL.Substring(bodyIndex));
            pagingBuild.AppendLine(") pagingTable WHERE 1=1");
            pagingBuild.AppendLine($" AND XROWINDEX>{(start ?? 0) * (limit ?? 0)} AND XROWINDEX<={((start ?? 0) + 1) * (limit ?? int.MaxValue)}");

            return Json(new
            {
                total,
                start,
                data = (db ?? Db).ExecuteEntities<dynamic>(pagingBuild.ToString(), parameters)
            });
        }

        /// <summary>
        /// 写入响应流,生成下载文件
        /// </summary>
        /// <returns></returns>
        public IActionResult FileDownload(DataTable source, string fileName)
        {
            string ext = FileHelper.FileExtension(fileName);
            SaveFormat saveFormat = (SaveFormat)Enum.Parse(typeof(SaveFormat), ext, true);
            using (Workbook wb = new Workbook())
            {
                wb.Worksheets[0].Cells.ImportData(source, 0, 0, new ImportTableOptions { });
                MemoryStream stream = new MemoryStream();
                wb.Save(stream, saveFormat);
                return WebHelper.DownLoad(stream, fileName);
            }
        }

        /// <summary>
        /// 生成导出excel
        /// </summary>
        /// <param name="source">要导出的数据</param>
        /// <param name="code">配置信息</param>
        /// <param name="fileName">要导出的文件名</param>
        /// <returns></returns>
        public IActionResult ExportData(DataTable source, string code, string fileName)
        {
            string strSQL = @"SELECT * FROM Sys_ColumnsForModule WHERE cModuleCode=@code AND iShow=1
                            ORDER BY nOrderID";
            List<Sys_ColumnsForModule> lst = Db.ExecuteEntities<Sys_ColumnsForModule>(strSQL, new { code });


            for (int i = source.Columns.Count - 1; i >= 0; i--)
            {
                Sys_ColumnsForModule module = lst.Where(p => p.cField.ToLower() == source.Columns[i].ColumnName.ToLower()).FirstOrDefault();
                if (module == null)
                {
                    source.Columns.RemoveAt(i);
                }
                else
                {
                    source.Columns[i].ColumnName = module.cTitle;
                }
            }

            string ext = FileHelper.FileExtension(fileName);
            SaveFormat saveFormat = (SaveFormat)Enum.Parse(typeof(SaveFormat), ext, true);
            using (Workbook wb = new Workbook())
            {
                wb.Worksheets[0].Cells.ImportData(source, 0, 0, new ImportTableOptions { });
                wb.Worksheets[0].FreezePanes(1, 0, 1, source.Columns.Count);
                MemoryStream stream = new MemoryStream();
                wb.Save(stream, saveFormat);
                return WebHelper.DownLoad(stream, fileName);
            }

        }
    }
}
