using System;

namespace Twinkle.Models
{
    public class Sys_User : BaseModel
    {
        [ModelProperty(Identity = true, Key = true)]
        public double? ID { get; set; }

        public string TenantId { get; set; }

        public string UserId { get; set; }

        public string cNonceStr { get; set; }

        public string cName { get; set; }

        public string cEmail { get; set; }

        public int? iSex { get; set; }

        public string cPhone { get; set; }

        public string cAddress { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public string cPassword { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public string cOpenId { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public DateTime? dCreatedDate { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public int? iEnabled { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public string cLoginIP { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public DateTime? dLoginDate { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public DateTime? dUnlockDate { get; set; }

        [ModelProperty(OnlyInsert = true)]
        public int? nFailedCount { get; set; }


    }
}
