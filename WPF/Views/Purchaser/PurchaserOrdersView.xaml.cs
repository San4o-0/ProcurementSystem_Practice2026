using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcurementSystem.Wpf.Views
{
    public partial class PurchaserOrdersView : UserControl
    {
        private readonly PurchaseRequestService _service;

        public PurchaserOrdersView()
        {
            InitializeComponent();
            _service = App.Services.GetRequiredService<PurchaseRequestService>();
            LoadApprovedRequests();

        }

        private void LoadApprovedRequests()
        {
            try
            {
                Grid.ItemsSource = _service.GetApprovedForPurchasing();
            }
            catch
            {
                Grid.ItemsSource = new System.Collections.Generic.List<object>();
            }
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                // ✅ ДОДАЙ перевірку перед InputBox
                var request = _service.GetById(requestId);
                if (request?.Status != "Approved")
                {
                    MessageBox.Show($"❌ Заявка #{requestId} не може бути оформлена!\nСтатус: {request?.Status ?? "Не існує"}");
                    return;
                }

                var supplier = Microsoft.VisualBasic.Interaction.InputBox(
                    "Назва постачальника:",
                    "Оформити замовлення",
                    "ТОВ 'Постачальник'");

                if (string.IsNullOrWhiteSpace(supplier)) return;

                try
                {
                    _service.CreatePurchaseOrder(requestId, supplier);
                    LoadApprovedRequests();
                    MessageBox.Show($"✅ Замовлення оформлено для заявки #{requestId} у '{supplier}'!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ {ex.Message}");
                }
            }
        }
        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Grid.SelectedItem is PurchaseRequest selectedRequest)
                {
                    var detailsWindow = new RequestDetailsWindow(selectedRequest);
                    detailsWindow.Owner = Window.GetWindow(this);
                    detailsWindow.ShowDialog();

                    // ✅ Оновлюємо список після перегляду
                    LoadApprovedRequests();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Помилка перегляду: {ex.Message}", "Помилка");
            }
        }

    }
}
