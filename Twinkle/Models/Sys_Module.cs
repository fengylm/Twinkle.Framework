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

        public int? cPID { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public int? iStatus { get; set; }
    }
}
