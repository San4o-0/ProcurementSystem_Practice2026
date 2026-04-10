using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProcurementSystem.Wpf.Views
{
    public partial class AuditLogView : UserControl
    {
        private readonly AuditService _auditService;
        private List<AuditLog> _allLogs;
        private CollectionViewSource _logViewSource;
        private List<User> _availableUsers;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;

        public AuditLogView()
        {
            InitializeComponent();
            _auditService = App.Services.GetRequiredService<AuditService>();
            Loaded += AuditLogView_Loaded;
        }

        private void AuditLogView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                _allLogs = UserSession.IsAdmin
                    ? _auditService.GetAllLogs()
                    : _auditService.GetMyRoleLogs();

                SetupFilters();
            }
            catch
            {
                _allLogs = new List<AuditLog>();
                SetupFilters();
            }
        }

        private void SetupFilters()
        {
            _availableUsers = _allLogs
                .Where(l => l.User != null)
                .Select(l => l.User)
                .DistinctBy(u => u.Id)
                .OrderBy(u => u.FullName)
                .ToList();

            var allUsersList = new List<User>
            {
                new User { Id = 0, FullName = "👤 Всі користувачі" }
            };
            allUsersList.AddRange(_availableUsers);

            UserFilterCombo.ItemsSource = allUsersList;
            UserFilterCombo.DisplayMemberPath = "FullName";
            UserFilterCombo.SelectedValuePath = "Id";
            UserFilterCombo.SelectedIndex = 0;

            _logViewSource = new CollectionViewSource { Source = _allLogs };
            AuditGrid.ItemsSource = _logViewSource.View;
            _logViewSource.Filter += LogFilter;
        }

        private void LogFilter(object sender, FilterEventArgs e)
        {
            var log = e.Item as AuditLog;
            if (log == null || log.User == null)
            {
                e.Accepted = false;
                return;
            }

            bool accepted = true;

            // ✅ Фільтр по користувачу
            if (UserFilterCombo.SelectedItem is User selectedUser && selectedUser.Id > 0)
            {
                accepted &= log.UserId == selectedUser.Id;
            }

            // ✅ Фільтр З/ПО дату
            if (_dateFrom.HasValue && log.ActionDate < _dateFrom.Value)
            {
                accepted = false;
            }
            if (_dateTo.HasValue && log.ActionDate > _dateTo.Value)
            {
                accepted = false;
            }

            e.Accepted = accepted;
        }

        // ✅ ВСІ ОБРАБОТКИ ПОДІЙ З XAML:
        private void UserFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _logViewSource?.View.Refresh();
        }

        private void DateFromPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _dateFrom = DateFromPicker.SelectedDate;
            _logViewSource?.View.Refresh();
        }

        private void DateToPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _dateTo = DateToPicker.SelectedDate;
            _logViewSource?.View.Refresh();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            UserFilterCombo.SelectedIndex = 0;
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            _dateFrom = null;
            _dateTo = null;
            _logViewSource?.View.Refresh();
        }

        // ✅ ЦЕЙ МЕТОД БУВ ВІДСУТНІЙ!
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}
