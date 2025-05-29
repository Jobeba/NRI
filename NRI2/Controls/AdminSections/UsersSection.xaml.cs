using Microsoft.VisualBasic.ApplicationServices;
using NRI.Classes;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NRI.Controls.AdminSections
{
    /// <summary>
    /// Логика взаимодействия для UsersSection.xaml
    /// </summary>
    public partial class UsersSection : UserControl
    {
        public ObservableCollection<Classes.User> Users { get; }
        public UsersSection(ObservableCollection<Classes.User> users)
        {
            Users = users;
            InitializeComponent();
        }
    }
}
