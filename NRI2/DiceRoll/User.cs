using NRI.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

    namespace NRI.Classes
    {
        [Table("Users")]
        public class User : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column("UserID")]
            public int Id { get; set; }

            [Required]
            [Column("login")] // Изменено на имя столбца в БД
            [MaxLength(50)]
            public string Login { get; set; }
            public string Status { get; set; }

            private bool _isOnline;

            [NotMapped]
            public bool IsOnline => LastActivity.HasValue &&
                          (DateTime.UtcNow - LastActivity.Value) <= TimeSpan.FromMinutes(5);

            public void UpdateActivity()
            {
                LastActivity = DateTime.UtcNow;
                OnPropertyChanged(nameof(IsOnline));
            }

            public static class TimeInterval
            {
                public static TimeSpan OnlineThreshold = TimeSpan.FromMinutes(5);
            }

            [NotMapped]
            public DateTime RegistrationDate { get; set; }

            [Column("LastActivity")]
            public DateTime? LastActivity { get; set; }

            [Required]
            [Column("password")] // Изменено на имя столбца в БД
            [MaxLength(100)]
            public string PasswordHash { get; set; }

            [Column("TwoFactorSecret")]
            public string TwoFactorSecret { get; set; }

            [Column("user_blocked")] // Изменено на имя столбца в БД
            public bool IsBlocked { get; set; } = false;

            [Column("Incorrect_pass")] // Изменено на имя столбца в БД
            public int IncorrectAttempts { get; set; } = 0;

            [Column("block_time")] // Изменено на имя столбца в БД
            public DateTime? BlockedUntil { get; set; }

            [Column("Full_name")] // Изменено на имя столбца в БД
            public string FullName { get; set; }

            [Column("Number_telephone")] // Изменено на имя столбца в БД
            public string Phone { get; set; }

            [Column("password_confirm")] // Изменено на имя столбца в БД
            public bool IsPasswordConfirmationRequired { get; set; }

            [Column("Email")]
            public string Email { get; set; }

            [Column("email_confirmed")]
            public bool EmailConfirmed { get; set; }

            [NotMapped]
            public string PhoneNumber { get; set; } // Оставлено как вычисляемое свойство

            [Column("account_confirmed")]
            public bool IsAccountConfirmed { get; set; }

            [Column("Date_auto")]
            public DateTime? LastLoginDate { get; set; }

            [Column("last_password_change")]
            public DateTime? LastPasswordChange { get; set; }

            [Column("ConfirmationDate")]
            public DateTime? ConfirmationDate { get; set; }

            [Column("ConfirmationCodeExpiry")]
            public DateTime? ConfirmationCodeExpiry { get; set; }

            [NotMapped]
            public ICollection<string> Roles { get; set; } = new List<string>();

            [NotMapped]
            public string Token { get; set; }

            [NotMapped]
            public DateTime TokenExpiry { get; set; }

            [NotMapped]
            public bool IsTokenValid => !string.IsNullOrEmpty(Token) && TokenExpiry > DateTime.UtcNow;

            [NotMapped]
            public string Role { get; set; }

            public void SetRolesFromCollection()
            {
                if (UserRoles != null && UserRoles.Any())
                {
                    Role = string.Join(",", UserRoles.Select(ur => ur.Role?.RoleName));
                }
            }

        public class GameSystem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Status { get; set; }
        }

        public ICollection<UserRole> UserRoles { get; set; }
            public ICollection<EventParticipant> EventParticipants { get; set; }
        }
    }

