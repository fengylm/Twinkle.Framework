using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinkle.Models
{
    public class Router
    {
        public string cUrl { get; set; }
        public string cPath { get; set; }
        public string cTitle { get; set; }
        public string cIcon { get; set; }
        public Router[] Children { get; set; }
    }
}
