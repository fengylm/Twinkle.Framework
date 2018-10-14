using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Twinkle.Framework.Import
{
    public sealed class SqlImport : DataImport
    {
        internal SqlImport(string databaseName = "") : base(databaseName) { }
        protected override IDbTransaction DBTrans { get; set; }
        protected override IDbCommand DBCmd { get; set; }

        protected override void SubmitDatabase(DataTable source, ImportConfig config, out string temptable)
        {
            temptable = $"#{config.TableName}_Import_tmp";

            //创建临时表
            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandText = $"SELECT {string.Join(',', Fields.ToArray())} INTO {temptable} FROM {config.TableName} WHERE 1=0";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();

            //使用SqlBulkCopy批量提交
            using (SqlBulkCopy sqlBC = new SqlBulkCopy(DBConn as SqlConnection, SqlBulkCopyOptions.CheckConstraints, DBTrans as SqlTransaction))
            {
                sqlBC.BatchSize = source.Rows.Count;
                sqlBC.BulkCopyTimeout = 60;
                sqlBC.DestinationTableName = temptable;

                foreach (var item in config.Mappings)
                {
                    sqlBC.ColumnMappings.Add(item.FileColumn, item.DBColumn);
                }

                sqlBC.WriteToServer(source);
            }
        }

        protected override void AbortStrategy(string temptable, ImportConfig config, out int affectCount)
        {
            string checkSql = $@"SELECT COUNT(1) FROM {temptable} WHERE EXISTS (SELECT 1 FROM {config.TableName} WHERE 1=1 {GetCondition(temptable, config)})";

            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = checkSql;

            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
            {
                affectCount = 0;
            }
            else
            {
                CommonStrategy(temptable, config, out affectCount);
            }
        }

        protected override void CommonStrategy(string temptable, ImportConfig config, out int affectCount)
        {
            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $"INSERT INTO {config.TableName} ({string.Join(',', Fields.ToArray())}) SELECT  {string.Join(',', Fields.ToArray())} FROM {temptable} ";
            affectCount = cmd.ExecuteNonQuery();
        }

        protected override void CoverStrategy(string temptable, ImportConfig config, out int affectCount)
        {
            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $"DELETE {config.TableName} WHERE EXISTS (SELECT 1 FROM {temptable} WHERE 1=1 {GetCondition(temptable, config)})";
            affectCount = cmd.ExecuteNonQuery();

            cmd.CommandText = $"INSERT INTO {config.TableName} ({string.Join(',', Fields.ToArray())}) SELECT  {string.Join(',', Fields.ToArray())} FROM {temptable} ";
            cmd.ExecuteNonQuery();

        }

        protected override void UpdateStrategy(string temptable, ImportConfig config, out int affectCount)
        {
            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $@"UPDATE {config.TableName} SET {string.Join(",", Fields.Except(Keys).Select(p => $"{p}={temptable}.{p}"))} FROM {config.TableName} 
                               INNER JOIN {temptable} ON 1=1 {GetCondition(temptable, config)}";

            affectCount = cmd.ExecuteNonQuery();

            cmd.CommandText = $@"INSERT INTO {config.TableName} ({string.Join(',', Fields.ToArray())}) 
                                  SELECT  {string.Join(',', Fields.ToArray())} FROM {temptable} WHERE NOT EXISTS (SELECT 1 FROM {config.TableName} WHERE 1=1 {GetCondition(temptable, config)})";
            cmd.ExecuteNonQuery();
        }

        private string GetCondition(string temptable, ImportConfig config)
        {
            string condition = string.Join(" AND ", Keys.Select(p => $"{temptable}.{p}={config.TableName}.{p}"));
            if (!string.IsNullOrEmpty(condition))
            {
                condition = " AND " + condition;
            }
            return condition;
        }
    }
}
