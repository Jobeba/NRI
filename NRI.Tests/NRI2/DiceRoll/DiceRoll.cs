using NRI.Classes;
using NRI.DiceRoll;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace NRI
{
    [Table("DiceRolls")]
    public class DiceRolling
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("RollId")]
        public int RollId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(10)")]
        public string DiceType { get; set; }

        [Column]
        public int DiceCount { get; set; } = 1;

        [Column]
        public int Modifier { get; set; }

        [NotMapped]
        public List<int> Results { get; set; } = new List<int>();

        [Column("Results", TypeName = "nvarchar(MAX)")]
        public string ResultsJson
        {
            get => JsonSerializer.Serialize(Results);
            set => Results = string.IsNullOrEmpty(value)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(value);
        }
        public string CustomDescription { get; set; }

        public string Description => CustomDescription ?? $"{CharacterName}: {DiceCount}{DiceType}{(Modifier != 0 ? $" + {Modifier}" : "")}";

        [Column(TypeName = "nvarchar(100)")]
        public string CharacterName { get; set; }

        [Column]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        [Column("UserId")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        [ForeignKey("Character")]
        [Column("CharacterId")]
        public int? CharacterId { get; set; }

        public virtual CharacterSheet Character { get; set; }

        // Вычисляемые свойства
        [NotMapped]
        public int Total => (Results?.Sum() ?? 0) + Modifier;

        [NotMapped]
        public string ResultsString => string.Join(", ", Results);

        [NotMapped]
        public string RollDescription => $"{CharacterName} бросает {DiceCount}{DiceType}{(Modifier != 0 ? (Modifier > 0 ? "+" : "") + Modifier : "")}";

        [NotMapped]
        public bool IsCriticalSuccess =>
                    (Character?.System == "Call of Cthulhu" && Results.Contains(1)) || // Критический успех в CoC
                    (DiceType == "D20" && Results.Contains(20)); // Критический успех в D&D


        [NotMapped]
        public bool IsCriticalFailure =>
                        (Character?.System == "Call of Cthulhu" && Results.Contains(100)) || // Критический провал в CoC
                        (DiceType == "D20" && Results.Contains(1)); // Критический провал в D&D

    }
}