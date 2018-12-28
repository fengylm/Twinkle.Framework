using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;

namespace Twinkle.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected DatabaseManager Db = null;

        public BaseController()
        {
            Db = TwinkleContext.GetRequiredService<DatabaseManager>();
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

            int bodyIndex = strSQL.IndexOf("select", StringComparison.CurrentCultureIgnoreCase) + "select".Length;

            int dataCount = (db ?? Db).ExecuteInteger($"SELECT COUNT(1) FROM ({strSQL}) totalTable", parameters);

            if (dataCount == 0)
            {
                return Json(new
                {
                    total = 0,
                    data = new object[] { }
                });
            }

            if (start != null && limit != null)
            {
                // 处理在改变数据后 当前页码大于实际页码 导致查询无数据的问题
                if (dataCount < (start * limit) + 1)
                {
                    start = dataCount == limit && start > 0 ? start - 1 : Convert.ToInt32(Math.Floor(dataCount / limit.Value * 1m));
                }
            }

            StringBuilder pagingBuild = new StringBuilder();
            pagingBuild.AppendLine("SELECT * FROM (");
            pagingBuild.AppendLine($"SELECT ROW_NUMBER() OVER(ORDER BY {orderBy}) _rowIndex,");
            pagingBuild.AppendLine(strSQL.Substring(bodyIndex));
            pagingBuild.AppendLine(") pagingTable WHERE 1=1");
            pagingBuild.AppendLine($" AND _rowIndex>{(start ?? 0) * (limit ?? 0)} AND _rowIndex<={((start ?? 0) + 1) * (limit ?? int.MaxValue)}");

            return Json(new
            {
                total = dataCount,
                data = (db ?? Db).ExecuteEntities<dynamic>(pagingBuild.ToString(), parameters)
            });
        }



    }
}
