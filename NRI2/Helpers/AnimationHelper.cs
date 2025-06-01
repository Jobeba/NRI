using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace NRI.Helpers
{
    public static class AnimationHelper
    {
        public static async Task ToggleFullscreenMode(
            Window window,
            ScaleTransform contentScale,
            PackIcon toggleIcon,
            TimeSpan duration)
        {
            var isFullscreen = window.WindowState != WindowState.Maximized;
            window.WindowState = isFullscreen ? WindowState.Maximized : WindowState.Normal;

            if (toggleIcon != null)
            {
                toggleIcon.Kind = isFullscreen ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
            }
        }

        public static Task AnimateScaleAsync(this ScaleTransform transform,
                                          double toScaleX, double toScaleY,
                                          TimeSpan duration)
        {
            var tcs = new TaskCompletionSource<bool>();

            var animX = new DoubleAnimation(toScaleX, duration);
            var animY = new DoubleAnimation(toScaleY, duration);

            animX.Completed += (s, e) => tcs.TrySetResult(true);

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, animY);

            return tcs.Task;
        }

        public static Task AnimateBlurAsync(this BlurEffect effect,
                                         double toRadius,
                                         TimeSpan duration)
        {
            var tcs = new TaskCompletionSource<bool>();

            var anim = new DoubleAnimation(toRadius, duration);
            anim.Completed += (s, e) => tcs.TrySetResult(true);

            effect.BeginAnimation(BlurEffect.RadiusProperty, anim);

            return tcs.Task;
        }
    }
}
