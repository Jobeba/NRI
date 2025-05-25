using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string? Description { get; set; }
        public int OrganizerID { get; set; }
        public int SystemID { get; set; }
        public int SettingID { get; set; }
        public int MaxParticipants { get; set; }
        public DateTime EventDate { get; set; }

        public ICollection<EventParticipant> Participants { get; set; }
        public ICollection<StaffEvent> StaffEvents { get; set; }
    }

}
