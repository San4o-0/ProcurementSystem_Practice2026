using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Windows;

namespace ProcurementSystem.Wpf.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly UserService _userService;
        private readonly User _user;

        public EditUserWindow(int userId)
        {
            InitializeComponent();

            _userService = App.Services.GetRequiredService<UserService>();
            _user = _userService.GetById(userId);

            FullNameBox.Text = _user.FullName;
            LoginBox.Text = _user.Login;

            RoleBox.ItemsSource = _userService.GetRoles();
            RoleBox.SelectedValue = _user.RoleId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text))
                return;

            _userService.UpdateUser(
                _user.Id,
                FullNameBox.Text.Trim(),
                (int)RoleBox.SelectedValue
            );

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChangePasswordWindow(_user.Id)
            {
                Owner = this
            };

            window.ShowDialog();
        }

    }
}
