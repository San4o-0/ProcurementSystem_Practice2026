using ProcurementSystem.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProcurementSystem.Wpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsAdmin => UserSession.IsAdmin;
        public bool IsEmployee => UserSession.IsEmployee;
        public bool IsManager => UserSession.IsManager;
        public bool IsPurchaser => UserSession.IsPurchaser;
        public Visibility PurchaserTabVisibility => IsPurchaser ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AdminTabVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EmployeeTabVisibility => IsEmployee ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ManagerTabVisibility => IsManager ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LogsTabVisibility =>
    UserSession.CurrentUser != null ? Visibility.Visible : Visibility.Collapsed;




        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UserSession.PropertyChanged += OnUserSessionChanged;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectFirstVisibleTab();
        }

        private void SelectFirstVisibleTab()
        {
            if (FindName("TabControl") is TabControl tabControl)
            {
                for (int i = 0; i < tabControl.Items.Count; i++)
                {
                    if (tabControl.Items[i] is TabItem tabItem && tabItem.Visibility == Visibility.Visible)
                    {
                        tabControl.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void OnUserSessionChanged(object? sender, PropertyChangedEventArgs e)
        {
            // ✅ ПРАВИЛЬНИЙ виклик - наш власний PropertyChanged
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdmin)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmployee)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdminTabVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmployeeTabVisibility)));

            SelectFirstVisibleTab();
        }

        // ✅ ВЛАСНИЙ метод OnPropertyChanged (НЕ з Window!)
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosed(EventArgs e)
        {
            UserSession.PropertyChanged -= OnUserSessionChanged;
            base.OnClosed(e);
        }
        public event Action OnOrderCreated; // ✅ Подія

       

    }
}
    