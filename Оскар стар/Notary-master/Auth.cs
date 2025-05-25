using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Нотариус
{
    public class Auth
    {
        public enum UserType : ushort
        {
            Admin = 0,
            User = 1,
            None = 2,
        }

        public UserType userType = UserType.None;

        private string connectionString;

        public Auth(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Auth(string connectionString, string login, string password) : this(connectionString)
        {
            Authenticate(login, password);
        }

        private void Authenticate(string login, string password)
        {
            // Хешируем пароль
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] bytePassword = Encoding.ASCII.GetBytes(password);
            byte[] hashPassword = sha.ComputeHash(bytePassword);
            string hashPasswordString = Convert.ToBase64String(hashPassword);

            // Подключаемся к базе данных и проверяем учетные данные
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserType FROM Users WHERE Login = @login AND PasswordHash = @passwordHash";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@passwordHash", hashPasswordString);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        if (result.ToString() == "Admin")
                            userType = UserType.Admin;
                        else if (result.ToString() == "User")
                            userType = UserType.User;
                        else
                            userType = UserType.None;
                    }
                    else
                    {
                        userType = UserType.None;
                    }
                }
            }
        }
    }
}
