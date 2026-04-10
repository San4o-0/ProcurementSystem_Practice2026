using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProcurementSystem.Wpf.Views
{
    public partial class RequestDetailsWindow : Window, INotifyPropertyChanged
    {
        private PurchaseRequest _request;
        public PurchaseRequest Request
        {
            get => _request;
            set
            {
                _request = value;
                OnPropertyChanged();
            }
        }

        public List<RequestItemViewModel> Items { get; set; } = new();

        public RequestDetailsWindow(PurchaseRequest request)
        {
            InitializeComponent();
            Request = request;
            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                var service = App.Services.GetRequiredService<PurchaseRequestService>();
                var fullRequest = service.GetById(Request.Id);

                // ✅ Позиції
                Items.Clear();
                decimal grandTotal = 0;
                if (fullRequest?.Items != null)
                {
                    foreach (var item in fullRequest.Items)
                    {
                        var vm = new RequestItemViewModel
                        {
                            ItemName = item.ItemName,
                            Quantity = item.Quantity,
                            EstimatedPrice = item.EstimatedPrice,
                            Total = item.Quantity * item.EstimatedPrice
                        };
                        Items.Add(vm);
                        grandTotal += vm.Total;  // ✅ Рахуємо суму
                    }
                }
                ItemsGrid.ItemsSource = Items;

                // ✅ ПОКАЗУЄМО ЗАГАЛЬНУ СУМУ
                TotalText.Text = $"💰 Загальна сума: {grandTotal:F0} грн";
            }
            catch
            {
                ItemsGrid.ItemsSource = new List<object>();
            }
        }


        // ✅ ОБОВ'ЯЗКОВИЙ метод
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RequestItemViewModel : INotifyPropertyChanged
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal EstimatedPrice { get; set; }
        public decimal Total { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
