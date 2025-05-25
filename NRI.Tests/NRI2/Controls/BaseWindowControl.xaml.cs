using NRI.Classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NRI.Controls
{
    public partial class BaseWindowControl : UserControl
    {
        public static readonly DependencyProperty SideMenuContentProperty =
            DependencyProperty.Register("SideMenuContent", typeof(object), typeof(BaseWindowControl));

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register("MainContent", typeof(object), typeof(BaseWindowControl));

        public object SideMenuContent
        {
            get => GetValue(SideMenuContentProperty);
            set => SetValue(SideMenuContentProperty, value);
        }

        public object MainContent
        {
            get => GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }

        public BaseWindowControl()
        {
            InitializeComponent();
        }
        public void ToggleMenu(bool isVisible)
        {
            var animation = new GridLengthAnimation
            {
                From = isVisible ? new GridLength(180) : new GridLength(0),
                To = isVisible ? new GridLength(0) : new GridLength(180),
                Duration = TimeSpan.FromSeconds(0.3)
            };

            MenuColumn.BeginAnimation(ColumnDefinition.WidthProperty, animation);
        }
    }
}
