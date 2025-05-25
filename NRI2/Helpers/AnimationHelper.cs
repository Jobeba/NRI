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
        ScaleTransform backgroundScale,
        BlurEffect backgroundBlur,
        PackIcon toggleIcon,
        TimeSpan duration)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                await AnimateToWindowedMode(window, contentScale, backgroundScale, backgroundBlur, toggleIcon, duration);
            }
            else
            {
                await AnimateToFullscreenMode(window, contentScale, backgroundScale, backgroundBlur, toggleIcon, duration);
            }
        }

        private static async Task AnimateToWindowedMode(
            Window window,
            ScaleTransform contentScale,
            ScaleTransform backgroundScale,
            BlurEffect backgroundBlur,
            PackIcon toggleIcon,
            TimeSpan duration)
        {
            var tasks = new List<Task>
        {
            AnimateScaleAsync(contentScale, 0.95, 0.95, duration),
            AnimateBlurAsync(backgroundBlur, 8, duration),
            AnimateScaleAsync(backgroundScale, 1.1, 1.1, duration)
        };

            await Task.WhenAll(tasks);

            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.CanResize;
            toggleIcon.Kind = PackIconKind.WindowMaximize;
        }

        private static async Task AnimateToFullscreenMode(
            Window window,
            ScaleTransform contentScale,
            ScaleTransform backgroundScale,
            BlurEffect backgroundBlur,
            PackIcon toggleIcon,
            TimeSpan duration)
        {
            window.WindowState = WindowState.Maximized;
            window.ResizeMode = ResizeMode.NoResize;
            toggleIcon.Kind = PackIconKind.WindowRestore;

            var tasks = new List<Task>
        {
            AnimateScaleAsync(contentScale, 1.0, 1.0, duration),
            AnimateBlurAsync(backgroundBlur, 4, duration),
            AnimateScaleAsync(backgroundScale, 1.05, 1.05, duration)
        };

            await Task.WhenAll(tasks);
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
