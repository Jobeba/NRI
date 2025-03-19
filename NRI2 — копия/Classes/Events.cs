using System;

namespace NRI.Models
{
    public class Events
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public int OrganizerID { get; set; }
        public int SystemID { get; set; }
        public int SettingID { get; set; }
        public int MaxParticipants { get; set; }
        public DateTime EventDate { get; set; }
    }
}