using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DB
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public int RecordId { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? UserLogin { get; set; }
        public string? AdditionalInfo { get; set; }
    }

}
