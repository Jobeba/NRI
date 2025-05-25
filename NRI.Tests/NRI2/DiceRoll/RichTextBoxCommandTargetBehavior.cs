using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace NRI.DiceRoll
{
    public class RichTextBoxCommandTargetBehavior : Behavior<RichTextBox>
    {
        // Определяем присоединяемое свойство RegisterButton
        public static readonly DependencyProperty RegisterButtonProperty =
              DependencyProperty.RegisterAttached(
                  "RegisterButton",
                  typeof(ToggleButton),
                  typeof(RichTextBoxCommandTargetBehavior),
                  new PropertyMetadata(null, OnRegisterButtonChanged));

        public static void SetRegisterButton(DependencyObject element, ToggleButton value)
        {
            element.SetValue(RegisterButtonProperty, value);
        }

        public static ToggleButton GetRegisterButton(DependencyObject element)
        {
            return (ToggleButton)element.GetValue(RegisterButtonProperty);
        }

        private static void OnRegisterButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton button)
            {
                var behavior = Interaction.GetBehaviors(button).FirstOrDefault(b => b is RichTextBoxCommandTargetBehavior) as RichTextBoxCommandTargetBehavior;
                if (behavior != null)
                {
                    behavior.UpdateButtonState(button);
                }
            }
        }
        private void UpdateButtonState(ToggleButton button)
        {
            if (AssociatedObject == null || AssociatedObject.Selection == null || button.Command == null)
                return;

            var command = button.Command as RoutedCommand;
            if (command == null) return;

            if (command == EditingCommands.ToggleBold)
            {
                var fontWeight = AssociatedObject.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                button.IsChecked = (fontWeight != DependencyProperty.UnsetValue &&
                                 (FontWeight)fontWeight == FontWeights.Bold);
            }
            else if (command == EditingCommands.ToggleItalic)
            {
                var fontStyle = AssociatedObject.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                button.IsChecked = (fontStyle != DependencyProperty.UnsetValue &&
                                 (FontStyle)fontStyle == FontStyles.Italic);
            }
            else if (command == EditingCommands.ToggleUnderline)
            {
                var textDecorations = AssociatedObject.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                button.IsChecked = (textDecorations != DependencyProperty.UnsetValue &&
                                 textDecorations == TextDecorations.Underline);
            }
        }
        private Dictionary<ICommand, ButtonBase> _commandButtons = new Dictionary<ICommand, ButtonBase>();

        protected override void OnAttached()
        {
            base.OnAttached();
            CommandManager.AddExecutedHandler(AssociatedObject, OnCommandExecuted);
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            CommandManager.RemoveExecutedHandler(AssociatedObject, OnCommandExecuted);
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            var selection = AssociatedObject.Selection;
            var boldButton = GetRegisterButton(AssociatedObject);

            if (boldButton != null && boldButton is ToggleButton toggleButton)
            {
                var fontWeight = selection.GetPropertyValue(Inline.FontWeightProperty);
                toggleButton.IsChecked = fontWeight != DependencyProperty.UnsetValue &&
                                       fontWeight.Equals(FontWeights.Bold);
            }
        }

        private void UpdateButtonStates()
        {
            if (AssociatedObject == null || AssociatedObject.Selection == null)
                return;

            // Получаем все зарегистрированные кнопки
            var buttons = _commandButtons.Values.OfType<ToggleButton>();
            foreach (var button in buttons)
            {
                var command = button.Command as RoutedCommand;
                if (command == null) continue;

                // Обновляем состояние кнопки в зависимости от форматирования выделенного текста
                if (command == EditingCommands.ToggleBold)
                {
                    var fontWeight = AssociatedObject.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                    button.IsChecked = (fontWeight != DependencyProperty.UnsetValue && (FontWeight)fontWeight == FontWeights.Bold);
                }
                else if (command == EditingCommands.ToggleItalic)
                {
                    var fontStyle = AssociatedObject.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                    button.IsChecked = (fontStyle != DependencyProperty.UnsetValue && (FontStyle)fontStyle == FontStyles.Italic);
                }
                else if (command == EditingCommands.ToggleUnderline)
                {
                    var textDecorations = AssociatedObject.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                    button.IsChecked = (textDecorations != DependencyProperty.UnsetValue && textDecorations == TextDecorations.Underline);
                }
            }
        }

        private void OnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (AssociatedObject.IsKeyboardFocusWithin)
            {
                bool handled = false;

                if (e.Command == EditingCommands.ToggleBold ||
                    e.Command == EditingCommands.ToggleItalic ||
                    e.Command == EditingCommands.ToggleUnderline ||
                    e.Command == EditingCommands.ToggleSubscript ||
                    e.Command == EditingCommands.AlignLeft ||
                    e.Command == EditingCommands.AlignCenter ||
                    e.Command == EditingCommands.AlignRight ||
                    e.Command == EditingCommands.AlignJustify)
                {
                    if (e.Command is RoutedCommand command)
                    {
                        try
                        {
                            command.Execute(null, AssociatedObject);
                            handled = true;
                            UpdateButtonStates();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error executing command: {ex.Message}");
                        }
                    }
                }

                e.Handled = handled;
            }
        }
    }
}
