using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NRI.Classes
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private string connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public DatabaseService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    var adapter = new SqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<object> ExecuteScalarAsync(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return await command.ExecuteScalarAsync();
                }
            }
        }
    }
}