using iTextSharp.text.pdf;
using NRI.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace NRI.DiceRoll
{
    public static class PdfCharacterSheet
    {
        public static bool FillPdfTemplate(CharacterSheet character, string templatePath, string outputPath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = new PdfReader(templatePath))
                using (var stream = new FileStream(outputPath, FileMode.Create))
                using (var stamper = new PdfStamper(reader, stream))
                {
                    var form = stamper.AcroFields;
                    var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    // Основная информация
                    SetFieldIfExists(form, "Имя персонажа", character.Name, baseFont);
                    SetFieldIfExists(form, "КЛАСС и УРОВЕНЬ", $"{character.Class} {character.Level}", baseFont);
                    SetFieldIfExists(form, "ПРЕДЫСТОРИЯ", character.Background ?? "", baseFont);
                    SetFieldIfExists(form, "ИМЯ_ИГРОКА", character.PlayerName ?? "", baseFont);
                    SetFieldIfExists(form, "РАСА", character.Race ?? "", baseFont);
                    SetFieldIfExists(form, "МИРОВОЗЗРЕНИЕ", character.Alignment ?? "", baseFont);
                    SetFieldIfExists(form, "EXPERIENCE_POINTS", character.ExperiencePoints?.ToString() ?? "0", baseFont);

                    // Характеристики
                    var attributes = character.AttributesCollection.ToDictionary(a => a.Name.ToUpper(), a => a.Value);
                    SetFieldIfExists(form, "СИЛА", GetDictionaryValueOrDefault(attributes, "СИЛА", "10"), baseFont);
                    SetFieldIfExists(form, "ЛОВКОСТЬ", GetDictionaryValueOrDefault(attributes, "ЛОВКОСТЬ", "10"), baseFont);
                    SetFieldIfExists(form, "ТЕЛОСЛОЖЕНИЕ", GetDictionaryValueOrDefault(attributes, "ТЕЛОСЛОЖЕНИЕ", "10"), baseFont);
                    SetFieldIfExists(form, "ИНТЕЛЕКТ", GetDictionaryValueOrDefault(attributes, "ИНТЕЛЛЕКТ", "10"), baseFont);
                    SetFieldIfExists(form, "МУДРОСТЬ", GetDictionaryValueOrDefault(attributes, "МУДРОСТЬ", "10"), baseFont);
                    SetFieldIfExists(form, "ХАРИЗМА", GetDictionaryValueOrDefault(attributes, "ХАРИЗМА", "10"), baseFont);

                    // Бонусы характеристик
                    SetFieldIfExists(form, "Сил", CalculateModifier(GetDictionaryValueOrDefault(attributes, "СИЛА", "10")), baseFont);
                    SetFieldIfExists(form, "Лов", CalculateModifier(GetDictionaryValueOrDefault(attributes, "ЛОВКОСТЬ", "10")), baseFont);
                    SetFieldIfExists(form, "Тлс", CalculateModifier(GetDictionaryValueOrDefault(attributes, "ТЕЛОСЛОЖЕНИЕ", "10")), baseFont);
                    SetFieldIfExists(form, "ИнТ", CalculateModifier(GetDictionaryValueOrDefault(attributes, "ИНТЕЛЛЕКТ", "10")), baseFont);
                    SetFieldIfExists(form, "МуД", CalculateModifier(GetDictionaryValueOrDefault(attributes, "МУДРОСТЬ", "10")), baseFont);
                    SetFieldIfExists(form, "ХаР", CalculateModifier(GetDictionaryValueOrDefault(attributes, "ХАРИЗМА", "10")), baseFont);

                    // Другие поля
                    SetFieldIfExists(form, "КЗ", GetDictionaryValueOrDefault(attributes, "КЛАСС ЗАЩИТЫ", "10"), baseFont);
                    SetFieldIfExists(form, "Иниц", GetDictionaryValueOrDefault(attributes, "ИНИЦИАТИВА", "0"), baseFont);
                    SetFieldIfExists(form, "Скорость", GetDictionaryValueOrDefault(attributes, "СКОРОСТЬ", "30"), baseFont);
                    SetFieldIfExists(form, "МАКС,ХП", GetDictionaryValueOrDefault(attributes, "MAX HP", "10"), baseFont);
                    SetFieldIfExists(form, "тек.ХП", GetDictionaryValueOrDefault(attributes, "ТЕКУЩИЙ HP", "10"), baseFont);
                    SetFieldIfExists(form, "врем.ХП", GetDictionaryValueOrDefault(attributes, "ВРЕМЕННЫЙ HP", "0"), baseFont);

                    // Навыки
                    var skills = character.SkillsCollection.ToDictionary(s => s.Name.Split(' ')[0].ToUpper(), s => s.Value);
                    SetSkillField(form, "знч.Акр", skills, "АКРОБАТИКА", baseFont);
                    SetSkillField(form, "знч.Дрес", skills, "ДРЕССИРОВКА", baseFont);
                    SetSkillField(form, "знч.Тай", skills, "ТАЙНЫЕ ЗНАНИЯ", baseFont);
                    SetSkillField(form, "знч.Атл", skills, "АТЛЕТИКА", baseFont);
                    SetSkillField(form, "знч.Обм", skills, "ОБМАН", baseFont);
                    SetSkillField(form, "знч.Ист", skills, "ИСТОРИЯ", baseFont);
                    SetSkillField(form, "знч.Инт", skills, "ИНТУИЦИЯ", baseFont);
                    SetSkillField(form, "знч.Зап", skills, "ЗАПУГИВАНИЕ", baseFont);
                    SetSkillField(form, "знч.Ана", skills, "АНАЛИЗ", baseFont);
                    SetSkillField(form, "знч.Леч", skills, "ЛЕЧЕНИЕ", baseFont);
                    SetSkillField(form, "знч.Прир", skills, "ПРИРОДА", baseFont);
                    SetSkillField(form, "знч.Вос", skills, "ВОСПРИЯТИЕ", baseFont);
                    SetSkillField(form, "знч.Пред", skills, "ПРЕДСТАВЛЕНИЕ", baseFont);
                    SetSkillField(form, "знч.Убж", skills, "УБЕЖДЕНИЕ", baseFont);
                    SetSkillField(form, "знч.Рел", skills, "РЕЛИГИЯ", baseFont);
                    SetSkillField(form, "знч.Лов", skills, "ЛОВКОСТЬ РУК", baseFont);
                    SetSkillField(form, "знч.Скр", skills, "СКРЫТНОСТЬ", baseFont);
                    SetSkillField(form, "знч.Выж", skills, "ВЫЖИВАНИЕ", baseFont);

                    // Инвентарь
                    string inventoryText = string.Join("\n", character.InventoryItems.Select(i => $"{i.Name} x{i.Quantity}"));
                    SetFieldIfExists(form, "ЭКИПИРОВКА", inventoryText, baseFont);
                    SetFieldIfExists(form, "Equipment", inventoryText, baseFont);

                    // Внешность и история
                    SetFieldIfExists(form, "ВНЕШНОСТЬ ПЕРСОНАЖА", character.Appearance ?? "", baseFont);
                    SetFieldIfExists(form, "ПРЕДЫСТОРИЯ", character.Backstory ?? "", baseFont);
                    SetFieldIfExists(form, "ЛИЧНЫЕ ЧЕРТЫ", character.PersonalityTraits ?? "", baseFont);
                    SetFieldIfExists(form, "ИДЕАЛ", character.Ideals ?? "", baseFont);
                    SetFieldIfExists(form, "УЗЫ", character.Bonds ?? "", baseFont);
                    SetFieldIfExists(form, "ИЗЪЯНЫ", character.Flaws ?? "", baseFont);

                    stamper.FormFlattening = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF: {ex.Message}");
                return false;
            }
        }
        private static void SetSkillField(AcroFields form, string fieldName, Dictionary<string, string> skills, string skillKey, BaseFont font)
        {
            if (skills.TryGetValue(skillKey.ToUpper(), out string value))
            {
                SetFieldIfExists(form, fieldName, value, font);
            }
        }

        private static string GetDictionaryValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue)
        {
            return dict.TryGetValue(key, out string value) ? value : defaultValue;
        }

        private static string CalculateModifier(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                int mod = (value - 10) / 2;
                return mod >= 0 ? $"+{mod}" : mod.ToString();
            }
            return "+0";
        }

        private static void SetFieldIfExists(AcroFields form, string fieldName, string value, BaseFont font)
        {
            if (form.Fields.ContainsKey(fieldName))
            {
                form.SetFieldProperty(fieldName, "textfont", font, null);
                form.SetField(fieldName, value ?? "");
            }
        }
    }
}
