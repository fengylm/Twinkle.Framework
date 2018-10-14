using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.Import
{
    public abstract class DataImportFactory
    {
        public static DataImport CreateDataImport(string databaseName = "")
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = TwinkleContext.AppConfig.GetValue<string>("ConnectionStrings:DefaultDatabase");
            }
            string providerName = TwinkleContext.AppConfig.GetValue<string>($"ConnectionStrings:{databaseName}:ProviderName");
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return new SqlImport(databaseName);
                case "Oracle.ManagedDataAccess.Client":
                    return new OracleImport(databaseName);
                case "MySql.Data.MySqlClient":
                    return new MySqlImport(databaseName);
                default:
                    throw new DataException("There is no suitable Provider for creating IDbConnection");
            }

        }
    }
}
