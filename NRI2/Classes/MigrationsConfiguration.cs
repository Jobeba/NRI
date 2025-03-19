using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using NRI.Models;

namespace NRI
{
    public class MigrationsConfiguration : IDesignTimeDbContextFactory<EventContext>
    {
        public EventContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventContext>();
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-FLP8EG6\\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False");

            return new EventContext(optionsBuilder.Options);
        }
    }
}