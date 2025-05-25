using System.Linq;
using System.Windows.Controls;
using System.Windows;


namespace NRI.DiceRoll
{
    public class TabContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AttributesTemplate { get; set; }
        public DataTemplate SkillsTemplate { get; set; }
        public DataTemplate InventoryTemplate { get; set; }
        public DataTemplate AdditionalFieldsTemplate { get; set; }
        public DataTemplate NotesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SystemTab tab)
            {
                if (tab.AttributeGroups?.Count > 0)
                    return AttributesTemplate;

                if (tab.Skills?.Count > 0)
                    return SkillsTemplate;

                if (tab.Inventory?.Count > 0)
                    return InventoryTemplate;

                if (tab.AdditionalFields?.Count > 0)
                    return AdditionalFieldsTemplate;

                if (!string.IsNullOrEmpty(tab.Notes))
                    return NotesTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}

