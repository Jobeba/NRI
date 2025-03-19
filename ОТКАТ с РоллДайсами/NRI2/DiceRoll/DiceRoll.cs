using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public class DiceRolling
    {
        public string DiceType { get; set; } // Тип кубика (D4, D6, D8 и т.д.)
        public int DiceCount { get; set; }   // Количество кубиков
        public List<int> Results { get; set; } // Результаты бросков
        public int Sum { get; set; }         // Сумма результатов
        public DateTime Timestamp { get; set; } = DateTime.Now; // Время броска
    }
}
