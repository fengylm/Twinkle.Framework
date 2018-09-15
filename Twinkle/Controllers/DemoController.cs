using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.AspNetCore.Mvc;
using Twinkle.Framework.File;
using Twinkle.Framework.Import;
using Twinkle.Framework.Mvc;

namespace Twinkle.Controllers
{
    public class DemoController : Controller
    {
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

        public JsonResult ImportC()
        {
            ImportConfig ic = new ImportConfig();
            ic.Strategy = Strategy.Cover;
            ic.TableName = "testTable";
            ic.Mappings = new Mapping[] {
                new Mapping{ Key=false,DBColumn="name",FileColumn="姓名",Type= DataType.String },
                new Mapping{ Key=false,DBColumn="age",FileColumn="年1龄",Type= DataType.Number },
                new Mapping{ Key=false,DBColumn="birthday",FileColumn="生日",Type= DataType.Date }
            };

            string path = Path.Combine(TwinkleContext.AppRoot, "excelDemo");
            string filePath = Path.Combine(path, "导入测试.xlsx");
            AbsImport si = new SqlImport();
            si.StatusReport += Si_StatusReport;
            si.RowCheck += Si_RowCheck;
            si.Init(System.IO.File.OpenRead(filePath), "testTable", new Mapping[] {
                new Mapping{ DBColumn="GUID",Macro=Macro.Guid },
                new Mapping{ DBColumn="INPUTDATE",Macro=Macro.Now,Type= DataType.Date },
                new Mapping{ DBColumn="TYPE",Macro=Macro.Default,Value=1,Key=true }
            }).ExcuteAsync();

            return Json(ic);
        }

        private void Si_RowCheck(AbsImport arg1, DataRow arg2, ImportConfig arg3)
        {
            arg1.WarningReport(new ReportArgs { Message = "哎呀哎呀,么得命咯" });
        }

        private void Si_StatusReport(ReportArgs obj)
        {
            Console.WriteLine(obj.Message);
        }
    }
}