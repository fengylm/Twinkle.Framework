using Dapper;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Twinkle.Framework.Mvc;
using System.Data.Common;
using Twinkle.Framework.Security;

namespace Twinkle.Framework.Database
{
    public class DatabaseManager
    {
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
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }
                    if (Connection != null)
                    {
                        if (Connection.State == ConnectionState.Open)
                        {
                            Connection.Close();
                        }
                        Connection.Dispose();
                        Connection = null;
                    }
                }

                //释放非托管代码

                m_disposed = true;
            }
        }

        ~DatabaseManager()
        {
            Dispose(false);
        }
        #endregion

        #region database对象
        /// <summary>
        /// 创建数据库访问实例
        /// </summary>
        /// <param name="databaseName">配置文件key</param>
        public DatabaseManager(string databaseName = "")
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = TwinkleContext.Config.GetValue<string>("ConnectionStrings:DefaultDatabase");
            }
            CreateConnection(databaseName);
        }

        /// <summary>
        /// 创建数据库访问实例
        /// </summary>
        /// <param name="connectionString">访问连接字符串</param>
        /// <param name="provider">数据提供管道,可以是静态类SqlClientFactory,OracleClientFactory,MySqlClientFactory 等的Instance对象</param>
        public DatabaseManager(string connectionString, DbProviderFactory provider)
        {
            IDbConnection connection = provider.CreateConnection();

            connection.ConnectionString = connectionString;

            this.Connection = connection;
        }
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection Connection { get; set; }

        /// <summary>
        /// 数据库事务对象
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        /// 通过配置文件中的ProviderName来自动生成Connection对象
        /// </summary>
        /// <param name="databaseName">在appsetting中配置的数据库连接对象名称</param>
        /// <returns></returns>
        public IDbConnection CreateConnection(string databaseName)
        {
            bool encrypt = TwinkleContext.Config.GetValue<bool>($"ConnectionStrings:{databaseName}:Encrypt");

            string connectString = TwinkleContext.Config.GetValue<string>($"ConnectionStrings:{databaseName}:ConnectString");

            //如果连接字符串被加密了 需要解密
            if (encrypt)
            {
                connectString = DataProtection.RSADecrypt(connectString);
            }

            string providerName = TwinkleContext.Config.GetValue<string>($"ConnectionStrings:{databaseName}:ProviderName");

            IDbConnection connection = null;
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    connection = SqlClientFactory.Instance.CreateConnection();
                    break;
                case "Oracle.ManagedDataAccess.Client":
                    connection = new OracleClientFactory().CreateConnection();
                    break;
                case "MySql.Data.MySqlClient":
                    connection = new MySqlClientFactory().CreateConnection();
                    break;
                default:
                    throw new DataException("There is no suitable Provider for creating IDbConnection");
            }

            connection.ConnectionString = connectString;

            this.Connection = connection;
            return connection;
        }
        #endregion

        #region 事务
        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new DataException("There is already a transaction in progress that cannot start a new transaction");
            }
            else
            {
                if (Connection != null && Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
                else
                {
                    throw new DataException("Open Connection failed");
                }

                Transaction = Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            if (Transaction != null && Transaction.Connection.State == ConnectionState.Open)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
            else
            {
                throw new DataException("There is no transaction to be committed");
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null && Transaction.Connection.State == ConnectionState.Open)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            }
            else
            {

            }
        }
        #endregion

        #region 同步数据库操作
        /// <summary>
        /// 非查询操作
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回收影响的行数(对于非数据操作,比如建表,删除表等,返回永远是-1)</returns>
        public int ExecuteNonQuery(string sql, object parameters = null)
        {
            try
            {
                return Connection.Execute(sql, parameters, Transaction);
            }
            catch (DataException e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 返回第一行第一列的数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回objec对象</returns>
        public object ExecuteScalar(string sql, object parameters = null)
        {
            try
            {
                return Connection.ExecuteScalar(sql, parameters, Transaction);
            }
            catch (DataException e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 返回第一行第一列的整型数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回int类型</returns>
        public int ExecuteInteger(string sql, object parameters = null)
        {
            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        /// <summary>
        /// 返回第一行第一列的浮点型数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回double类型</returns>
        public double ExecuteDouble(string sql, object parameters = null)
        {
            return Convert.ToDouble(ExecuteScalar(sql, parameters));
        }

        /// <summary>
        /// 返回第一行第一列的日期类型数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回datetime类型</returns>
        public DateTime ExecuteDateTime(string sql, object parameters = null)
        {
            return Convert.ToDateTime(ExecuteScalar(sql, parameters));
        }

        /// <summary>
        /// 返回指定类型对象
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回T对应的实体对象</returns>
        public T ExecuteEntity<T>(string sql, object parameters = null)
        {
            return ExecuteEntities<T>(sql, parameters).FirstOrDefault();
        }

        /// <summary>
        /// 返回指定类型对象集合
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回T对应的实体对象集合</returns>
        public List<T> ExecuteEntities<T>(string sql, object parameters = null)
        {
            try
            {
                return Connection.Query<T>(sql, parameters, Transaction).AsList();
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 返回DataTable对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回DataTable对象</returns>
        public DataTable ExecuteDataTable(string sql, object parameters = null)
        {
            try
            {
                using (IDataReader reader = this.Connection.ExecuteReader(sql, parameters, Transaction))
                {
                    DataTable result = new DataTable();

                    int intFieldCount = reader.FieldCount;
                    for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                    {
                        result.Columns.Add(reader.GetName(intCounter).ToLower(), reader.GetFieldType(intCounter));
                    }
                    result.BeginLoadData();
                    object[] objValues = new object[intFieldCount];
                    while (reader.Read())
                    {
                        reader.GetValues(objValues);
                        result.LoadDataRow(objValues, true);
                    }
                    reader.Close();
                    reader.Dispose();
                    result.EndLoadData();
                    return result;
                }
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }
        /// <summary>
        /// 返回IDataReader对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数,示例 new {param1=value1,param2=value2}</param>
        /// <returns>返回IDataReader对象</returns>
        public IDataReader ExecuteReader(string sql, object parameters = null)
        {
            try
            {
                return Connection.ExecuteReader(sql, parameters, Transaction);
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 执行无返回参数的存储过程
        /// </summary>
        /// <param name="sql">存储过程名称</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>返回结果集(非返回参数)</returns>
        public int ExecuteProcedureNonQuery(string sql, DataParameters parameters = null)
        {
            try
            {
                return Connection.Execute(sql, parameters?.GetDynamicParameters(), Transaction, null, CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 执行带返回结果集的存储过程
        /// </summary>
        /// <param name="sql">存储过程名称</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>返回结果集</returns>
        public IList<T> ExecuteProcedure<T>(string sql, DataParameters parameters = null)
        {
            try
            {
                return Connection.Query<T>(sql, parameters?.GetDynamicParameters(), Transaction, true, null, CommandType.StoredProcedure).ToList();
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }

        /// <summary>
        /// 执行带返回参数的存储过程
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>返回值字典集合</returns>
        public IDictionary<string, object> ExecuteProcedure(string sql, DataParameters parameters = null)
        {
            try
            {
                Connection.Execute(sql, parameters?.GetDynamicParameters(), Transaction, null, CommandType.StoredProcedure);
                return parameters.GetOutput();
            }
            catch (Exception e)
            {
                Rollback();//异常时候回滚,防止数据库锁死
                throw new DataException(e.Message);
            }
        }
        #endregion
    }

    public class DataParameters
    {
        private DynamicParameters _params;
        private IList<string> _outputlst;
        public DataParameters()
        {
            _params = new DynamicParameters();
            _outputlst = new List<string>();
        }
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="direction">参数方向</param>
        public void Add(string paramName, object value, DbType? dbType, Direction? direction)
        {
            _params.Add(paramName, value, dbType, (ParameterDirection)direction, 2000);
            if (direction == Direction.Output)
            {
                _outputlst.Add(paramName);
            }
        }
        /// <summary>
        /// 获取返回的参数集合(如果有)
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetOutput()
        {
            IDictionary<string, object> dstOutput = new Dictionary<string, object>();
            foreach (var item in _outputlst)
            {
                dstOutput.Add(item, _params.Get<object>(item));
            }
            return dstOutput;
        }

        internal DynamicParameters GetDynamicParameters()
        {
            return _params;
        }

    }

    public enum Direction
    {
        Input = 1,
        Output = 2
    }
}
