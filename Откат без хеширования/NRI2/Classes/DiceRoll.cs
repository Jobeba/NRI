using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public class DiceRoll
    {
        public string DiceType { get; set; }
        public int DiceCount { get; set; }
        public List<int> Results { get; set; }
        public int Sum { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
