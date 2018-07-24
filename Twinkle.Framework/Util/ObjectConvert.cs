﻿using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace Twinkle.Framework.Util
{
    public static class ObjectConvert
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="Source">要被序列化的对象</param>
        /// <returns></returns>
        public static string FromObject(this object Source)
        {
            if (Source == null)
            {
                return "";
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (Source is DataSet)
            {
                using (StringWriter stringWriter = new StringWriter(stringBuilder))
                {
                    (Source as DataSet)?.WriteXml(stringWriter, XmlWriteMode.WriteSchema);
                }
                return stringBuilder.ToString();
            }
            else if (Source is DataTable)
            {
                using (StringWriter stringWriter = new StringWriter(stringBuilder))
                {
                    (Source as DataTable)?.WriteXml(stringWriter, XmlWriteMode.WriteSchema);
                }
                return stringBuilder.ToString();
            }
            else
            {
                return JToken.FromObject(Source).ToString();
            }
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="StrValue">要被反序列化的字符串</param>
        /// <returns></returns>
        public static T ToObject<T>(this string StrValue)
        {
            if (StrValue.Length == 0)
            {
                return default(T);
            }

            switch (typeof(T).Name)
            {
                case "DataSet":
                    using (StringReader streamRead = new StringReader(StrValue))
                    {
                        DataSet ds = new DataSet();
                        ds.ReadXml(streamRead);
                        return (T)(object)ds;
                    }
                case "DataTable":
                    using (StringReader streamRead = new StringReader(StrValue))
                    {
                        DataSet ds = new DataSet();
                        ds.ReadXml(streamRead);
                        return (T)(object)ds.Tables[0];
                    }
                case "DateTime":
                    if (DateTime.TryParse(StrValue, out DateTime result))
                    {
                        return (T)(object)result;
                    }
                    else
                    {
                        return default(T);
                    }

                default:
                    try
                    {
                        return JToken.Parse(StrValue).ToObject<T>();
                    }
                    catch
                    {
                        try
                        {
                            return (T)Convert.ChangeType(StrValue, typeof(T));
                        }
                        catch
                        {
                            return default(T);
                        }
                    }
            }

        }
    }
}
