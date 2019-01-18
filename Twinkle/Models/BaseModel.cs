using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;

namespace Twinkle.Models
{
    public class BaseModel
    {
        #region 操作库
        /// <summary>
        /// 检测实体在数据库中是否存在(根据主键判断)
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            IEnumerable<KeyValuePair<string, PropertyInfo>> keys = this.Keys();
            if (keys.Count() == 0)
            {
                throw new KeyNotFoundException("未找到主键信息,无法操作");
            }

            string sql = $"SELECT COUNT(1) FROM {this.GetTableName()} WHERE 1=1 {BuildCondition(keys)}";
            if (TwinkleContext.GetRequiredService<DatabaseManager>().ExecuteInteger(sql, BuildDataParameter(keys)) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <returns></returns>
        public int Delete()
        {

            IEnumerable<KeyValuePair<string, PropertyInfo>> keys = this.Keys();

            string sql = $"DELETE {this.GetTableName()} WHERE 1=1 {BuildCondition(keys)}";

            return TwinkleContext.GetRequiredService<DatabaseManager>().ExecuteNonQuery(sql, BuildDataParameter(keys));

        }
        /// <summary>
        /// 新增或修改实体
        /// </summary>
        public int InsertOrUpdate()
        {
            IEnumerable<KeyValuePair<string, PropertyInfo>> keys = this.Keys();
            IEnumerable<KeyValuePair<string, PropertyInfo>> generals = this.Generals();

            if (Exists())
            {
                return Update(keys, generals);
            }
            else
            {
                return Insert(keys, generals);
            }
        }
        /// <summary>
        /// 根据非主键信息创建赋值字符串
        /// </summary>
        /// <param name="generals">非主键集合</param>
        /// <returns></returns>
        private int Update(IEnumerable<KeyValuePair<string, PropertyInfo>> keys, IEnumerable<KeyValuePair<string, PropertyInfo>> generals)
        {
            if (keys.Count() == 0)
            {
                throw new KeyNotFoundException("无主键信息");
            }
            if (generals.Count() == 0)
            {
                throw new KeyNotFoundException("无字段信息");
            }


            string update = string.Empty;
            foreach (KeyValuePair<string, PropertyInfo> item in generals)
            {
                ModelPropertyAttribute mpa = item.Value.GetCustomAttribute<ModelPropertyAttribute>(false);
                if (mpa != null && (mpa.Identity || mpa.OnlyInsert))
                {
                    continue;
                }
                update += $", {item.Key}=@{item.Key}";
            }
            update = $"UPDATE {GetTableName()} SET {update.TrimStart(',')} WHERE 1=1 {BuildCondition(keys)}";

            //拼接主键和普通属性,并移除非Key的Identity属性
            IEnumerable<KeyValuePair<string, PropertyInfo>> allKeys = keys.Union(generals).Where(item =>
            {
                ModelPropertyAttribute mpa = item.Value.GetCustomAttribute<ModelPropertyAttribute>(false);
                if (mpa == null)
                {
                    return true;
                }
                else
                {
                    if (mpa.Identity && !mpa.Key)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            });

            return TwinkleContext.GetRequiredService<DatabaseManager>().ExecuteNonQuery(update, BuildDataParameter(allKeys));

        }
        /// <summary>
        /// 执行插入操作
        /// </summary>
        /// <param name="keys">主键集合</param>
        /// <param name="generals">非主键集合</param>
        /// <returns></returns>
        private int Insert(IEnumerable<KeyValuePair<string, PropertyInfo>> keys, IEnumerable<KeyValuePair<string, PropertyInfo>> generals)
        {

            PropertyInfo identityKey = null;
            //拼接主键和普通属性,并移除Identity属性
            IEnumerable<KeyValuePair<string, PropertyInfo>> allKey = keys.Union(generals).Where(item =>
            {
                ModelPropertyAttribute mpa = item.Value.GetCustomAttribute<ModelPropertyAttribute>(false);
                if (mpa == null)
                {
                    return true;
                }
                else if (mpa.Identity)
                {
                    identityKey = item.Value;
                    return false;
                }
                else
                {
                    return true;
                }
            });

            if (allKey.Count() == 0)
            {
                throw new KeyNotFoundException("无字段信息");
            }

            string insert = string.Empty;
            string insertValues = string.Empty;
            foreach (KeyValuePair<string, PropertyInfo> item in allKey)
            {
                insert += $",{item.Key}";
                insertValues += $",@{item.Key}";
            }

            insert = $"INSERT INTO {GetTableName()}({insert.TrimStart(',')}) VALUES({insertValues.TrimStart(',')});";
            insert += "SELECT @@Identity";//获取自增长列的值
            int result = TwinkleContext.GetRequiredService<DatabaseManager>().ExecuteInteger(insert, BuildDataParameter(allKey));
            if (identityKey != null)
            {
                identityKey.SetValue(this, Convert.ChangeType(result.ToString(), (Nullable.GetUnderlyingType(identityKey.PropertyType) == null ? identityKey.PropertyType : Nullable.GetUnderlyingType(identityKey.PropertyType))));
            }
            return result;
        }
        #endregion

        #region 创建条件&参数
        /// <summary>
        /// 根据主键信息创建检索条件字符串 不带where,and开头
        /// </summary>
        /// <param name="keys">主键集合</param>
        /// <returns></returns>
        private string BuildCondition(IEnumerable<KeyValuePair<string, PropertyInfo>> keys)
        {
            if (keys.Count() == 0)
            {
                throw new KeyNotFoundException("未找到主键信息,无法操作");
            }

            string condition = string.Empty;
            foreach (KeyValuePair<string, PropertyInfo> item in keys)
            {
                condition += $" AND {item.Key}=@{item.Key}";
            }
            return condition;
        }
        /// <summary>
        /// 创建参数集合
        /// </summary>
        /// <param name="propertys"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, object>> BuildDataParameter(IEnumerable<KeyValuePair<string, PropertyInfo>> propertys)
        {
            return propertys.Select(item =>
            {
                ModelPropertyAttribute mpa = item.Value.GetCustomAttribute<ModelPropertyAttribute>(false);
                if (mpa != null)
                {
                    if (mpa.Identity)
                    {
                        return new KeyValuePair<string, object>(item.Key, item.Value.GetValue(this) ?? -1);//自增列给与默认值-1
                    }
                    else if (mpa.Guid)
                    {
                        return new KeyValuePair<string, object>(item.Key, item.Value.GetValue(this) ?? Guid.NewGuid().ToString("N"));//Guid给与默认值
                    }
                    else
                    {
                        return new KeyValuePair<string, object>(item.Key, item.Value.GetValue(this));
                    }
                }
                else
                {
                    return new KeyValuePair<string, object>(item.Key, item.Value.GetValue(this));
                }
            });
        }
        #endregion

        #region 获取实体信息
        /// <summary>
        /// 获取主键属性集合
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, PropertyInfo>> Keys()
        {
            return Propertys(true);
        }
        /// <summary>
        /// 获取非主键属性集合
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, PropertyInfo>> Generals()
        {
            return Propertys(false);
        }
        /// <summary>
        /// 获取属性集合
        /// </summary>
        /// <param name="isKey"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, PropertyInfo>> Propertys(bool isKey)
        {
            PropertyInfo[] Properties = this.GetType().GetProperties();

            Dictionary<string, PropertyInfo> dict = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo prop in Properties)
            {
                ModelPropertyAttribute mpa = prop.GetCustomAttribute<ModelPropertyAttribute>(false);
                if (mpa != null)
                {
                    if (mpa.Virtual)
                    {
                        continue;//虚拟列不做处理
                    }

                    if (mpa.Identity && mpa.Guid)
                    {
                        throw new CustomAttributeFormatException($"属性{GetTableName()}.{prop.Name}不能既是Identity又是Guid");
                    }

                    if (mpa.Key == isKey)
                    {
                        dict.Add(prop.Name, prop);
                    }
                }
                else
                {
                    if (!isKey)//未添加ModelPropertyAttribute特性的属性都按照普通属性处理
                    {
                        dict.Add(prop.Name, prop);
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// 获取实体表名
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            ModelAttribute ma = this.GetType().GetCustomAttribute<ModelAttribute>(false);
            if (ma == null)
            {
                return this.GetType().Name;
            }
            else
            {
                return ma.TableName;
            }
        }
        #endregion

        #region 对象复制
        public object Clone()
        {
            return Clone(this, JObject.Parse(JObject.FromObject(this).ToString()));
        }

        private T Clone<T>(T model, JObject jobject)
        {
            return jobject.ToObject<T>();
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ModelPropertyAttribute : Attribute
    {
        /// <summary>
        /// 是否允许空 默认true
        /// </summary>
        public bool AllowNull { get; set; } = true;

        /// <summary>
        /// 是否虚拟列 不与数据库交互 默认false
        /// </summary>
        public bool Virtual { get; set; } = false;

        /// <summary>
        /// 仅插入时候操作 默认false
        /// </summary>
        public bool OnlyInsert { get; set; } = false;

        /// <summary>
        /// 是否主键 默认false
        /// </summary>
        public bool Key { get; set; } = false;

        /// <summary>
        /// 是否自增长列 默认false
        /// </summary>
        public bool Identity { get; set; } = false;

        /// <summary>
        /// 是否Guid列 默认false
        /// </summary>
        public bool Guid { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModelAttribute : Attribute
    {
        public ModelAttribute(string tableName)
        {
            this.TableName = tableName;
        }
        /// <summary>
        /// 实体对象对应的数据库表名
        /// </summary>
        public string TableName { get; set; }
    }
}
