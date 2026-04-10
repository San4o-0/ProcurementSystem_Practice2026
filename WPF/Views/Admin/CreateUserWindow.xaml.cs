using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Services;
using System;
using System.Windows;

namespace ProcurementSystem.Wpf.Views
{
    public partial class CreateUserWindow : Window
    {
        private readonly UserService _userService;

        public CreateUserWindow()
        {
            InitializeComponent();

            _userService = App.Services.GetRequiredService<UserService>();

            LoadRoles();
        }

        private void LoadRoles()
        {
            RoleBox.ItemsSource = _userService.GetRoles();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var fullName = FullNameBox.Text.Trim();
            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password;
            var roleId = RoleBox.SelectedValue as int?;

            // 🔒 Валідація
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                roleId == null)
            {
                ErrorText.Text = "Please fill all fields";
                return;
            }

            if (password.Length < 4)
            {
                ErrorText.Text = "Password must be at least 4 characters";
                return;
            }

            if (_userService.LoginExists(login))
            {
                ErrorText.Text = "Login already exists";
                return;
            }

            try
            {
                _userService.CreateUser(
                    fullName,
                    login,
                    password,
                    roleId.Value
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
