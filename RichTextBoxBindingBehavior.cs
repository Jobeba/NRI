using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace NRI.DiceRoll
{
    public class RichTextBoxBindingBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty RichTextProperty =
            DependencyProperty.Register("RichText", typeof(string),
                typeof(RichTextBoxBindingBehavior),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnRichTextChanged));

        private bool _isUpdating;

        public string RichText
        {
            get => (string)GetValue(RichTextProperty);
            set => SetValue(RichTextProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var range = new TextRange(
                AssociatedObject.Document.ContentStart,
                AssociatedObject.Document.ContentEnd);

            using (var ms = new MemoryStream())
            {
                range.Save(ms, DataFormats.Rtf);
                RichText = Encoding.UTF8.GetString(ms.ToArray());
            }

            _isUpdating = false;
        }

        private static void OnRichTextChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RichTextBoxBindingBehavior)d;
            if (behavior._isUpdating || behavior.AssociatedObject == null) return;

            behavior._isUpdating = true;

            var newValue = e.NewValue as string;
            var range = new TextRange(
                behavior.AssociatedObject.Document.ContentStart,
                behavior.AssociatedObject.Document.ContentEnd);

            if (!string.IsNullOrEmpty(newValue))
            {
                try
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(newValue)))
                    {
                        range.Load(ms, DataFormats.Rtf);
                    }
                }
                catch
                {
                    range.Text = newValue;
                }
            }
            else
            {
                range.Text = string.Empty;
            }

            behavior._isUpdating = false;
        }
    }
}
