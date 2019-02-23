﻿using System;
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

        public string cSummaryType { get; set; }
        public string cColumnRender { get; set; }
        public string cHeaderRender { get; set; }
        public double? nOrderID { get; set; }
    }
}