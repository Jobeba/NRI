// AppDbContext.cs
using Microsoft.EntityFrameworkCore;


namespace NRI.API
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Добавьте конфигурацию модели при необходимости
    }
}

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public DateTime LastActivity { get; set; }
}
