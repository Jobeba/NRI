using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class SnackbarMessage
{
    public object Content { get; set; }
    public object ActionContent { get; set; }
    public Action<object, RoutedEventArgs> ActionClick { get; set; }
    public bool IsActive { get; set; } = true;
}
