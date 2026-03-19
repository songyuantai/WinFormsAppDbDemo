using System.Data;
using System.Data.Common;

namespace WinFormsAppDbDemo
{
    /// <summary>
    /// 支持的数据库类型
    /// </summary>
    public enum DatabaseProvider
    {
        SqlServer,
        MySql,
        Oracle,
        PostgreSQL
    }

    /// <summary>
    /// 通用数据库访问帮助类（非静态，参数使用 DbParameter）
    /// </summary>
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _factory;

        /// <summary>
        /// 初始化 DatabaseHelper 实例
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="provider">数据库类型</param>
        public DatabaseHelper(string connectionString, DatabaseProvider provider)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _factory = GetDbProviderFactory(provider);
        }

        #region 私有辅助方法

        private DbConnection? CreateConnection()
        {
            return _factory.CreateConnection();
        }

        private static DbProviderFactory GetDbProviderFactory(DatabaseProvider provider)
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    // 使用 Microsoft.Data.SqlClient 或 System.Data.SqlClient
                    return Microsoft.Data.SqlClient.SqlClientFactory.Instance;
                case DatabaseProvider.MySql:
                    // 使用 MySql.Data 或 MySqlConnector
                    return MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                case DatabaseProvider.Oracle:
                    // 使用 Oracle.ManagedDataAccess.Core
                    return Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance;
                case DatabaseProvider.PostgreSQL:
                    // 使用 Npgsql
                    return Npgsql.NpgsqlFactory.Instance;
                default:
                    throw new NotSupportedException($"不支持的数据库类型: {provider}");
            }
        }

        #endregion

        #region 同步方法（自动管理连接）

        public DbParameter? CreateDbParameter(string name,
            object value,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = _factory?.CreateParameter();
            if (null == parameter) return null;
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.Direction = direction;
            return parameter;
        }

        /// <summary>
        /// 执行非查询 SQL（INSERT、UPDATE、DELETE），返回受影响行数
        /// </summary>
        /// <param name="sql">SQL 语句或存储过程名称</param>
        /// <param name="commandType">命令类型（默认 Text）</param>
        /// <param name="parameters">参数数组（具体类型需与数据库匹配）</param>
        public int ExecuteNonQuery(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询，返回第一行第一列的值（object 类型）
        /// </summary>
        public object ExecuteScalar(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            connection.Open();
            return command.ExecuteScalar();
        }

        /// <summary>
        /// 执行查询，返回第一行第一列的强类型值
        /// </summary>
        public T ExecuteScalar<T>(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            object value = ExecuteScalar(sql, commandType, parameters);
            return value == null || value == DBNull.Value ? default : (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// 执行查询，返回 DataTable
        /// </summary>
        public DataTable ExecuteDataTable(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            using var adapter = _factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// 执行查询，返回 DbDataReader
        /// 注意：调用者必须负责关闭 Reader（可使用 using 语句），连接将在 Reader 关闭时自动释放（CommandBehavior.CloseConnection）
        /// </summary>
        public DbDataReader ExecuteReader(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            try
            {
                connection.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                connection.Dispose();
                command.Dispose();
                throw;
            }
        }

        #endregion

        #region 异步方法（自动管理连接）

        public async Task<int> ExecuteNonQueryAsync(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            await connection.OpenAsync().ConfigureAwait(false);
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<object> ExecuteScalarAsync(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            await connection.OpenAsync().ConfigureAwait(false);
            return await command.ExecuteScalarAsync().ConfigureAwait(false);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            object value = await ExecuteScalarAsync(sql, commandType, parameters).ConfigureAwait(false);
            return value == null || value == DBNull.Value ? default : (T)Convert.ChangeType(value, typeof(T));
        }

        public async Task<DataTable> ExecuteDataTableAsync(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            using var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            using var adapter = _factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            // DataAdapter 没有标准的 FillAsync，此处用 Task.Run 模拟（实际可改用同步）
            await Task.Run(() => adapter.Fill(dt)).ConfigureAwait(false);
            return dt;
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
            }
            catch
            {
                connection.Dispose();
                command.Dispose();
                throw;
            }
        }

        #endregion

        #region 支持事务的重载（接受外部连接和事务）

        /// <summary>
        /// 在已有事务中执行非查询命令（同步）
        /// </summary>
        /// <param name="connection">已打开的连接（从事务中获得）</param>
        /// <param name="transaction">当前事务</param>
        /// <param name="sql">SQL 语句或存储过程名称</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parameters">参数数组</param>
        public int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }

        public object ExecuteScalar(DbConnection connection, DbTransaction transaction, string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            return command.ExecuteScalar();
        }

        public DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = commandType;
            command.CommandText = sql;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            using var adapter = _factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        // 异步事务重载可类似实现，此处省略

        #endregion

        #region 简化事务执行的辅助方法

        /// <summary>
        /// 在一个事务中执行多个操作（使用当前实例的连接字符串）
        /// </summary>
        /// <param name="action">需要执行的操作，参数为 DatabaseHelper 实例、连接和事务</param>
        public void ExecuteInTransaction(Action<DatabaseHelper, DbConnection, DbTransaction> action)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                action(this, connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<DatabaseHelper, DbConnection, DbTransaction, Task> asyncAction)
        {
            using var connection = CreateConnection();
            connection.ConnectionString = _connectionString;
            await connection.OpenAsync().ConfigureAwait(false);
            using var transaction = connection.BeginTransaction();
            try
            {
                await asyncAction(this, connection, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion
    }
}
