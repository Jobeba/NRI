using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace NRI.DiceRoll
{
    public class RichTextBoxCommandTargetBehavior : Behavior<RichTextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            CommandManager.AddExecutedHandler(AssociatedObject, OnCommandExecuted);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            CommandManager.RemoveExecutedHandler(AssociatedObject, OnCommandExecuted);
        }

        private void OnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == EditingCommands.ToggleBold ||
                e.Command == EditingCommands.ToggleItalic ||
                e.Command == EditingCommands.ToggleUnderline ||
                e.Command == EditingCommands.AlignLeft ||
                e.Command == EditingCommands.AlignCenter ||
                e.Command == EditingCommands.AlignRight ||
                e.Command == EditingCommands.AlignJustify)
            {
                if (AssociatedObject != null && AssociatedObject.IsKeyboardFocused)
                {
                    AssociatedObject.Focus();
                    e.Handled = true;
                }
            }
        }
    }
}
