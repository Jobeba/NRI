using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public interface IDatabaseService
    {
        Task ExecuteNonQueryAsync(string query, params SqlParameter[] parameters);
        Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters);
        Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters);
    }
}
