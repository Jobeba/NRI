using System.Windows;
using System.Windows.Controls;

namespace NRI.Controls
{
    public partial class ErrorPage : UserControl
    {
        public ErrorPage(string errorMessage)
        {
            InitializeComponent();
            ErrorText.Text = errorMessage;
        }
    }
}
