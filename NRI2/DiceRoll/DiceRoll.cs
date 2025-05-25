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

        // Свойства для вычисления границ значений
        [NotMapped]
        public int DiceSides => int.Parse(DiceType.Substring(1)); // Извлекаем количество граней из DiceType (например, "D20" → 20)

        [NotMapped]
        public int MaxPossibleValue => DiceCount * DiceSides + Modifier;

        [NotMapped]
        public int MinPossibleValue => DiceCount * 1 + Modifier; // Минимум 1 на каждом кубике

        [NotMapped]
        public int MaxPossible
        {
            get
            {
                return DiceType switch
                {
                    "D2" => 2,
                    "D3" => 3,
                    "D4" => 4,
                    "D6" => 6,
                    "D8" => 8,
                    "D10" => 10,
                    "D12" => 12,
                    "D20" => 20,
                    "D100" => 100,
                    "FATE" => 1, // Для FATE dice
                    _ => 0
                } * DiceCount + Modifier;
            }
        }

        [NotMapped]
        public int MinPossible
        {
            get
            {
                return DiceType switch
                {
                    "FATE" => -1, // Для FATE dice
                    _ => 1
                } * DiceCount + Modifier;
            }
        }

        // Критический успех
        [NotMapped]
        public bool IsCriticalSuccess
        {
            get
            {
                if (Character == null || Results == null || !Results.Any()) return false;

                return Character.System switch
                {
                    "D&D 5e" =>
                        DiceType == "D20" && Results.Any(r => r == 20),

                    "Pathfinder" =>
                        DiceType == "D20" && Results.Any(r => r == 20),

                    "Call of Cthulhu" =>
                        DiceType == "D100" && Results.Any(r => r <= 5),

                    "Warhammer" =>
                        DiceType == "D6" && Results.Any(r => r == 6),

                    "GURPS" =>
                        DiceType == "D6" && Results.Sum() <= 4,

                    "FATE" =>
                        Results.Any(r => r == 1), // В FATE 1 - это "+"

                    _ =>
                        false
                };
            }
        }

        // Критический провал
        [NotMapped]
        public bool IsCriticalFailure
        {
            get
            {
                if (Character == null || Results == null || !Results.Any()) return false;

                return Character.System switch
                {
                    "D&D 5e" =>
                        DiceType == "D20" && Results.Any(r => r == 1),

                    "Pathfinder" =>
                        DiceType == "D20" && Results.Any(r => r == 1),

                    "Call of Cthulhu" =>
                        DiceType == "D100" && Results.Any(r => r >= 96),

                    "Warhammer" =>
                        DiceType == "D6" && Results.Any(r => r == 1),

                    "GURPS" =>
                        DiceType == "D6" && Results.Sum() >= 17,

                    "FATE" =>
                        Results.Any(r => r == -1), // В FATE -1 - это "-"

                    _ =>
                        false
                };
            }
        }

        [NotMapped]
        public string CriticalMessage
        {
            get
            {
                if (!IsCriticalSuccess && !IsCriticalFailure) return string.Empty;

                if (Character == null)
                    return IsCriticalSuccess ? "Критический успех!" : "Критический провал!";

                return Character.System switch
                {
                    "D&D 5e" => IsCriticalSuccess
                        ? "⚔️ Критическое попадание! Сокрушительный удар!"
                        : "💥 Критический провал! Оружие заклинило!",

                    "Pathfinder" => IsCriticalSuccess
                        ? "🔥 Критический успех! Урон удвоен!"
                        : "❌ Критический провал! Осечка!",

                    "Call of Cthulhu" => IsCriticalSuccess
                        ? "🔮 Критический успех! Древние благоволят вам."
                        : "🌀 Критический провал! Потеря 1d10 Расудка!",

                    "Warhammer" => IsCriticalSuccess
                        ? "⚡ Отличный бросок! Благословение Императора!"
                        : "☠️ Ужасный провал! Проклятие Хаоса!",

                    "GURPS" => IsCriticalSuccess
                        ? "🎯 Идеальное попадание!"
                        : "💢 Критическая неудача!",

                    "FATE" => IsCriticalSuccess
                        ? "✨ Идеальное исполнение!"
                        : "🌪️ Полный провал!",

                    _ => IsCriticalSuccess
                        ? "🎉 Критический успех!"
                        : "💀 Критический провал!"
                };
            }
        }

                // Эпический бросок (90% от максимума или 10% от минимума)
        [NotMapped]
        public bool IsEpic
        {
            get
            {
                if (Results == null || !Results.Any()) return false;

                decimal range = MaxPossibleValue - MinPossibleValue;
                decimal epicSuccessThreshold = MinPossibleValue + range * 0.9m;
                decimal epicFailThreshold = MinPossibleValue + range * 0.1m;

                return Total >= epicSuccessThreshold || Total <= epicFailThreshold;
            }
        }

        // Дополнительное свойство для удобства
        [NotMapped]
        public string ResultType
        {
            get
            {
                if (IsEpic) return "Epic";
                if (IsCriticalSuccess) return "Critical Success";
                if (IsCriticalFailure) return "Critical Failure";
                return "Normal";
            }
        }
    }
}
