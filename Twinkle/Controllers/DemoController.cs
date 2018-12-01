using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Twinkle.Framework.File;
using Twinkle.Framework.Import;
using Twinkle.Framework.Mvc;
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

        #region 导入测试
        [AllowAnonymous]
        public async Task Upload(UploadFileArgs args)
        {
            DataImport si = DataImportFactory.CreateDataImport();
            si.Init(args.FileStream, "testTable", new Mapping[] {
                new Mapping{ DBColumn="GUID",Macro=Macro.Guid,Type= DataType.String },
                new Mapping{ DBColumn="INPUTDATE",Macro=Macro.Now,Type= DataType.String },
                new Mapping{ DBColumn="TYPE",Macro=Macro.Default,Value=1,Key=true,Type= DataType.Number }
            });

            await si.ExcuteAsync();
        }
        #endregion

        [AllowAnonymous]
        public IActionResult downLoad(ClientModel client)
        {
            string code = client.GetString("code");

            return WebHelper.DownLoad(Path.Combine(TwinkleContext.AppRoot, "excelDemo", "导入测试.xlsx"));

        }
        [AllowAnonymous]
        public string Settings()
        {
            return "\ndata[0]=new Object();\ndata[0][\"tag\"] = \"tw-table\";\ndata[0][\"data\"]=new Object();\ndata[0][\"data\"][\"props\"]=new Object();\ndata[0][\"data\"][\"props\"][\"autoLoad\"] = true;\ndata[0][\"data\"][\"props\"][\"cellStyle\"]=new Object();\ndata[0][\"data\"][\"props\"][\"cellStyle\"][\"color\"] = \"red\";\ndata[0][\"data\"][\"props\"][\"index\"] = true;\ndata[0][\"data\"][\"props\"][\"selection\"] = true;\ndata[0][\"data\"][\"props\"][\"expand\"] = true;\ndata[0][\"data\"][\"props\"][\"paging\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"]=new Array();\ndata[0][\"data\"][\"props\"][\"columns\"][0]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"label\"] = \"数据测试\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"align\"] = \"center\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"]=new Array();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"prop\"] = \"datetime\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"label\"] = \"日期时间\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"sortable\"] = \"custom\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"attrs\"][\"type\"] = \"datetime\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][0][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"prop\"] = \"date\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"label\"] = \"日期\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"sortable\"] = \"custom\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"attrs\"][\"type\"] = \"date\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][1][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"prop\"] = \"datetime\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"label\"] = \"时间\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"attrs\"][\"type\"] = \"time\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][2][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"prop\"] = \"number\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"label\"] = \"数字\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"type\"] = \"number\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"precision\"] = 2;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"min\"] = -10;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"max\"] = 20;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][3][\"attrs\"][\"summary\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"prop\"] = \"select\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"label\"] = \"下拉\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"type\"] = \"select\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"]=new Array();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][0]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][0][\"value\"] = \"1\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][0][\"label\"] = \"男\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][1]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][1][\"value\"] = \"0\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"options\"][1][\"label\"] = \"女\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"value\"] = \"value\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][4][\"attrs\"][\"label\"] = \"label\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"prop\"] = \"select\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"label\"] = \"布尔\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"width\"] = 180;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"attrs\"][\"type\"] = \"boolean\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][5][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"prop\"] = \"string\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"label\"] = \"文本\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"attrs\"][\"type\"] = \"string\";\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"attrs\"][\"maxlength\"] = 20;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"attrs\"][\"minlength\"] = 3;\ndata[0][\"data\"][\"props\"][\"columns\"][0][\"columns\"][6][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"columns\"][1]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"label\"] = \"操作\";\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"fixed\"] = \"left\";\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"align\"] = \"center\";\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"width\"] = 200;\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"attrs\"]=new Object();\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"attrs\"][\"type\"] = \"string\";\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"attrs\"][\"maxlength\"] = 20;\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"attrs\"][\"minlength\"] = 3;\ndata[0][\"data\"][\"props\"][\"columns\"][1][\"attrs\"][\"editable\"] = true;\ndata[0][\"data\"][\"props\"][\"url\"] = \"Home/GetData\";\ndata[0][\"data\"][\"props\"][\"showSummary\"] = true;\ndata[0][\"children\"]=new Array();\ndata[0][\"children\"][0]=new Object();\ndata[0][\"children\"][0][\"tag\"] = \"div\";\ndata[0][\"children\"][0][\"data\"]=new Object();\ndata[0][\"children\"][0][\"data\"][\"slot\"] = \"leftTools\";\ndata[0][\"children\"][0][\"children\"]=new Array();\ndata[0][\"children\"][0][\"children\"][0]=new Object();\ndata[0][\"children\"][0][\"children\"][0][\"tag\"] = \"el-button\";\ndata[0][\"children\"][0][\"children\"][0][\"data\"]=new Object();\ndata[0][\"children\"][0][\"children\"][0][\"data\"][\"props\"]=new Object();\ndata[0][\"children\"][0][\"children\"][0][\"data\"][\"props\"][\"type\"] = \"primary\";\ndata[0][\"children\"][0][\"children\"][0][\"data\"][\"props\"][\"loading\"] = me.data.loading;\ndata[0][\"children\"][0][\"children\"][0][\"data\"][\"on\"]=new Object();\ndata[0][\"children\"][0][\"children\"][0][\"data\"][\"on\"][\"click\"] = function click() {\n                  me.data.loading = true;\n                  console.log(me.data);\n                };\ndata[0][\"children\"][0][\"children\"][0][\"children\"]=new Array();\ndata[0][\"children\"][0][\"children\"][0][\"children\"][0] = me.data.log;\ndata[0][\"children\"][0][\"children\"][1]=new Object();\ndata[0][\"children\"][0][\"children\"][1][\"tag\"] = \"el-button\";\ndata[0][\"children\"][0][\"children\"][1][\"data\"]=new Object();\ndata[0][\"children\"][0][\"children\"][1][\"data\"][\"props\"]=new Object();\ndata[0][\"children\"][0][\"children\"][1][\"data\"][\"props\"][\"type\"] = \"error\";\ndata[0][\"children\"][0][\"children\"][1][\"data\"][\"on\"]=new Object();\ndata[0][\"children\"][0][\"children\"][1][\"data\"][\"on\"][\"click\"] = function click() {\n                  me.data.loading = false;\n                };\ndata[0][\"children\"][0][\"children\"][1][\"children\"]=new Array();\ndata[0][\"children\"][0][\"children\"][1][\"children\"][0] = me.data.log;";
        }
    }
}