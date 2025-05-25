using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace NRI.Controls
{
    public class BaseWindowControl : UserControl
    {
        protected readonly IServiceProvider _serviceProvider;

        public BaseWindowControl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}