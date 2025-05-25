using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NRI.DiceRoll
{
    public class GameSystemTheme
    {
        public string Name { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public FontFamily Font { get; set; }
        public ImageSource Icon { get; set; }
        public Uri RollSound { get; set; }
        public Uri CriticalSound { get; set; }
        public Brush ButtonBackground { get; set; }
        public Brush ButtonForeground { get; set; }
        public Brush TextBoxBackground { get; set; }
        public Brush TextBoxForeground { get; set; }
        public Style DiceStyle { get; set; }
        public ImageBrush TextureBrush { get; set; }
        

        public static GameSystemTheme GetTheme(string systemName)
        {
            return systemName switch
            {
                "D&D 5e" => new GameSystemTheme
                {
                    Name = "D&D 5e",
                    BackgroundBrush = (Brush)Application.Current.FindResource("DnDBackground"),
                    Font = (FontFamily)Application.Current.FindResource("FantasyFont"),
                    Icon = (ImageSource)Application.Current.FindResource("DnDIcon"),
                    RollSound = new Uri("/Resources/Sounds/DnD/roll.wav", UriKind.Relative),
                    CriticalSound = new Uri("/Resources/Sounds/DnD/critical.wav", UriKind.Relative)
                },

                "Pathfinder" => new GameSystemTheme
                {
                    Name = "Pathfinder",
                    BackgroundBrush = (Brush)Application.Current.FindResource("PathfinderBackground"),
                    ForegroundBrush = Brushes.White,
                    Font = (FontFamily)Application.Current.FindResource("SciFiFont"),
                    Icon = (ImageSource)Application.Current.FindResource("PathfinderIcon"),
                    RollSound = new Uri("/Resources/Sounds/Pathfinder/roll.wav", UriKind.Relative),
                    CriticalSound = new Uri("/Resources/Sounds/Pathfinder/critical.wav", UriKind.Relative),
                    ButtonBackground = (Brush)Application.Current.FindResource("PathfinderButtonBackground"),
                    ButtonForeground = Brushes.White,
                    TextBoxBackground = (Brush)Application.Current.FindResource("PathfinderTextBoxBackground"),
                    TextBoxForeground = Brushes.Black
                },
                "Call of Cthulhu" => new GameSystemTheme
                {
                    Name = "Call of Cthulhu",
                    BackgroundBrush = (LinearGradientBrush)Application.Current.FindResource("CthulhuBackground"),
                    ForegroundBrush = Brushes.LightGray,
                    Font = (FontFamily)Application.Current.FindResource("EldritchFont"),
                    Icon = (ImageSource)Application.Current.FindResource("CthulhuIcon"),
                    RollSound = new Uri("pack://application:,,,/Resources/Sounds/Cthulhu/whisper.wav"),
                    CriticalSound = new Uri("pack://application:,,,/Resources/Sounds/Cthulhu/insanity.wav"),
                    DiceStyle = (Style)Application.Current.FindResource("CthulhuDiceStyle"),
                    TextureBrush = (ImageBrush)Application.Current.FindResource("CthulhuElderSignTexture")
                },
                _ => DefaultTheme
            };
        }

        public static readonly GameSystemTheme DefaultTheme = new() { };
    }
}
