using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twinkle.Framework.Mvc
{
    public class ClientModel
    {
        /// <summary>
        /// 获取整型参数
        /// </summary>
        /// <param name="key">参数名,多阶参数用冒号分割</param>
        /// <returns></returns>
        public int? GetInt(string key)
        {
            object value = GetValue(key);
            if (value == null)
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(value);
            }

        }

        /// <summary>
        /// 获取字符串类型参数
        /// </summary>
        /// <param name="key">参数名,多阶参数用冒号分割</param>
        /// <returns></returns>
        public string GetString(string key)
        {
            return GetValue(key)?.ToString();
        }

        /// <summary>
        /// 获取双精度类型参数
        /// </summary>
        /// <param name="key">参数名,多阶参数用冒号分割</param>
        /// <returns></returns>
        public double? GetDouble(string key)
        {
            object value = GetValue(key);
            if (value == null)
            {
                return null;
            }
            else
            {
                return Convert.ToDouble(value);
            }
        }

        /// <summary>
        /// 获取日期类型参数
        /// </summary>
        /// <param name="key">参数名,多阶参数用冒号分割</param>
        /// <returns></returns>
        public DateTime? GetDateTime(string key)
        {
            object value = GetValue(key);
            if (value == null)
            {
                return null;
            }
            else
            {
                return Convert.ToDateTime(value);
            }
        }

        /// <summary>
        /// 获取实体类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">参数名,多阶参数用冒号分割</param>
        /// <returns></returns>
        public T GetEntity<T>(string key) where T : class, new()
        {
            return JToken.Parse(GetValue(key)?.ToString()).ToObject<T>();
        }

        private object GetValue(string key)
        {
            List<string> keys = key.Split(":", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (ClientParams == null)
            {
                return null;
            }
            JToken result = ClientParams;
            while(keys.Count>0)
            {
                result = result[keys[0]];
                keys.RemoveAt(0);
            }
            return result;
        }

        private static JToken FindChild(JObject Parent, List<string> keys)
        {
            if (Parent.TryGetValue(keys[0], StringComparison.InvariantCultureIgnoreCase, out JToken child))
            {
                if (keys.Count > 1)
                {
                    keys.RemoveAt(0);
                    return FindChild(child as JObject, keys);
                }
                else
                {
                    return child;
                }
            }
            return null;
        }

        private JObject clientParams = null;
        private JObject ClientParams
        {
            get
            {
                if (clientParams == null)
                {
                    JObject query = new JObject() ;
                    try
                    {
                        //获取浏览器参数,get和post都会有
                        foreach (var item in TwinkleContext.MvcHttpContext.Request.Query)
                        {
                            query.Add(new JProperty(item.Key.ToString(), item.Value.ToString()));
                        }

                        if (TwinkleContext.MvcHttpContext.Request.Method.Equals("POST",StringComparison.OrdinalIgnoreCase))
                        {
                            //POST 提交时,从Form获得表单参数
                            if (TwinkleContext.MvcHttpContext.Request.HasFormContentType)
                            {
                                foreach (var item in TwinkleContext.MvcHttpContext.Request.Form)
                                {
                                    query.Add(new JProperty(item.Key.ToString(), item.Value.ToString()));
                                }
                            }
                            else
                            {
                                //POST 提交时,从body获取提交参数
                                int bodyLength = (int)TwinkleContext.MvcHttpContext.Request.ContentLength;
                                if (bodyLength > 0)
                                {
                                    byte[] buffer = new byte[bodyLength];
                                    TwinkleContext.MvcHttpContext.Request.Body.Read(buffer, 0, buffer.Length);
                                    var paramObj = JToken.Parse(Encoding.UTF8.GetString(buffer));
                                    foreach (var item in paramObj)
                                    {
                                        query.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {

                        throw ex;
                    }
                    clientParams = query;
                }

                return clientParams;
            }
        }
    }
}
