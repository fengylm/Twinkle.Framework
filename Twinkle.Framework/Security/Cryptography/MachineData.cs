using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Twinkle.Framework.Security.Cryptography
{
    /// <summary>
    /// 机器信息,目前在非window系统下还无法获取
    /// </summary>
    public class MachineData
    {
        /// <summary>
        /// 根据字符串创建Hash编码
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string CreateHash(string plainText)
        {
            HashAlgorithm hasher = new MD5CryptoServiceProvider();

            byte[] data = hasher.ComputeHash(Encoding.Default.GetBytes(plainText));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 获取c盘序列号
        /// </summary>
        /// <returns></returns>
        public static string GetDiskVolumeSerialNumber()
        {
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        /// <summary>
        /// 获得CPU编码
        /// </summary>
        /// <returns></returns>
        public static string GetCpu()
        {
            string strCpu = string.Empty;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuConnection)
            {
                strCpu += myObject.Properties["Processorid"].Value.ToString();
            }
            return strCpu;
        }

        /// <summary>
        /// 获取机器码
        /// </summary>
        /// <returns></returns>
        public static string MachineCode()
        {
            string code = GetCpu() + "|{A5B417F8-4A1D-411C-A972-6B559BDCB715}";

            code = CreateHash(code).ToUpper();

            #region 格式化机器码
            string machineCode = string.Empty;
            for (int i = 0; i < code.Length; i++)
            {
                machineCode += code[i];
                if ((i + 1) % 5 == 0 && i != 26)
                {
                    machineCode += "-";
                }
            }
            return machineCode;
            #endregion

        }

        public static (DateTime? ValidityDate, bool HasError) AppPeriod(string code)
        {
            DateTime validityDate = DateTime.MinValue;

            try
            {
                string licence = DataCipher.RSADecrypt(code, "MIIEpQIBAAKCAQEAmlx+ouP4MJehomQOn+8YqjBUnX3oUVXDR2R3I4HdC0QUG9Qq0565n1fPl3TZLdS3njamNtUMu9Ovjl2bI0/oRyv536J4px4QDKGrB78PRbLC/jIq+Nuk2V3ObEPXJA8EAnSrdGGqn3rb4fejZgCAKashTp96VD+SKbhaCk3kTbVL9TMIyCDTv9/QjK3xKSFxlq2x3bnt/hqTUMHveTcE93qFDpEV2jtNbUz1oT43//J/wvIFIHFU+Xd5CjEYqeo0gaX0uzt2oAODljHP7ce1R+d1Gt6ab2kYfKE2t5beXQhEETsWcAm3U1nmq4d/YNXoE2RwXNfQzFr01LsFmFks4QIDAQABAoIBAQCH9FNtMJCWa9bm6m2fG72DmBHOrOiDSqA2Lxxn36dKlumHITWfMbuXxoWGhloKbcJTzRpUTQ1sGSQUpglP7r9MgEUSXU0bE/VaysGSjwrqfmoT7SfAC+SDtoVrAc1pavhDGXMxjBv1XwZvXedCncpD6P/q+beKoHsh18cmMDiW32sIUap75GOM5UEUJ8ira/jE5OuKY2H3G8OnISRbIkGEb9P/0YM/WRSbQibeVvW98gmV+t7t580MFjISDC0iqCgpxgF5jxDaRcKgLAE77YHx5f22IWMF4E75vWk4g2JBdUy8MwIqE3aknZc8rQEkkhu7u2KOnHEDhdi4Oz+6gKuBAoGBAMsltiXELPM+qjo24diMOBp433nOEovKptL4Buokv8BJrm0aKpMjfo7ZXCTYa9pgYsaUwppMdKkSCttWHvw1w6N9CNQVZhmTi5AEkiLfzx2K9yZ96zmWg5V1+hd+EAXDIVO+60Ue97PEhpY1D8mggIU9kh0301Y4n2W6qLHpp6CJAoGBAMKFeP0/DN3UQase003K74NWwwgoI91y6PmpRsM/GFiv0EvLCpN+AvOViLBaapXNcEWlsu4lf/98ypFENXQIjPNO1ER1VE9/hRoQvQVbUu0o5aET8DBXBgISfiEETa8KgEsHh3MFzyghBXv4LI3tKpYj/2kErnnZkm+l6vGdm6OZAoGAJsPpgEdxNHGu1jEG4+XOBu8t5fZ2/4oKT5PY7fFZTf5BdLxbh3xseCHuPXG3ExL1hmN4xyzzzheNTtGeVA6GaLpBZwc2VocbSL42jMUcpsyP4R6CNpkMPwcmVDlQIWldALgb+TKxnfJQpHU3sAgavlJDgaPXhkqD0EvQSTMHM9ECgYEAkKwATilkgl9o78IfWc5C0KXoq9pewkbCa1ywmmoEy4EHJDfAh+3CeDOQ08iLWRrQE5ynNWOCjRvc9KmML95kJllMmhXBPNcUwUwNqTxAss4l6uUPUISInWXvlNLEjBj9TdAttyhs5+WXVJpBWNU5RS1EunCW610cAhmeYVDc/pECgYEAsXu1EaxGO4wj92/C7nzOnPgMs3CwlmOJ5njiOzs9olASV5CgV6J2NA4Lq2mAjALuZe0tNm5ox8XR2HlZU760uE86CvPGhyvsUAcNV+dmN2VhKgES7vPF6pT3j7yD1xkAAvNxDs1Zvz+CSpdfhuYiBl0a8KGlU/dFr49KL5SC460=");

                string[] subs = licence.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (subs.Length < 2 || subs[0] != MachineCode())
                {
                    return (validityDate, true);
                }
                else
                {
                    return (Convert.ToDateTime(subs[1]), false);
                }
            }
            catch
            {
                return (validityDate, true);
            }


        }
    }
}
