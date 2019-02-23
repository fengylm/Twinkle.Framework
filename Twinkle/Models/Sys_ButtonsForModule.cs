using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinkle.Models
{
    public class Sys_ButtonsForModule : BaseModel
    {
        [ModelProperty(Key = true, Identity = true)]
        public int? ID { get; set; }
        public string cModuleCode { get; set; }
        public string cModuleName { get; set; }
        public string cButtonID { get; set; }
        public string cButtonName { get; set; }
    }
}
