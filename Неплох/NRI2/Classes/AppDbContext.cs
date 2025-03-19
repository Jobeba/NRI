using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NRI.Models;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);
    }

    // Конструктор для использования Dependency Injection
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

public class EventContext : DbContext
{
    private readonly IConfiguration _configuration;

    public EventContext(DbContextOptions<EventContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    public DbSet<Event> Events { get; set; }
}