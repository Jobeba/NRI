using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

public class PasswordConfirmationDialog : Control
{
    public static readonly DependencyProperty SecretKeyProperty =
        DependencyProperty.Register("SecretKey", typeof(string), typeof(PasswordConfirmationDialog));

    public string SecretKey
    {
        get => (string)GetValue(SecretKeyProperty);
        set => SetValue(SecretKeyProperty, value);
    }

    public ICommand ConfirmCommand
    {
        get => (ICommand)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public static readonly DependencyProperty ConfirmCommandProperty =
        DependencyProperty.Register("ConfirmCommand", typeof(ICommand), typeof(PasswordConfirmationDialog));

    public PasswordConfirmationDialog()
    {
        DefaultStyleKey = typeof(PasswordConfirmationDialog);
    }
}

