using NRI.Classes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRI.DB
{
    public class EventParticipant
    {
        [Key] // Добавляем атрибут первичного ключа
        public int ParticipationID { get; set; }

        public int EventID { get; set; }
        public int UserID { get; set; }
        public DateTime RegistrationDate { get; set; }

        [Column("Status")]
        public string Status { get; set; }

        [ForeignKey("EventID")] 
        public Event Event { get; set; }

        [ForeignKey("UserID")] 
        public User User { get; set; }
    }
}



