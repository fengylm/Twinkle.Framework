namespace Twinkle.Models
{
    public class Sys_Role : BaseModel
    {
        [ModelProperty(Identity = true)]
        public double? ID { get; set; }

        [ModelProperty(Key = true)]
        public string TenantId { get; set; }

        [ModelProperty(Key = true)]
        public string cCode { get; set; }

        public string cTitle { get; set; }

        public string cDesc { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public int? iStatus { get; set; }

    }
}
