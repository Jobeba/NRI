using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    internal class DateBase
    {
       // public SqlConnection sqlConnection = new SqlConnection(@"Data Source=209-U\SQLEXPRESS209;Initial Catalog = Kasyanov_NRI;User ID=guest;Password = 0000");
        public SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-FLP8EG6\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False");
        public void Open()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
               sqlConnection.Open();
            }

        }
        public void Close()
        {

            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection Get()
        {
            return sqlConnection;
        }
    }
}
