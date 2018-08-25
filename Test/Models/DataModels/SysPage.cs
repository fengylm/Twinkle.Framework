using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Models.DataModels
{
    [Table("Sys_Page")]
    public partial class SysPage: IDbTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string CCode { get; set; }
        public int? NValue { get; set; }
        public DateTime? DDate { get; set; }
    }
}
