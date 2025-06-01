using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Animation;
using System;
using Microsoft.Extensions.Logging;
using NRI.Pages;
using GalaSoft.MvvmLight.Views;

namespace NRI.Controls
{
    public partial class BaseWindowControl : UserControl, IRoleNavigation
    {
        public static readonly DependencyProperty SideMenuContentProperty =
            DependencyProperty.Register(
                "SideMenuContent",
                typeof(object),
                typeof(BaseWindowControl));

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
                nameof(MainContent),         
                typeof(object),                
                typeof(BaseWindowControl),   
                new PropertyMetadata(null));

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
            var targetWidth = isVisible ? 200 : 0;
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            MenuColumn.BeginAnimation(ColumnDefinition.WidthProperty, animation);
        }
        public void NavigateToHome()
        {
            // Реализация навигации
        }

        public void NavigateToDiceRoller()
        {
            // Реализация навигации
        }
    }
}
