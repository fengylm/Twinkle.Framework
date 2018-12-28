using Microsoft.AspNetCore.Mvc;
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Twinkle.Framework.Security.Cryptography;

namespace Twinkle.Controllers
{
    public class HomeController : Controller
    {
        public string Index()
        {
            string licence = DataCipher.RSAEncrypt(MachineData.MachineCode()+"|"+DateTime.Now.AddDays(20).ToString("yyyy-MM-dd"), "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmlx+ouP4MJehomQOn+8YqjBUnX3oUVXDR2R3I4HdC0QUG9Qq0565n1fPl3TZLdS3njamNtUMu9Ovjl2bI0/oRyv536J4px4QDKGrB78PRbLC/jIq+Nuk2V3ObEPXJA8EAnSrdGGqn3rb4fejZgCAKashTp96VD+SKbhaCk3kTbVL9TMIyCDTv9/QjK3xKSFxlq2x3bnt/hqTUMHveTcE93qFDpEV2jtNbUz1oT43//J/wvIFIHFU+Xd5CjEYqeo0gaX0uzt2oAODljHP7ce1R+d1Gt6ab2kYfKE2t5beXQhEETsWcAm3U1nmq4d/YNXoE2RwXNfQzFr01LsFmFks4QIDAQAB");

            var a = MachineData.AppPeriod(licence);
            return a.ValidityDate.ToString();
        }
    }
}