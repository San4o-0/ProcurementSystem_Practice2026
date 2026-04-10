using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using ProcurementSystem.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ProcurementSystem.Wpf.Views
{
    public partial class AdminUsersView : UserControl
    {
        private UserService? _userService;

        public AdminUsersView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_userService != null)
                return; // щоб не вантажити двічі

            if (App.Services == null)
                return;

            _userService = App.Services.GetRequiredService<UserService>();
            LoadUsers();
        }
        private ObservableCollection<User> _users;


        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            _users = new ObservableCollection<User>(users);
            UsersDataGrid.ItemsSource = _users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateUserWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Select a user to delete",
                                "Info",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var user = (Models.User)UsersDataGrid.SelectedItem;

            // ❌ забороняємо видаляти себе
            if (user.Id == UserSession.CurrentUser.Id)
            {
                MessageBox.Show("You cannot delete your own account",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Delete user '{user.FullName}'?",
                "Confirm delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _userService.DeleteUser(user.Id);
            LoadUsers();
        }


        private void IsActive_Checked(object sender, RoutedEventArgs e)
        {
            UpdateIsActive(sender, true);
        }

        private void IsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateIsActive(sender, false);
        }

        private void UpdateIsActive(object sender, bool isActive)
        {
            if (sender is not CheckBox cb)
                return;

            if (cb.DataContext is not User user)
                return;

            if (user.Id == UserSession.CurrentUser.Id)
            {
                cb.IsChecked = true;
                return;
            }

            _userService.SetActive(user.Id, isActive);
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not User user)
                return;

            var window = new EditUserWindow(user.Id)
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true)
                LoadUsers();
        }



        private void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();  // ✅ той самий метод!
        }
    }
}
