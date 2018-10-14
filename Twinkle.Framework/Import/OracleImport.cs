using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Twinkle.Framework.Import
{
    public sealed class OracleImport : DataImport
    {
        internal OracleImport(string databaseName = "") : base(databaseName) { }
        protected override IDbTransaction DBTrans { get; set; }
        protected override IDbCommand DBCmd { get; set; }

        protected override void SubmitDatabase(DataTable source, ImportConfig config, out string temptable)
        {
            temptable = $"{config.TableName}_Import";

            //创建临时表
            OracleCommand cmd = DBConn.CreateCommand() as OracleCommand;
            cmd.Transaction = DBTrans as OracleTransaction;
            cmd.CommandText = $"SELECT COUNT(1) FROM USER_TABLES WHERE TABLE_NAME = UPPER('{temptable}')";
            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
            {
                //删除已存在的临时表
                cmd.CommandText = $"DROP TABLE {temptable}";
                cmd.ExecuteNonQuery();
            }

            cmd.CommandText = $"CREATE GLOBAL TEMPORARY TABLE {temptable} ON COMMIT DELETE ROWS AS SELECT {string.Join(',', Fields.ToArray())}  FROM {config.TableName} WHERE 1=0";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();


            //批量插入oracle
            cmd.ArrayBindCount = source.Rows.Count;
            cmd.CommandText = $"INSERT INTO {temptable}({string.Join(',', Fields.ToArray())})VALUES({string.Join(',', Fields.Select(p => config.Mappings.Where(c=>c.DBColumn==p).FirstOrDefault().Type== DataType.Date?$"to_date(:{p},'yyyy-mm-dd hh24:mi:ss')":$":{p}"))})";


            for (int i = 0; i < source.Columns.Count; i++)
            {
                object[] value = new object[source.Rows.Count];
                for (int j = 0; j < source.Rows.Count; j++)
                {
                    try
                    {
                        value[j] = source.Rows[j][i];
                    }
                    catch (Exception ex)
                    {
                        throw new ImportException(ex.Message);
                    }
                }

                OracleParameter param = CreateParameter(config.Mappings[i]);
                param.Direction = ParameterDirection.Input;
                param.Value = value;
                cmd.Parameters.Add(param);
            }

            cmd.ExecuteNonQuery();
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

            cmd.CommandText = $@"UPDATE {config.TableName} SET ({string.Join(",", Fields.Except(Keys).Select(p => $"{p}"))})
                               = (SELECT {string.Join(",", Fields.Except(Keys).Select(p => $"{temptable}.{p}"))} FROM {temptable} WHERE 1=1 {GetCondition(temptable, config)})
                               WHERE EXISTS (SELECT 1 FROM {temptable} WHERE 1=1 {GetCondition(temptable, config)})";

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

        private string GetUpdateField(string temptable, ImportConfig config)
        {
            return string.Join(",", Fields.Except(Keys).Select(p => $"{p}={temptable}.{p}"));
        }

        private OracleParameter CreateParameter(Mapping mapping)
        {
            OracleDbType type = OracleDbType.Varchar2;
            switch (mapping.Type)
            {
                case DataType.Date:
                    type = OracleDbType.Varchar2;//对于日期格式 使用varchar2的方式传值,通过to_date函数转义
                    break;
                case DataType.Number:
                    type = OracleDbType.Double;
                    break;
                default:
                    type = OracleDbType.Varchar2;
                    break;
            }

            return new OracleParameter(mapping.DBColumn, type);
        }
    }
}
