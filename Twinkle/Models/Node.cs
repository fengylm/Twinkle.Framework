using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinkle.Models
{
    [Model("Node")]
    public class Node : BaseModel
    {
        public string label { get; set; }
        public Boolean? expand { get; set; }
        public List<Node> children { get; set; }
        public double? id { get; set; }
        public string cCode { get; set; }
        public bool? @checked { get; set; }
        public bool? leaf { get; set; }
        public string cField { get; set; }
        public string key { get; set; }
    }
}
