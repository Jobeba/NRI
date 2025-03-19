using Moq;
using Xunit; // Пространство имен, где находится IDatabaseService
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace NRI.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetUserByLoginAsync_ShouldReturnUser_WhenCredentialsAreValid()
        {
            // Arrange
            var mockDatabaseService = new Mock<IDatabaseService>();
            var userRepository = new UserRepository(mockDatabaseService.Object);

            var expectedUser = new DataTable();
            expectedUser.Columns.Add("login");
            expectedUser.Columns.Add("password");
            expectedUser.Rows.Add("test", "hashed_password");

            mockDatabaseService
                .Setup(service => service.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await userRepository.GetUserByLoginAsync("test", "password");

            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Equals("test", result.Rows[0]["login"]);
        }
    }
}