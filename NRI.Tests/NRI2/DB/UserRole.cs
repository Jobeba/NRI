using NRI.Classes;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

    namespace NRI.DB
    {
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public int RoleID { get; set; }
        public Role Role { get; set; }
    }
}

