using NRI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int UserID { get; set; }
        public int? EventID { get; set; }
        public int? TableID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }

        public User User { get; set; }
        public Event Event { get; set; }
        public Table Table { get; set; }
    }
}

