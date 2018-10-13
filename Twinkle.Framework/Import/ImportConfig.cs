using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twinkle.Framework.Import
{
    /// <summary>
    /// 导入映射文件
    /// </summary>
    public class ImportConfig
    {
        /// <summary>
        /// 存在性处理策略
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Strategy Strategy { get; set; } = Strategy.Cover;

        /// <summary>
        /// 要导入的表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 数据库文件映射
        /// </summary>
        public Mapping[] Mappings { get; set; }
    }

    public class Mapping
    {
        /// <summary>
        /// 数据库字段
        /// </summary>
        public string DBColumn { get; set; }

        /// <summary>
        /// 文件字段
        /// </summary>
        public string FileColumn { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 宏 设置为非None的时候,FileColumn无效,设置为default的时候,取Value字段的固定值
        /// </summary>
        public Macro Macro { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataType Type { get; set; } = DataType.String;

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool Key { get; set; }

        /// <summary>
        /// 是否允许空 默认true
        /// </summary>
        public bool AllowNull { get; set; } = true;
    }

    public enum DataType
    {
        String,
        Date,
        Number
    }

    public enum Strategy
    {
        /// <summary>
        /// 不采用策略:尝试继续导入到数据库(可能会导致异常,异常后事务还原)
        /// </summary>
        None,
        /// <summary>
        /// 覆盖策略:删除原数据,导入新数据
        /// </summary>
        Cover,
        /// <summary>
        /// 终止策略:停止导入(事务还原)
        /// </summary>
        Abort,
        /// <summary>
        /// 更新策略:更新主键以外的所有导入字段栏位
        /// </summary>
        Update
    }

    public enum Macro
    {
        None,
        Guid,
        Now,
        Default
    }
}
