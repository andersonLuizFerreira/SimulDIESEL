using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public interface IBdServiceProvider
    {
        int ExecuteNonQuery(string sql, IEnumerable<BdCommandParameter> parameters = null);

        object ExecuteScalar(string sql, IEnumerable<BdCommandParameter> parameters = null);

        T ExecuteScalar<T>(string sql, IEnumerable<BdCommandParameter> parameters = null);

        IReadOnlyList<T> Query<T>(string sql, Func<DbDataReader, T> map, IEnumerable<BdCommandParameter> parameters = null);

        T InTransaction<T>(Func<IBdServiceProvider, T> action);

        void InTransaction(Action<IBdServiceProvider> action);
    }
}
