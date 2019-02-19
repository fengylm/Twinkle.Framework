namespace Twinkle.Models
{
    public class Sys_Module : BaseModel
    {
        [ModelProperty(Identity = true)]
        public double? ID { get; set; }

        [ModelProperty(Key = true)]
        public string cCode { get; set; }

        public string cTitle { get; set; }

        public string cRoute { get; set; }

        public string cPath { get; set; }

        public string cIcon { get; set; }

        public int? nPID { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public int? iStatus { get; set; }
        public string cModuleCode { get; set; }
        [ModelProperty(Virtual = true)]
        public int? iHasRole { get; set; }
    }
}
