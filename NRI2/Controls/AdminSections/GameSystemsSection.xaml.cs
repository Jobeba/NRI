﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static NRI.Classes.User;

namespace NRI.Controls.AdminSections
{
    /// <summary>
    /// Логика взаимодействия для GameSystemsSection.xaml
    /// </summary>
    public partial class GameSystemsSection : UserControl
    {
        public ObservableCollection<GameSystem> GameSystems { get; }
        public GameSystemsSection(ObservableCollection<GameSystem> gameSystems)
        {
            GameSystems = gameSystems;
            InitializeComponent();
        }
    }
}
