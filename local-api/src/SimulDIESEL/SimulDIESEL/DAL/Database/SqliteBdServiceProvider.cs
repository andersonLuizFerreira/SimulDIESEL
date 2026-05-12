using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public sealed class SqliteBdServiceProvider : IBdServiceProvider
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SqliteBdServiceProvider(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public int ExecuteNonQuery(string sql, IEnumerable<BdCommandParameter> parameters = null)
        {
            return Execute(connection =>
            {
                using (var command = CreateCommand(connection, null, sql, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            });
        }

        public object ExecuteScalar(string sql, IEnumerable<BdCommandParameter> parameters = null)
        {
            return Execute(connection =>
            {
                using (var command = CreateCommand(connection, null, sql, parameters))
                {
                    return command.ExecuteScalar();
                }
            });
        }

        public T ExecuteScalar<T>(string sql, IEnumerable<BdCommandParameter> parameters = null)
        {
            var value = ExecuteScalar(sql, parameters);
            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public IReadOnlyList<T> Query<T>(string sql, Func<DbDataReader, T> map, IEnumerable<BdCommandParameter> parameters = null)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            return Execute(connection =>
            {
                var results = new List<T>();
                using (var command = CreateCommand(connection, null, sql, parameters))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(map(reader));
                    }
                }

                return (IReadOnlyList<T>)results;
            });
        }

        public T InTransaction<T>(Func<IBdServiceProvider, T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var connection = _connectionFactory.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var scopedProvider = new TransactionScopedProvider(connection, transaction);
                try
                {
                    var result = action(scopedProvider);
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void InTransaction(Action<IBdServiceProvider> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            InTransaction<object>(provider =>
            {
                action(provider);
                return null;
            });
        }

        private T Execute<T>(Func<DbConnection, T> action)
        {
            using (var connection = _connectionFactory.CreateOpenConnection())
            {
                return action(connection);
            }
        }

        private static DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string sql, IEnumerable<BdCommandParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL command text is required.", nameof(sql));
            }

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = parameter.Name;
                    dbParameter.Value = parameter.Value ?? DBNull.Value;
                    command.Parameters.Add(dbParameter);
                }
            }

            return command;
        }

        private sealed class TransactionScopedProvider : IBdServiceProvider
        {
            private readonly DbConnection _connection;
            private readonly DbTransaction _transaction;

            public TransactionScopedProvider(DbConnection connection, DbTransaction transaction)
            {
                _connection = connection;
                _transaction = transaction;
            }

            public int ExecuteNonQuery(string sql, IEnumerable<BdCommandParameter> parameters = null)
            {
                using (var command = CreateCommand(_connection, _transaction, sql, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            }

            public object ExecuteScalar(string sql, IEnumerable<BdCommandParameter> parameters = null)
            {
                using (var command = CreateCommand(_connection, _transaction, sql, parameters))
                {
                    return command.ExecuteScalar();
                }
            }

            public T ExecuteScalar<T>(string sql, IEnumerable<BdCommandParameter> parameters = null)
            {
                var value = ExecuteScalar(sql, parameters);
                if (value == null || value == DBNull.Value)
                {
                    return default(T);
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }

            public IReadOnlyList<T> Query<T>(string sql, Func<DbDataReader, T> map, IEnumerable<BdCommandParameter> parameters = null)
            {
                if (map == null)
                {
                    throw new ArgumentNullException(nameof(map));
                }

                var results = new List<T>();
                using (var command = CreateCommand(_connection, _transaction, sql, parameters))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(map(reader));
                    }
                }

                return results;
            }

            public T InTransaction<T>(Func<IBdServiceProvider, T> action)
            {
                return action(this);
            }

            public void InTransaction(Action<IBdServiceProvider> action)
            {
                action(this);
            }
        }
    }
}
