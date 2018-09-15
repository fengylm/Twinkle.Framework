using Aspose.Cells;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Twinkle.Framework.File
{
    public class DataReader
    {
        /// <summary>
        /// 通过文件流得到DataTable
        /// </summary>
        /// <param name="Stream">文件流</param>
        /// <param name="Separator">分隔符</param>
        /// <returns></returns>
        public static DataTable ReadEntireDataTable(Stream Stream, string Separator=",")
        {
            Workbook workbook;
            if (string.IsNullOrEmpty(Separator))
            {
                workbook = new Workbook(Stream);
            }
            else
            {
                workbook = new Workbook(Stream, new TxtLoadOptions { Encoding = Encoding.Default, ConvertNumericData = true, Separator = Separator[0] });
            }
            return ExportDataTable(workbook);
        }

        /// <summary>
        /// 通过文件流得到DataTable
        /// </summary>
        /// <param name="Stream">文件流</param>
        /// <returns></returns>
        public static DataTable ReadEntireDataTable(Stream Stream)
        {
            Workbook workbook;
            try
            {
                workbook = new Workbook(Stream);
            }
            catch (CellsException)//牺牲性能,增加部分容错,在转换遇到cellsException的时候,尝试以文本文件的方式解析
            {
                workbook = new Workbook(Stream, new TxtLoadOptions { Encoding = Encoding.Default, ConvertNumericData = true, Separator = ',' });
            }
            return ExportDataTable(workbook);
        }

        /// <summary>
        /// 通过文件路径得到DataTable
        /// </summary>
        /// <param name="File">带后缀名的完整文件路径</param>
        /// <param name="Separator">如果文件是文本文件,需要提供分隔符</param>
        /// <returns></returns>
        public static DataTable ReadEntireDataTable(string File, string Separator = ",")
        {
            Workbook workbook;
            if (new string[] { "txt", "csv" }.Contains(File.Substring(File.LastIndexOf(".")+1).ToLower()))
            {
                workbook = new Workbook(File, new TxtLoadOptions { Encoding = Encoding.Default, ConvertNumericData = true, Separator = Separator[0] });
            }
            else
            {
                workbook = new Workbook(File);
            }
            return ExportDataTable(workbook);
        }

        private static DataTable ExportDataTable(Workbook workbook)
        {
            return workbook.Worksheets[0].Cells.ExportDataTableAsString(0, 0, workbook.Worksheets[0].Cells.MaxDataRow+1, workbook.Worksheets[0].Cells.MaxDataColumn+1, true);
        }
    }
}
