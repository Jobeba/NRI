using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class CalendarEvent
    {
        public int CalendarEventID { get; set; }
        public int? EventID { get; set; }
        public int? TableID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
