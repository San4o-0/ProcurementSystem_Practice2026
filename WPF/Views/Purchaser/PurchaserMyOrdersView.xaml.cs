using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcurementSystem.Wpf.Views
{
    public partial class PurchaserMyOrdersView : UserControl
    {
        private readonly PurchaseRequestService _service;

        public PurchaserMyOrdersView()
        {
            InitializeComponent();
            _service = App.Services.GetRequiredService<PurchaseRequestService>();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                OrdersGrid.ItemsSource = _service.GetMyPurchaseOrders();
            }
            catch
            {
                OrdersGrid.ItemsSource = new List<object>();
            }
        }
        public void Refresh()  // ✅ Публічний метод оновлення
        {
            LoadOrders();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh(); // ✅ Оновлення списку
        }
        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (OrdersGrid.SelectedItem is PurchaseOrder selectedOrder)
                {
                    // ✅ Завантажуємо повну заявку з позиціями
                    var fullRequest = _service.GetById(selectedOrder.PurchaseRequestId);

                    if (fullRequest != null)
                    {
                        var detailsWindow = new RequestDetailsWindow(fullRequest);
                        detailsWindow.Owner = Window.GetWindow(this);
                        detailsWindow.ShowDialog();

                        // ✅ Оновлюємо список після перегляду
                        LoadOrders();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Помилка перегляду: {ex.Message}", "Помилка");
            }
        }
    }
}
