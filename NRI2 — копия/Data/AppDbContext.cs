using Microsoft.EntityFrameworkCore;
using NRI.Classes;
using NRI.DB;
using NRI.DiceRoll;

namespace NRI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NRI;Trusted_Connection=True;")
                    .EnableSensitiveDataLogging();
            }
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CharacterSheet> Characters { get; set; }
        public DbSet<DiceRolling> DiceRolls { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffEvent> StaffEvents { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // 1. Сначала конфигурация таблиц без зависимостей
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleID);
                entity.HasData(
                    new Role { RoleID = 1, RoleName = "Игрок" },
                    new Role { RoleID = 2, RoleName = "Организатор" },
                    new Role { RoleID = 3, RoleName = "Администратор" }
                );
            });

            // 2. Конфигурация User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsBlocked).HasColumnName("user_blocked");
                entity.Property(e => e.IncorrectAttempts).HasColumnName("Incorrect_pass");
                entity.Property(e => e.BlockedUntil).HasColumnName("block_time");
                entity.Property(e => e.FullName).HasColumnName("Full_name");
                entity.Property(e => e.Phone).HasColumnName("Number_telephone");
                entity.Property(e => e.IsPasswordConfirmationRequired).HasColumnName("password_confirm");
                entity.Ignore(e => e.Roles);
                entity.Ignore(e => e.Token);
                entity.Ignore(e => e.TokenExpiry);
                entity.Ignore(e => e.IsTokenValid);
                entity.Ignore(e => e.PhoneNumber);

                entity.HasData(
                    new User
                    {
                        Id = 1,
                        Login = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                        FullName = "Администратор",
                        PhoneNumber = "+79001234567"
                    }
                );
            });

            // 3. Конфигурация DiceRolling (должна быть перед UserRole, так как не имеет зависимостей)
            modelBuilder.Entity<DiceRolling>(entity =>
            {
                entity.HasKey(d => d.RollId);
                entity.Property(d => d.RollId).ValueGeneratedOnAdd(); // Явно указываем автоинкремент

                // Для CharacterId делаем его необязательным
                entity.Property(d => d.CharacterId).IsRequired(false);

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Character)
                      .WithMany(c => c.DiceRolls)
                      .HasForeignKey(d => d.CharacterId)
                      .OnDelete(DeleteBehavior.SetNull);
            });


            // 4. Конфигурация CharacterSheet
            modelBuilder.Entity<CharacterSheet>(entity =>
            {
                entity.HasKey(e => e.CharacterID);
                entity.Property(e => e.CharacterID)
                              .ValueGeneratedOnAdd();

                // Настраиваем JSON-поля
                entity.Property(e => e.AttributesJson)
                    .HasColumnName("Attributes")
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.SkillsJson)
                    .HasColumnName("Skills")
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.InventoryJson)
                    .HasColumnName("Inventory")
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });



            // 6. Конфигурация UserRole (должна быть после User и Role)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserID, ur.RoleID });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasData(
                    new UserRole { UserRoleID = 1, UserID = 1, RoleID = 3 }
                );
            });

            // 7. Конфигурация EventParticipant
            modelBuilder.Entity<EventParticipant>(entity =>
            {
                entity.HasKey(e => e.ParticipationID);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasOne(ep => ep.Event)
                      .WithMany(e => e.Participants)
                      .HasForeignKey(ep => ep.EventID)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(ep => ep.User)
                      .WithMany(u => u.EventParticipants)
                      .HasForeignKey(ep => ep.UserID)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Ignore<ObservableKeyValuePair>();
        }
    }
}
