using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Twinkle.Framework.File;
using Twinkle.Framework.Import;
using Twinkle.Framework.Mvc;

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

        #region 导入测试
        [AllowAnonymous]
        public async Task Upload(UploadFileArgs args)
        {
            BaseImport si = ImportFactory.CreateImport();
            si.Init(args.FileStream, "testTable", new Mapping[] {
                new Mapping{ DBColumn="GUID",Macro=Macro.Guid,Type= DataType.String },
                new Mapping{ DBColumn="INPUTDATE",Macro=Macro.Now,Type= DataType.String },
                new Mapping{ DBColumn="TYPE",Macro=Macro.Default,Value=1,Key=true,Type= DataType.Number }
            });

            await si.ExcuteAsync();
        }
        #endregion

        [AllowAnonymous]
        public IActionResult downLoad()
        {
            return File(new FileStream(Path.Combine(TwinkleContext.AppRoot, "excelDemo", "导入测试.xlsx"), FileMode.Open), "application/octet-stream", "导入测试.xlsx");
        }
    }
}