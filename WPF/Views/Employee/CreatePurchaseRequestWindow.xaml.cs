using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System;
using System.Collections.ObjectModel;  // ✅ ObservableCollection!
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProcurementSystem.Wpf.Views
{
    public partial class CreatePurchaseRequestWindow : Window
    {
        private readonly PurchaseRequestService _service;

        // ✅ ЗМІНА: ObservableCollection замість List!
        public ObservableCollection<PurchaseRequestItem> _items { get; set; } = new();

        public CreatePurchaseRequestWindow()
        {
            InitializeComponent();
            _service = App.Services.GetRequiredService<PurchaseRequestService>();

            // ✅ Автоматичне оновлення DataGrid!
            ItemsGrid.ItemsSource = _items;
            UpdateTotalLabel();
        }

        private void PreviewTextInput_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void PreviewTextInput_NumbersDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, NumberStyles.Any, null, out _);
        }



        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
            {
                MessageBox.Show("❌ Введіть найменування товару!", "Помилка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                ItemNameBox.Focus();
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("❌ Кількість: число > 0!", "Помилка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityBox.Focus();
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("❌ Ціна: число > 0!", "Помилка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceBox.Focus();
                return;
            }

            // ✅ ObservableCollection автоматично оновлює DataGrid!
            _items.Add(new PurchaseRequestItem
            {
                ItemName = ItemNameBox.Text.Trim(),
                Quantity = quantity,
                EstimatedPrice = price
            });

            UpdateTotalLabel();
            ItemNameBox.Clear();
            QuantityBox.Text = "1";
            PriceBox.Text = "0.00";
            ItemNameBox.Focus();
        }

        private void ItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemsGrid.SelectedItem is PurchaseRequestItem selectedItem)
            {
                var result = MessageBox.Show($"Видалити '{selectedItem.ItemName}'?",
                    "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _items.Remove(selectedItem);  // ✅ Автооновлення!
                    UpdateTotalLabel();
                }
            }
        }

        private void UpdateTotalLabel()
        {
            var total = _items.Sum(i => i.Total);  // ✅ Використовуємо Total з моделі
            TotalLabel.Text = $"Разом: {total:C2} грн";
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommentBox.Text.Trim()))
            {
                MessageBox.Show("❌ Введіть коментар!", "Помилка");
                CommentBox.Focus();
                return;
            }

            if (!_items.Any())
            {
                MessageBox.Show("❌ Додайте позиції!", "Помилка");
                return;
            }

            try
            {
                SubmitButton.IsEnabled = false;
                SubmitButton.Content = "⏳ Створюємо...";

                int requestId = _service.Create(CommentBox.Text.Trim());

                foreach (var item in _items)
                {
                    _service.AddItem(requestId, item.ItemName, item.Quantity, item.EstimatedPrice);
                }

                MessageBox.Show($"✅ Заявку #{requestId} СТВОРЕНО!\n\n" +
                               $"📦 Позицій: {_items.Count}\n" +
                               $"💰 Сума: {_items.Sum(i => i.Total):C2}",
                               "УСПІХ!", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ ПОМИЛКА:\n{ex.Message}", "Помилка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "✅ Створити заявку";
            }
        }
    }
}
