using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class EventEquipment
    {
        public int EventEquipmentID { get; set; }
        public int EventID { get; set; }
        public int EquipmentID { get; set; }
        public int Quantity { get; set; }

        public Event Event { get; set; }
        public Equipment Equipment { get; set; }
    }
}

