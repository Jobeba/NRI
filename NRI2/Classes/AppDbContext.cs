using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NRI.Models;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    // Конструктор для использования Dependency Injection
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

namespace NRI.Models
{
    public class EventContext : DbContext
    {
        public DbSet<Events> Events { get; set; }

        public EventContext(DbContextOptions<EventContext> options)
          : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("DefaultConnection");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Events>()
                .HasKey(e => e.EventID);
        }
    }
}