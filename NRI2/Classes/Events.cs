using System;
using System.ComponentModel.DataAnnotations;

namespace NRI.Models
{
    public class Events
    {
        [Key] // Указываем, что это первичный ключ
        public int EventID;

        [Required]
        [MaxLength(255)]
        public string EventName { get; set; }

        public string Description { get; set; }

        [Required]
        public int OrganizerID { get; set; }

        [Required]
        public int SystemID { get; set; }

        [Required]
        public int SettingID { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        [Required]
        public DateTime EventDate { get; set; }
    }
}