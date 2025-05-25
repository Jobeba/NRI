using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using Нотариус.Models;
using System.Data.SqlClient; // Используем SqlClient для SQL Server


namespace Нотариус
{
    public class DBNotary
    {
        private string connectionString = @"Server=your_server;Database=NotaryDB;User Id=your_username;Password=your_password;";
        private SqlConnection connection;
        private string log = "";

        public DBNotary()
        {
            connection = new SqlConnection(connectionString);
        }

        public string Log => log;

        public void RecreateDB()
        {
            log = "База данных 'NotaryDB' уже существует.\n";
        }

        #region Client
        public List<int> getClientId()
        {
            List<int> list = new List<int>();
            connection.Open();
            using (SqlCommand command = new SqlCommand(Client.SQLSelectId(), connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader.GetInt32(0)); // Используем GetInt32 для более безопасного получения значения
                }
            }
            connection.Close();
            return list;
        }

        public Client getClient(int id)
        {
            Client client = null;
            connection.Open();
            using (SqlCommand command = new SqlCommand(Client.SQLSelect(id), connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    client = new Client(reader);
                }
            }
            connection.Close();
            return client;
        }

        public int setClient(Client client)
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(client.SQLInsertOrUpdate(), connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        public int deleteClient(Client client)
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(client.SQLDelete(), connection))
            {
                return command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Deal
        public List<int> getDealId()
        {
            List<int> list = new List<int>();
            connection.Open();
            using (SqlCommand command = new SqlCommand(Deal.SQLSelectId(), connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader.GetInt32(0));
                }
            }
            connection.Close();
            return list;
        }

        public Deal getDeal(int id)
        {
            Deal deal = null;
            connection.Open();
            using (SqlCommand command = new SqlCommand(Deal.SQLSelect(id), connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    deal = new Deal(reader);
                }
            }

            if (deal != null)
            {
                // Загрузка связанных услуг
                using (SqlCommand command = new SqlCommand(deal.SQLServiceSelect(), connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deal.idServices.Add(reader.GetInt32(0));
                    }
                }

                // Загрузка связанных скидок
                using (SqlCommand command = new SqlCommand(deal.SQLDiscontSelect(), connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deal.idDisconts.Add(reader.GetInt32(0));
                    }
                }
            }

            connection.Close();
            return deal;
        }

        public int setDeal(Deal deal)
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(deal.SQLInsertOrUpdate(), connection))
            {
                command.ExecuteNonQuery();

                // Если это новая сделка, получаем её ID
                if (deal.id == -1)
                {
                    deal.id = getDealId().Max();
                }

                // Удаление связанных данных
                new SqlCommand(deal.SQLDiscontDeleteAll(), connection).ExecuteNonQuery();
                new SqlCommand(deal.SQLServiceDeleteAll(), connection).ExecuteNonQuery();

                // Вставка связанных данных
                new SqlCommand(deal.SQLServiceInsert(), connection).ExecuteNonQuery();
                new SqlCommand(deal.SQLDiscontInsert(), connection).ExecuteNonQuery();
            }
            connection.Close();
            return deal.id;
        }
        #endregion
    }
}
