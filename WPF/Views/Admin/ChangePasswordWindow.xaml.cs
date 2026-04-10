using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Services;
using System.Windows;

namespace ProcurementSystem.Wpf.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly UserService _userService;
        private readonly int _userId;

        public ChangePasswordWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _userService = App.Services.GetRequiredService<UserService>();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox1.Password != PasswordBox2.Password)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            if (PasswordBox1.Password.Length < 4)
            {
                MessageBox.Show("Password too short");
                return;
            }

            _userService.ChangePassword(_userId, PasswordBox1.Password);

            MessageBox.Show("Password changed");

            DialogResult = true; // 🔴 ВАЖЛИВО
            Close();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
