using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.File;
using Twinkle.Framework.Import;
using Twinkle.Framework.Utils;

namespace Twinkle.Controllers
{
    public class DemoController : Controller
    {
        #region excel操作
        public JsonResult CreateExcel()
        {
            Workbook wb = new Workbook();
            Worksheet ws = wb.Worksheets[0];
            ws.Name = "测试文档";

            object[] vals = new object[] { 1, "2", 2.06, "2.02", DateTime.Now, DateTime.Now.ToString() };

            for (int i = 0; i < 100000; i++)
            {
                //区域赋值生成的excel比较小
                ws.Cells.CreateRange(i, 0, 1, 1).Value = vals;

                //单元格赋值,生成的excel比较大
                //ws.Cells[i, 0].Value = 1;
                //ws.Cells[i, 1].Value = "2";
                //ws.Cells[i, 2].Value = 2.06;
                //ws.Cells[i, 3].Value = "2.02";
                //ws.Cells[i, 4].Value = DateTime.Now;
                //ws.Cells[i, 5].Value = DateTime.Now.ToString();
            }


            string path = Path.Combine(TwinkleContext.AppRoot, "excelDemo");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            wb.Save(Path.Combine(path, "代码生成.xlsx"));

            return Json(new { status = 0 });
        }

        public JsonResult ReadExcel()
        {
            string path = Path.Combine(TwinkleContext.AppRoot, "excelDemo");

            DataTable dt = DataReader.ReadEntireDataTable(Path.Combine(path, "代码生成.xlsx"));

            return Json(new { status = 0 });
        }
        #endregion

        public async Task Upload(UploadModel args)
        {
            DataImport si = DataImportFactory.CreateDataImport();
            si.Init(args.FileStream, "twtable");

            await si.ExcuteAsync();
        }

        public IActionResult downLoad(ClientModel client)
        {
            return WebHelper.DownLoad(Path.Combine(TwinkleContext.AppRoot, "excelDemo", "导入测试.xlsx"));

        }

        public JsonResult GetData()
        {
            dynamic data = TwinkleContext.GetRequiredService<DatabaseManager>().ExecuteEntities<dynamic>("SELECT * FROM testTable");

            return Json(new { total = 100, data });
        }
    }
}