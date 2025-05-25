// DatabaseService.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NRI.Classes
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfigService _configService;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfigService configService, ILogger<DatabaseService> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка выполнения запроса: {query}");
                throw;
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        return await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка выполнения скалярного запроса: {query}");
                throw;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка выполнения non-query: {query}");
                throw;
            }
        }
    }
}
