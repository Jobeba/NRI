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
            var isFullscreen = window.WindowState != WindowState.Maximized;

            // Переключаем состояние окна
            window.WindowState = isFullscreen ? WindowState.Maximized : WindowState.Normal;
            window.ResizeMode = isFullscreen ? ResizeMode.NoResize : ResizeMode.CanResize;

            // Обновляем иконку если есть
            if (toggleIcon != null)
            {
                toggleIcon.Kind = isFullscreen ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
            }

            // Анимации
            var targetBgScale = isFullscreen ? 1.05 : 1.1;
            var targetBlur = isFullscreen ? 4 : 8;

            await Task.WhenAll(
                AnimateScaleAsync(backgroundScale, targetBgScale, targetBgScale, duration),
                AnimateBlurAsync(backgroundBlur, targetBlur, duration)
            );
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

            // Добавляем проверку на null
            if (toggleIcon != null)
            {
                toggleIcon.Kind = PackIconKind.WindowRestore;
            }

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
