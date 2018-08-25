using System;
using System.Collections.Generic;

namespace Test.Models.DataModels
{
    public partial class SysUser: IDbTable
    {
        public int Id { get; set; }
        public string CName { get; set; }
        public DateTime? DDate { get; set; }
        public int? NAge { get; set; }
    }
}
