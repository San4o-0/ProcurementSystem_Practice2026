using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProcurementSystem.Wpf.Views
{
    public partial class EmployeeRequestsView : UserControl
    {
        private PurchaseRequestService _service;

        public EmployeeRequestsView()
        {
            InitializeComponent();
            // ✅ АВТОЗАВАНТАЖЕННЯ при створенні
            Loaded += EmployeeRequestsView_Loaded;
        }

        private void EmployeeRequestsView_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeService();
            SafeLoad();
        }

        private void InitializeService()
        {
            try
            {
                _service = App.Services.GetRequiredService<PurchaseRequestService>();
            }
            catch
            {
                _service = null;
            }
        }

        private void SafeLoad()
        {
            try
            {
                if (_service == null)
                {
                    InitializeService();
                }

                if (_service == null || UserSession.CurrentUser == null)
                {
                    Grid.ItemsSource = new List<object>();
                    return;
                }

                var requests = _service.GetMy();
                Grid.ItemsSource = requests ?? new List<PurchaseRequest>();
            }
            catch
            {
                Grid.ItemsSource = new List<object>();
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CreatePurchaseRequestWindow();
                win.Owner = Window.GetWindow(this);

                if (win.ShowDialog() == true)
                {
                    SafeLoad(); // ✅ Оновлення після створення
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }
        // ✅ ДОДАЙ ЦЕЙ МЕТОД
        private void Grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (Grid.SelectedItem is PurchaseRequest selectedRequest)
                {
                    // ✅ Відкриваємо вікно деталей
                    var detailsWindow = new RequestDetailsWindow(selectedRequest);
                    detailsWindow.Owner = Window.GetWindow(this);
                    detailsWindow.ShowDialog();

                    // ✅ Оновлюємо список після закриття
                    SafeLoad();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка перегляду: {ex.Message}", "Помилка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
