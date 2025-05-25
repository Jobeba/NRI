using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace NRI.Controls
{
    public partial class AdminWindowControl : UserControl
    {
        public AdminWindowControl(IServiceProvider serviceProvider)
        {
            InitializeComponent();
        }
    }
}
