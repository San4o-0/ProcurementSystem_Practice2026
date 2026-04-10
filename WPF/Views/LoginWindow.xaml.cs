using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Services;
using System.Windows;
using static ProcurementSystem.Services.UserService;
using System.Windows.Media;  // ✅ ДОДАЙ ЦЕ!

namespace ProcurementSystem.Wpf
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "⏳ Авторизація...";


            try
            {
                var authService = App.Services.GetRequiredService<AuthService>();
                var user = authService.Login(LoginBox.Text, PasswordBox.Password);

                if (user == null)
                {
                    ErrorText.Text = "❌ Невірний логін або пароль";
                    PasswordBox.Focus();
                    PasswordBox.SelectAll();  // ✅ Виділяємо пароль
                    return;
                }

                ErrorText.Text = "";
                UserSession.CurrentUser = user;
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                Close();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ЗАБЛОКОВАНО"))
            {
                ErrorText.Text = "🚫 КОРИСТУВАЧ ЗАБЛОКОВАНО!";
                LoginBox.Focus();
                LoginBox.SelectAll();
            }
            catch
            {
                ErrorText.Text = "❌ Помилка сервера";
                LoginBox.Focus();
            }
        }





    }
}
