using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinkle.Framework.Database;
using Twinkle.Framework.File;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.Import
{
    public abstract class AbsImport : IDisposable
    {
        public AbsImport(string databaseName = "")
        {
            this.databaseName = databaseName;
        }
        #region 属性
        private string databaseName { get; }
        //导入文件生成的数据源
        private DataTable source { get; set; }

        //导入配置文件
        private ImportConfig config { get; set; }

        //数据库连接接口
        protected IDbConnection DBConn { get; private set; }

        //事务接口
        protected abstract IDbTransaction DBTrans { get; set; }

        //命令操作接口
        protected abstract IDbCommand DBCmd { get; set; }

        /// <summary>
        /// 主键字段
        /// </summary>
        protected List<string> Keys => config.Mappings.Where(p => p.Key == true).Select(p => p.DBColumn).ToList();

        /// <summary>
        /// 已经隐射的数据库字段
        /// </summary>
        protected List<string> Fields => config.Mappings.Select(p => p.DBColumn).ToList();

        //记录警告数量,警告数量大于0的时候,不会写入到数据库
        private int WarningCount { get; set; } = 0;
        #endregion

        #region 事件
        public event Action<ReportArgs> StatusReport;

        public event Action<AbsImport, DataRow, ImportConfig> RowCheck;

        /// <summary>
        /// 异常信息报告,报告完后直接终止导入的所有操作
        /// </summary>
        /// <param name="ex"></param>
        public void ErrorReport(ReportArgs args)
        {
            Rollback();
            StatusReport?.Invoke(args);
        }

        /// <summary>
        /// 普通消息报告,不影响操作流程
        /// </summary>
        /// <param name="args"></param>
        public void InfoReport(ReportArgs args)
        {
            StatusReport?.Invoke(args);
        }

        /// <summary>
        /// 警告信息报告,产生警告信息后,除非手动调用ErrorReport 否则直到执行异常才会退出
        /// </summary>
        /// <param name="args"></param>
        public void WarningReport(ReportArgs args)
        {
            WarningCount++;
            StatusReport?.Invoke(args);
        }

        #endregion

        #region 私有方法
        //加载配置
        private void LoadConfig(string configName, Mapping[] mappings)
        {

            InfoReport(new ReportArgs { Message = "加载导入配置文件..." });
            string path = Path.Combine(TwinkleContext.AppRoot, "ImportConfig", $"{configName}.json");
            config = JToken.Parse(System.IO.File.ReadAllText(path, Encoding.UTF8)).ToObject<ImportConfig>();
            if (config != null)
            {
                if (mappings != null && mappings.Length > 0)
                {
                    List<Mapping> lstMapping = new List<Mapping>();
                    lstMapping.AddRange(mappings);
                    lstMapping.AddRange(config.Mappings);
                    config.Mappings = lstMapping.ToArray();
                }

                config.Mappings = config.Mappings.OrderBy(item => item.Macro).ToArray();
            }
            else
            {
                throw new ImportException("加载导入配置文件失败.");
            }
        }

        //处理数据源
        private void DealSource()
        {
            InfoReport(new ReportArgs { Message = "执行数据检测..." });

            foreach (var item in config.Mappings)
            {
                if (item.Macro != Macro.None)
                {
                    source.Columns.Add(new DataColumn(item.DBColumn, MappingType(item)));
                }
                else
                {
                    if (!source.Columns.Contains(item.FileColumn))
                    {
                        WarningReport(new ReportArgs
                        {
                            Message = $"列[{item.FileColumn}]不存在"
                        });
                    }
                }
            }

            if (WarningCount > 0)
            {
                throw new ImportException("数据列校验异常,导入终止");
            }

            source = source.DefaultView.ToTable(false, config.Mappings.Select(p => string.IsNullOrEmpty(p.FileColumn) ? p.DBColumn : p.FileColumn).ToArray());

            foreach (DataRow row in source.Rows)
            {
                foreach (var item in config.Mappings.Where(p => p.Macro != Macro.None))
                {
                    row[item.DBColumn] = MacroValue(item);
                }
                RowCheck?.Invoke(this, row, config);
            }
            if (WarningCount > 0)
            {
                throw new ImportException("数据检测异常,导入终止");
            }
        }

        //导入宏值处理
        private object MacroValue(Mapping mapping)
        {
            switch (mapping.Macro)
            {
                case Macro.Now:
                    return DateTime.Now;
                case Macro.Guid:
                    return Guid.NewGuid().ToString("N");
                case Macro.Default:
                    return mapping.Value;
                default:
                    return null;
            }
        }

        //导入类型转c#类型
        private Type MappingType(Mapping mapping)
        {
            switch (mapping.Type)
            {
                case DataType.Date:
                    return typeof(DateTime);
                case DataType.Number:
                    return typeof(Double);
                default:
                    return typeof(String);
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 初始化导入
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="configName">配置名称</param>
        /// <param name="mappings">自定义导入映射</param>
        public AbsImport Init(Stream fileStream, string configName, Mapping[] mappings = null)
        {
            InfoReport(new ReportArgs { Message = "读取导入文件信息..." });
            source = DataReader.ReadEntireDataTable(fileStream);

            LoadConfig(configName, mappings);
            return this;
        }

        /// <summary>
        /// 执行导入
        /// </summary>
        /// <param name="DatabaseName">数据库连接字符串</param>
        /// <returns></returns>
        public Task ExcuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    DBConn = new DatabaseManager(databaseName).Connection;
                    DBConn.Open();
                    DBTrans = DBConn.BeginTransaction();

                    DealSource();
                    InfoReport(new ReportArgs { Message = "开始提交数据..." });
                    string temptable;
                    SubmitDatabase(source, config, out temptable);

                    InfoReport(new ReportArgs { Message = "提交成功,执行冲突策略校验..." });
                    switch (config.Strategy)
                    {
                        case Strategy.None:
                            {
                                CommonStrategy(temptable, config, out int affectCount);
                                InfoReport(new ReportArgs { Message = $"启用默认策略,导入{affectCount}条数据" });
                            }
                            break;
                        case Strategy.Abort:
                            {
                                AbortStrategy(temptable, config, out int affectCount);
                                InfoReport(new ReportArgs { Message = $"启用终止策略,导入{affectCount}条数据" });
                            }
                            break;
                        case Strategy.Cover:
                            {
                                CoverStrategy(temptable, config, out int affectCount);
                                InfoReport(new ReportArgs { Message = $"启用覆盖策略,导入{source.Rows.Count}条数据,覆盖{affectCount}条历史数据." });
                            }
                            break;
                        case Strategy.Update:
                            {
                                UpdateStrategy(temptable, config, out int affectCount);
                                InfoReport(new ReportArgs { Message = $"启用更新策略,导入{source.Rows.Count - affectCount}条数据,更新{affectCount}条历史数据." });
                            }
                            break;
                    }
                    Commit();
                }
                catch (Exception ex)
                {
                    ErrorReport(new ReportArgs { Message = ex.Message });
                }
            });
        }
        #endregion

        #region 抽象方法
        /// <summary>
        /// 提交到数据操作
        /// </summary>
        /// <param name="source"></param>
        protected abstract void SubmitDatabase(DataTable source, ImportConfig config, out string temptable);

        /// <summary>
        /// 无策略提交
        /// </summary>
        protected abstract void CommonStrategy(string temptable, ImportConfig config, out int affectCount);

        /// <summary>
        /// 覆盖策略提交
        /// </summary>
        protected abstract void CoverStrategy(string temptable, ImportConfig config, out int affectCount);

        /// <summary>
        /// 终端策略提交
        /// </summary>
        protected abstract void AbortStrategy(string temptable, ImportConfig config, out int affectCount);

        /// <summary>
        /// 更新策略提交
        /// </summary>
        protected abstract void UpdateStrategy(string temptable, ImportConfig config, out int affectCount);

        #endregion

        #region 事务处理
        /// <summary>
        /// 回滚数据库操作
        /// </summary>
        protected void Rollback()
        {
            if (this.DBTrans != null)
            {
                DBTrans.Rollback();
                DBTrans.Dispose();
                DBTrans = null;

            }
            if (DBConn.State == ConnectionState.Open)
            {
                DBConn.Close();
            }

            DBConn.Dispose();
            DBConn = null;
        }

        /// <summary>
        /// 提交数据库操作
        /// </summary>
        protected void Commit()
        {
            if (this.DBTrans != null)
            {
                DBTrans.Commit();
                DBTrans.Dispose();
                DBTrans = null;

            }
            if (DBConn.State == ConnectionState.Open)
            {
                DBConn.Close();
            }

            DBConn.Dispose();
            DBConn = null;
        }
        #endregion

        #region 资源释放
        private bool m_disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // 释放托管代码
                    if (DBTrans != null)
                    {
                        DBTrans.Dispose();
                        DBTrans = null;
                    }
                    if (DBConn != null)
                    {
                        if (DBConn.State == ConnectionState.Open)
                        {
                            DBConn.Close();
                        }
                        DBConn.Dispose();
                        DBConn = null;
                    }
                }

                //释放非托管代码

                m_disposed = true;
            }
        }

        ~AbsImport()
        {
            Dispose(false);
        }
        #endregion
    }

    public class ReportArgs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class ImportException : Exception
    {
        public ImportException(string message) : base(message) { }
    }
}
