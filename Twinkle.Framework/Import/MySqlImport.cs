using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Twinkle.Framework.Import
{
    public sealed class MySqlImport : DataImport
    {
        internal MySqlImport(string databaseName = "") : base(databaseName) { }
        protected override IDbTransaction DBTrans { get; set; }
        protected override IDbCommand DBCmd { get; set; }

        protected override void SubmitDatabase(DataTable source, ImportConfig config, out string temptable)
        {
            temptable = $"{config.TableName}_Import";

            string tmpPath = Path.GetTempFileName();
            string csv = DataTableToCsv(source);
            System.IO.File.WriteAllText(tmpPath, csv);


            //创建临时表
            MySqlCommand cmd = DBConn.CreateCommand() as MySqlCommand;

            cmd.Transaction = DBTrans as MySqlTransaction;


            cmd.CommandText = $"CREATE TEMPORARY TABLE  IF NOT EXISTS {temptable}  SELECT {string.Join(',', Fields.ToArray())}  FROM {config.TableName} WHERE 1=0";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"TRUNCATE TABLE {temptable} ";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();


            MySqlBulkLoader bulk = new MySqlBulkLoader(DBConn as MySqlConnection)
            {
                FieldTerminator = ",",
                FieldQuotationCharacter = '"',
                EscapeCharacter = '"',
                LineTerminator = "\r\n",
                FileName = tmpPath,
                NumberOfLinesToSkip = 0,
                TableName = temptable,
            };
            bulk.Columns.AddRange(config.Mappings.Select(m => m.DBColumn).ToArray());
            bulk.Load();
            System.IO.File.Delete(tmpPath);
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

            cmd.CommandText = $"DELETE FROM {config.TableName} WHERE EXISTS (SELECT 1 FROM {temptable} WHERE 1=1 {GetCondition(temptable, config)})";
            affectCount = cmd.ExecuteNonQuery();

            cmd.CommandText = $"INSERT INTO {config.TableName} ({string.Join(',', Fields.ToArray())}) SELECT  {string.Join(',', Fields.ToArray())} FROM {temptable} ";
            cmd.ExecuteNonQuery();

        }

        protected override void UpdateStrategy(string temptable, ImportConfig config, out int affectCount)
        {
            IDbCommand cmd = DBConn.CreateCommand();
            cmd.Transaction = DBTrans;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $@"UPDATE {config.TableName},{temptable}  SET {string.Join(",", Fields.Except(Keys).Select(p => $"{config.TableName}.{p}={temptable}.{p}"))} 
                               WHERE 1=1 {GetCondition(temptable, config)}";

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

        private static string DataTableToCsv(DataTable table)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(row[colum].ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }
}
