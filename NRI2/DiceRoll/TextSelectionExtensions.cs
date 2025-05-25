using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NRI.DiceRoll
{
    public static class TextSelectionExtensions
    {
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.RegisterAttached(
                "FontFamily",
                typeof(FontFamily),
                typeof(TextSelectionExtensions),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFontFamily(DependencyObject element, FontFamily value)
            => element.SetValue(FontFamilyProperty, value);

        public static FontFamily GetFontFamily(DependencyObject element)
            => (FontFamily)element.GetValue(FontFamilyProperty);

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.RegisterAttached(
                "FontSize",
                typeof(double),
                typeof(TextSelectionExtensions),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFontSize(DependencyObject element, double value)
            => element.SetValue(FontSizeProperty, value);

        public static double GetFontSize(DependencyObject element)
            => (double)element.GetValue(FontSizeProperty);
    }

}
