using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Services;
using System.Windows;
using System.Windows.Controls;

namespace ProcurementSystem.Wpf.Views
{
    public partial class ManagerApprovalView : UserControl
    {
        private readonly PurchaseRequestService _service;

        public ManagerApprovalView()
        {
            InitializeComponent();
            _service = App.Services.GetRequiredService<PurchaseRequestService>();
            LoadRequests();
        }

        private void LoadRequests()
        {
            Grid.ItemsSource = _service.GetPendingApproval();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                _service.Approve(requestId);
                LoadRequests();
                MessageBox.Show($"Заявку #{requestId} погоджено!");
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                var reason = Microsoft.VisualBasic.Interaction.InputBox("Причина відмови:", "Відхилити заявку");
                if (!string.IsNullOrEmpty(reason))
                {
                    _service.Reject(requestId, reason);
                    LoadRequests();
                    MessageBox.Show($"Заявку #{requestId} відхилено!");
                }
            }
        }
        private void Grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (Grid.SelectedItem is Models.PurchaseRequest selectedRequest)
                {
                    var detailsWindow = new Views.RequestDetailsWindow(selectedRequest);
                    detailsWindow.Owner = Window.GetWindow(this);
                    detailsWindow.ShowDialog();

                    // ✅ Оновлюємо список після перегляду
                    LoadRequests();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка перегляду: {ex.Message}", "Помилка");
            }
        }
    }
}
