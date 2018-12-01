using Microsoft.Extensions.Configuration;
using System.Data;
using Twinkle.Framework.Extensions;

namespace Twinkle.Framework.Import
{
    public abstract class DataImportFactory
    {
        public static DataImport CreateDataImport(string databaseName = "")
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = TwinkleContext.Config.GetValue<string>("ConnectionStrings:DefaultDatabase");
            }
            string providerName = TwinkleContext.Config.GetValue<string>($"ConnectionStrings:{databaseName}:ProviderName");
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
