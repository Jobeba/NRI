using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class StaffEvent
    {
        public int StaffEventID { get; set; }
        public int StaffID { get; set; }
        public int EventID { get; set; }
        public string Role { get; set; }

        public Staff Staff { get; set; }
        public Event Event { get; set; }
    }
}

