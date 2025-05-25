// IDatabaseService.cs
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public interface IDatabaseService
    {
        Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null);
        Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null);
    }
}
