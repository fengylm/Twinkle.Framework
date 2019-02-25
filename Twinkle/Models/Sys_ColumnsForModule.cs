using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinkle.Models
{
    public class Sys_ColumnsForModule : BaseModel
    {
        [ModelProperty(Key = true, Identity = true)]
        public int? ID { get; set; }
        public string cModuleCode { get; set; }
        public string cModuleName { get; set; }
        public string cField { get; set; }
        public string cTitle { get; set; }
        public string cDataType { get; set; }
        public int? nWidth { get; set; }
        public string cAlign { get; set; }
        public int? iShow { get; set; }
        public int? iSum { get; set; }
        public int? iSort { get; set; }
        public int? iFilter { get; set; }
        public int? iEdit { get; set; }
        public int? iSpan { get; set; }
        public int nPrecision { get; set; }
        public string cGroupHeader { get; set; }
        public string cFormatter { get; set; }
        public string cColumnRender { get; set; }
        public string cHeaderRender { get; set; }
        public double? nOrderID { get; set; }
        public string cChangeFn { get; set; }
        public string cFixed { get; set; }
    }
}
