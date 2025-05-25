using NRI.Classes;
using NRI.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NRI.Pages
{
    /// <summary>
    /// Логика взаимодействия для EventsPage.xaml
    /// </summary>
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public int OrganizerID { get; set; } // ID организатора
        public int? SystemID { get; set; }   // ID игровой системы (необязательное)
        public int? SettingID { get; set; }  // ID сеттинга (необязательное)
        public int MaxParticipants { get; set; }
        public DateTime EventDate { get; set; }

        [NotMapped]
        public string SystemName { get; set; }
        [NotMapped]
        public string SettingName { get; set; }

        // Навигационные свойства
        public virtual User Organizer { get; set; }
        public virtual ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
    }
}
