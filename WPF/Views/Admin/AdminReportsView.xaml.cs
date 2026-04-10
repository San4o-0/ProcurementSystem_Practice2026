using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace ProcurementSystem.Wpf.Views
{
    public partial class AdminReportsView : UserControl
    {
        private readonly PurchaseRequestService _requestService;

        public AdminReportsView()
        {
            InitializeComponent();
            _requestService = App.Services.GetRequiredService<PurchaseRequestService>();
        }

       

        private string GetReportTitle()
        {
            if (RequestsReportBtn.IsFocused || OrdersReportBtn.IsFocused)
                return "Звіт_Заявки";
            return "Звіт_Замовлення";
        }


        private void ClearDatesBtn_Click(object sender, RoutedEventArgs e)
        {
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            StatusText.Text = "✅ Фільтр дат очищено";
        }

        // ✅ ОНОВЛЕНІ методи звітів З ФІЛЬТРОМ
        private void RequestsReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "📊 Фільтруємо заявки по датах...";
                var requests = _requestService.GetAllRequests();

                // ✅ ФІЛЬТР ДАТ
                if (DateFromPicker.SelectedDate.HasValue)
                    requests = requests.Where(r => r.CreatedAt.Date >= DateFromPicker.SelectedDate.Value.Date).ToList();

                if (DateToPicker.SelectedDate.HasValue)
                    requests = requests.Where(r => r.CreatedAt.Date <= DateToPicker.SelectedDate.Value.Date).ToList();

                var reportData = requests.Select(r => new
                {
                    Id = r.Id.ToString(),
                    Date = r.CreatedAt.ToString("dd.MM.yy HH:mm"),
                    User = r.CreatedByUser?.FullName ?? "N/A",
                    Status = r.Status.ToString(),
                    Comment = r.Comment ?? "",
                    TotalAmount = r.Items != null ? r.Items.Sum(i => i.Quantity * i.EstimatedPrice) : 0m
                }).ToList();

                // Агрегація
                decimal totalSum = reportData.Sum(x => x.TotalAmount);
                int recordCount = reportData.Count;

                reportData.Add(new
                {
                    Id = "ЗАГАЛОМ",
                    Date = "",
                    User = "",
                    Status = $"Записів: {recordCount}",
                    Comment = "",
                    TotalAmount = totalSum
                });

                ReportsGrid.ItemsSource = reportData;
                StatusText.Text = $"✅ {recordCount} заявок ({GetDateRangeText()}) | {totalSum:N2} грн";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }

        private string GetDateRangeText()
        {
            if (!DateFromPicker.SelectedDate.HasValue && !DateToPicker.SelectedDate.HasValue)
                return "всі дати";

            string from = DateFromPicker.SelectedDate?.ToString("dd.MM.yy") ?? "початок";
            string to = DateToPicker.SelectedDate?.ToString("dd.MM.yy") ?? "сьогодні";
            return $"з {from} по {to}";
        }


        private void OrdersReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "📋 Фільтруємо замовлення по датах...";
                var orders = _requestService.GetAllOrders();

                // ✅ ФІЛЬТР ДАТ ПО OrderDate
                if (DateFromPicker.SelectedDate.HasValue)
                    orders = orders.Where(o => o.OrderDate.Date >= DateFromPicker.SelectedDate.Value.Date).ToList();

                if (DateToPicker.SelectedDate.HasValue)
                    orders = orders.Where(o => o.OrderDate.Date <= DateToPicker.SelectedDate.Value.Date).ToList();

                var reportData = orders.Select(o => new
                {
                    Id = o.Id.ToString(),
                    Date = o.OrderDate.ToString("dd.MM.yy HH:mm"),
                    User = o.PurchaseRequest?.CreatedByUser?.FullName ?? "N/A",
                    Status = o.PurchaseRequest?.Status.ToString() ?? "N/A",
                    Comment = o.PurchaseRequest?.Comment ?? "",
                    TotalAmount = o.PurchaseRequest?.Items != null ?
                        o.PurchaseRequest.Items.Sum(i => i.Quantity * i.EstimatedPrice) : 0m
                }).ToList();

                decimal totalSum = reportData.Sum(x => x.TotalAmount);
                int recordCount = reportData.Count;

                reportData.Add(new
                {
                    Id = "ЗАГАЛОМ",
                    Date = "",
                    User = "",
                    Status = $"Замовлень: {recordCount}",
                    Comment = "",
                    TotalAmount = totalSum
                });

                ReportsGrid.ItemsSource = reportData;
                StatusText.Text = $"✅ {recordCount} замовлень ({GetDateRangeText()}) | {totalSum:N2} грн";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }
        private void ExcelExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsGrid.ItemsSource == null || ReportsGrid.Items.Count == 0)
            {
                StatusText.Text = "❌ Спочатку сформуйте звіт!";
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"Звіт_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                decimal grandTotal = 0;  // ✅ ОГОЛОШЕНО ЗВЕРХУ, ДО using

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Звіт");

                    // Заголовки
                    string[] headers = { "ID", "Дата", "Користувач", "Статус", "Коментар", "Сума, грн" };
                    for (int col = 0; col < headers.Length; col++)
                        worksheet.Cell(1, col + 1).Value = headers[col];

                    // Дані
                    int row = 2;
                    foreach (dynamic item in ReportsGrid.ItemsSource)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.Date;
                        worksheet.Cell(row, 3).Value = item.User;
                        worksheet.Cell(row, 4).Value = item.Status;
                        worksheet.Cell(row, 5).Value = item.Comment;

                        if (decimal.TryParse(item.TotalAmount?.ToString(), out decimal amount))
                        {
                            worksheet.Cell(row, 6).Value = amount;
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                            grandTotal += amount;  // ✅ Тепер доступний
                        }
                        row++;
                    }

                    // Підсумковий рядок
                    int summaryRow = row;
                    worksheet.Cell(summaryRow, 1).Value = "ЗАГАЛОМ";
                    worksheet.Cell(summaryRow, 4).Value = $"Записів: {ReportsGrid.Items.Count - 1}";
                    worksheet.Cell(summaryRow, 6).Value = grandTotal;  // ✅ Доступний
                    worksheet.Cell(summaryRow, 6).Style.NumberFormat.Format = "#,##0.00";

                    // Форматування
                    worksheet.Range(1, 1, 1, 6).Style.Font.Bold = true;
                    worksheet.Range(1, 1, summaryRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                    worksheet.Columns(1, 6).AdjustToContents();

                    workbook.SaveAs(saveDialog.FileName);
                }  // ✅ using закінчується тут

                // ✅ grandTotal доступний тут
                StatusText.Text = $"✅ Збережено! {grandTotal:N2} грн | {ReportsGrid.Items.Count - 1} записів";
                MessageBox.Show($"📊 Звіт:\n• Записів: {ReportsGrid.Items.Count - 1}\n• Сума: {grandTotal:N2} грн", "Готово!");
            }
        }





        private void ExportRequestsToCsv(string backupDir)
        {
            var requests = _requestService.GetAllRequests();
            var csv = "Id;Дата;Користувач;Status;Коментар;Сума\n" +
                     string.Join("\n", requests.Select(r =>
                         $"{r.Id};{r.CreatedAt:dd.MM.yy HH:mm};" +
                         $"{r.CreatedByUser?.FullName ?? "N/A"};" +
                         $"{r.Status};" +  // ✅ string статус
                         $"{(r.Comment ?? "").Replace(";", "/")};" +
                         $"{r.Items?.Sum(i => i.Quantity * i.EstimatedPrice) ?? 0:N2}"));

            File.WriteAllText(Path.Combine(backupDir, "requests.csv"), csv, System.Text.Encoding.UTF8);
        }


        private void ExportOrdersToCsv(string backupDir)
        {
            var orders = _requestService.GetAllOrders();
            var csv = "ID;Дата;Постачальник;Заявка;Сума\n" +
                     string.Join("\n", orders.Select(o =>
                         $"{o.Id};{o.OrderDate:dd.MM.yy HH:mm};" +
                         $"{o.Supplier ?? "N/A"};{o.PurchaseRequestId};" +
                         $"{o.PurchaseRequest?.Items?.Sum(i => i.Quantity * i.EstimatedPrice) ?? 0:F2}"));

            File.WriteAllText(Path.Combine(backupDir, "orders.csv"), csv, System.Text.Encoding.UTF8);
        }
        private void Top10Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "🏆 Завантажуємо топ-10 найдорожчих...";
                var topRequests = _requestService.GetTopExpensiveRequests(10);

                var reportData = topRequests.Select(r => new
                {
                    Id = $"#{r.Id}",
                    Date = r.CreatedAt.ToString("dd.MM.yy"),
                    User = r.CreatedByUser?.FullName ?? "N/A",
                    Status = r.Status.ToString(),
                    Comment = r.Comment?.Substring(0, Math.Min(50, r.Comment?.Length ?? 0)) + "...",
                    TotalAmount = r.Items.Sum(i => i.Quantity * i.EstimatedPrice)
                }).ToList();

                decimal grandTotal = reportData.Sum(x => x.TotalAmount);
                reportData.Add(new { Id = "🏆 ТОП-10", Date = "", User = "", Status = $"10 заявок", Comment = "", TotalAmount = grandTotal });

                ReportsGrid.ItemsSource = reportData;
                StatusText.Text = $"🏆 Топ-10 заявок | {grandTotal:N0} грн ({topRequests.Count} записів)";
                ReportsGrid.Columns[0].Header = "Ранг";
                ReportsGrid.Columns[1].Header = "Дата";
                ReportsGrid.Columns[2].Header = "Автор";
                ReportsGrid.Columns[3].Header = "Статус";
                ReportsGrid.Columns[4].Header = "Коментар";
                ReportsGrid.Columns[5].Header = "💰 Сума";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }

        private void UserReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "👥 Аналізуємо користувачів...";
                var userStats = _requestService.GetUserStatistics();

                var reportData = userStats.Select(u => new
                {
                    Id = "",
                    Date = u.RequestsCount.ToString(),
                    User = u.FullName,
                    Status = $"{u.ApprovalRate:P1}",
                    Comment = $"Середня: {u.AvgAmount:N0} грн",
                    TotalAmount = u.TotalAmount
                }).ToList();

                decimal grandTotal = reportData.Sum(x => x.TotalAmount);
                reportData.Add(new { Id = "", Date = userStats.Count.ToString(), User = "ВСЬОГО", Status = "", Comment = "", TotalAmount = grandTotal });

                ReportsGrid.ItemsSource = reportData;
                StatusText.Text = $"👥 {userStats.Count} користувачів | {grandTotal:N0} грн загалом";
                ReportsGrid.Columns[0].Header = "";
                ReportsGrid.Columns[1].Header = "Заявки";
                ReportsGrid.Columns[2].Header = "Користувач";
                ReportsGrid.Columns[3].Header = "% Погоджень";
                ReportsGrid.Columns[4].Header = "Середня сума";
                ReportsGrid.Columns[5].Header = "Всього";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }

        private void StatusReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "📊 Аналіз по статусах...";
                var statusStats = _requestService.GetStatusStatistics(
                    DateFromPicker.SelectedDate,
                    DateToPicker.SelectedDate);

                var reportData = statusStats.Select(s => new
                {
                    Id = s.Count.ToString(),
                    Date = "",
                    User = s.Status,
                    Status = "",
                    Comment = "",
                    TotalAmount = s.TotalAmount
                }).ToList();

                decimal grandTotal = reportData.Sum(x => x.TotalAmount);
                reportData.Add(new { Id = statusStats.Count.ToString(), Date = "", User = "ВСЬОГО", Status = "", Comment = "", TotalAmount = grandTotal });

                ReportsGrid.ItemsSource = reportData;
                StatusText.Text = $"📊 Статусів: {statusStats.Count} | {grandTotal:N0} грн ({GetDateRangeText()})";
                ReportsGrid.Columns[0].Header = "Кількість";
                ReportsGrid.Columns[1].Header = "";
                ReportsGrid.Columns[2].Header = "Статус";
                ReportsGrid.Columns[3].Header = "";
                ReportsGrid.Columns[4].Header = "";
                ReportsGrid.Columns[5].Header = "Сума";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }
        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "📁 Вибираємо папку бекапу...";
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = "Backup Info|backup_info.txt|CSV|*.csv",
                    Title = "Виберіть backup_info.txt"
                };

                if (dlg.ShowDialog() == true)
                {
                    string backupDir = Path.GetDirectoryName(dlg.FileName);
                    bool isFullReset = ImportModeCombo.SelectedIndex == 1;

                    // ⚠️ Попередження для повного оновлення
                    if (isFullReset)
                    {
                        var result = MessageBox.Show(
                            "⚠️ УВАГА!\n\nВсі дані в базі будуть ВИДАЛЕНІ і замінені на бекап!\n\nПродовжити?",
                            "Повне оновлення БД", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result != MessageBoxResult.Yes) return;
                    }

                    StatusText.Text = $"📊 {(isFullReset ? "ПОВНЕ" : "БЕЗПЕЧНЕ")} відновлення...";
                    var stats = isFullReset
                        ? ImportFullReset(backupDir)
                        : ImportFullBackup(backupDir);

                    StatusText.Text = $"✅ {(isFullReset ? "ПОВНЕ" : "БЕЗПЕЧНЕ")} відновлення OK!\n" +
                                     $"👥 {stats.users} користувачів\n📋 {stats.requests} заявок\n" +
                                     $"📦 {stats.items} товарів\n🧾 {stats.orders} замовлень\n📝 {stats.logs} логів";

                    MessageBox.Show($"✅ {(isFullReset ? "ПОВНЕ" : "БЕЗПЕЧНЕ")} відновлення!\n\n" +
                                   $"👥 {stats.users} користувачів\n📋 {stats.requests} заявок\n" +
                                   $"📦 {stats.items} товарів\n🧾 {stats.orders} замовлень\n📝 {stats.logs} логів",
                                   "Готово!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
            }
        }

        // 🔥 ПОВНЕ ОНОВЛЕННЯ
        private (int requests, int orders, int users, int items, int logs) ImportFullReset(string backupDir)
        {
            StatusText.Text = "💥 1/5 ОЧИЩУЄМО БД...";

            // Збираємо дані з CSV
            var users = ReadUsersFromCsv(Path.Combine(backupDir, "users.csv"));
            var requests = ReadRequestsFromCsv(Path.Combine(backupDir, "requests.csv"));
            var items = ReadItemsFromCsv(Path.Combine(backupDir, "request_items.csv"));
            var orders = ReadOrdersFromCsv(Path.Combine(backupDir, "orders.csv"));
            var logs = ReadAuditLogsFromCsv(Path.Combine(backupDir, "audit_logs.csv"));

            // 🔥 ПОВНЕ оновлення через Service
            _requestService.FullResetUsers();
            _requestService.FullResetRequests();
            _requestService.FullResetItems();
            _requestService.FullResetOrders();
            _requestService.FullResetAuditLogs();

            // Імпортуємо нові
            _requestService.BulkImportUsers(users);
            _requestService.BulkImportRequests(requests);
            _requestService.BulkImportItems(items);
            _requestService.BulkImportOrders(orders);
            _requestService.BulkImportAuditLogs(logs);

            return (requests.Count, orders.Count, users.Count, items.Count, logs.Count);
        }






        private (int requests, int orders, int users, int items, int logs) ImportFullBackup(string backupDir)
        {
            // ✅ ПРАВИЛЬНИЙ ПОРЯДОК + LOGS!
            int users = ImportUsersFromCsv(Path.Combine(backupDir, "users.csv"));
            int requests = ImportRequestsFromCsv(Path.Combine(backupDir, "requests.csv"));
            int items = ImportItemsFromCsv(Path.Combine(backupDir, "request_items.csv"));
            int orders = ImportOrdersFromCsv(Path.Combine(backupDir, "orders.csv"));
            int logs = ImportAuditLogsFromCsv(Path.Combine(backupDir, "audit_logs.csv"));  // ✅ ДОДАНО!

            return (requests, orders, users, items, logs);
        }


        private int ImportRequestsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return 0;

            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1).ToList();
            if (!lines.Any()) return 0;

            int count = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length < 5) continue;

                try
                {
                    // Знайти користувача за FullName з CSV
                    string userName = parts[2];
                    var user = _requestService.GetAllUsers()
                        .FirstOrDefault(u => u.FullName.Contains(userName) || u.Login.Contains(userName));

                    _requestService.ImportRequest(new PurchaseRequest  // ✅ Безпечний!
                    {
                        Id = int.Parse(parts[0]),  // ID для перевірки існування
                        CreatedAt = DateTime.ParseExact(parts[1], "dd.MM.yy HH:mm", null),
                        CreatedByUserId = user?.Id ?? 1,
                        Status = parts[3],
                        Comment = parts[4].Replace("/", ";")  // Відновлюємо ;
                    });
                    count++;
                }
                catch (Exception ex)
                {
                    StatusText.Text += $"\n❌ Заявка '{parts[0]}': {ex.Message}";
                }
            }
            return count;
        }

        private int ImportAuditLogsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return 0;

            int count = 0;
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length < 5) continue;

                try
                {
                    _requestService.ImportAuditLog(new AuditLog  // ✅ Потрібен цей метод!
                    {
                        Id = int.Parse(parts[0]),
                        Action = parts[1],
                        UserId = int.Parse(parts[2]),
                        PurchaseRequestId = int.Parse(parts[3]),
                        ActionDate = DateTime.ParseExact(parts[4], "dd.MM.yy HH:mm", null)
                    });
                    count++;
                }
                catch { }
            }
            return count;
        }


        private int ImportOrdersFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return 0;
            int count = 0;
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length < 4) continue;

                try
                {
                    _requestService.CreateOrUpdateOrder(new PurchaseOrder
                    {
                        Id = int.Parse(parts[0]),
                        OrderDate = DateTime.ParseExact(parts[1], "dd.MM.yy HH:mm", null),
                        Supplier = parts[2],
                        PurchaseRequestId = int.Parse(parts[3])  // ✅ Заявка вже існує!
                    });
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка імпорту замовлення: {line} -> {ex.Message}");
                }
            }
            return count;
        }


        private int ImportUsersFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return 0;

            int count = 0;
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length < 5) continue;  // Тепер 5 колонок!

                try
                {
                    _requestService.CreateOrUpdateUser(new User
                    {
                        Id = int.Parse(parts[0]),
                        FullName = parts[1],
                        Login = parts[2],
                        RoleId = int.Parse(parts[3]),
                        PasswordHash = parts[4].Trim('"'),  // ✅ ОРИГІНАЛЬНИЙ хеш з CSV!
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    count++;
                }
                catch { }
            }
            return count;
        }


        private int ImportItemsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return 0;
            int count = 0;
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length < 5) continue;

                try
                {
                    _requestService.CreateOrUpdateItem(new PurchaseRequestItem
                    {
                        Id = int.Parse(parts[0]),
                        PurchaseRequestId = int.Parse(parts[1]),
                        ItemName = parts[2],
                        Quantity = int.Parse(parts[3]),
                        EstimatedPrice = decimal.Parse(parts[4])
                    });
                    count++;
                }
                catch { }
            }
            return count;
        }


       
        private void BackupBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "💾 Створюємо резервну копію...";
                string backupPath = CreateBackup();

                StatusText.Text = $"✅ Бекап створено: {Path.GetFileName(backupPath)}";

                MessageBox.Show($"💾 Резервна копія готова!\n\n" +
                               $"📁 Шлях: {backupPath}\n\n" +
                               $"🗂️ Файли:\n" +
                               $"• requests.csv\n" +
                               $"• orders.csv\n" +
                               $"• backup_info.txt",
                               "Бекап успішний!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Помилка бекапу: {ex.Message}";
                MessageBox.Show($"❌ {ex.Message}", "Помилка бекапу", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string CreateBackup()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupDir = Path.Combine(Directory.GetCurrentDirectory(), "Backups", timestamp);
            Directory.CreateDirectory(backupDir);

            // ✅ 1. Заявки
            ExportRequestsToCsv(backupDir);

            // ✅ 2. Замовлення  
            ExportOrdersToCsv(backupDir);

            // ✅ 3. Користувачі
            ExportUsersToCsv(backupDir);

            // ✅ 4. Товари заявок (деталі)
            ExportRequestItemsToCsv(backupDir);

            // ✅ 5. AuditLog
            ExportAuditLogsToCsv(backupDir);

            string info = $"📁 Шлях: {backupDir}\n🕒 {timestamp}\n" +
                          $"📋 Заявки: {_requestService.GetAllRequests().Count}\n" +
                          $"📦 Замовлення: {_requestService.GetAllOrders().Count}\n" +
                          $"👥 Користувачі: {_requestService.GetAllUsers().Count}\n" +
                          $"📦 Товари: {_requestService.GetAllRequestItems().Count}";

            File.WriteAllText(Path.Combine(backupDir, "backup_info.txt"), info, System.Text.Encoding.UTF8);
            return backupDir;
        }
        private void ExportAuditLogsToCsv(string backupDir)
        {
            try
            {
                var auditLogs = _requestService.GetAllAuditLogs();
                var csv = "Id;Action;UserId;PurchaseRequestId;ActionDate\n" +
                         string.Join("\n", auditLogs.Select(a =>
                             $"{a.Id};" +
                             $"{a.Action};" +
                             $"{a.UserId};" +
                             $"{a.PurchaseRequestId ?? 0};" +
                             $"{a.ActionDate:dd.MM.yy HH:mm}"));

                File.WriteAllText(Path.Combine(backupDir, "audit_logs.csv"), csv, System.Text.Encoding.UTF8);
            }
            catch
            {
                // Пропускаємо помилки AuditLog
            }
        }


        private void ExportUsersToCsv(string backupDir)
        {
            var users = _requestService.GetAllUsers();
            var csv = "Id;FullName;Login;RoleId;PasswordHash\n" +
                     string.Join("\n", users.Select(u =>
                         $"{u.Id};{u.FullName};{u.Login};{u.RoleId};\"{u.PasswordHash}\""));  // ✅ ОРИГІНАЛЬНИЙ хеш!

            File.WriteAllText(Path.Combine(backupDir, "users.csv"), csv, System.Text.Encoding.UTF8);
        }



        private void ExportRequestItemsToCsv(string backupDir)
        {
            var items = _requestService.GetAllRequestItems();
            var csv = "Id;RequestId;ItemName;Quantity;EstimatedPrice\n" +
                     string.Join("\n", items.Select(i => $"{i.Id};{i.PurchaseRequestId};{i.ItemName};{i.Quantity};{i.EstimatedPrice}"));
            File.WriteAllText(Path.Combine(backupDir, "request_items.csv"), backupDir, System.Text.Encoding.UTF8);
        }


        private List<User> ReadUsersFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return new List<User>();
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           var parts = line.Split(';');
                           if (parts.Length < 5) return null;
                           return new User
                           {
                               Id = 0, // Новий ID
                               FullName = parts[1],
                               Login = parts[2],
                               RoleId = int.Parse(parts[3]),
                               PasswordHash = parts[4].Trim('"'),
                               IsActive = true,
                               CreatedAt = DateTime.UtcNow
                           };
                       }).Where(u => u != null).ToList();
        }

        private List<PurchaseRequest> ReadRequestsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return new List<PurchaseRequest>();
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           var parts = line.Split(';');
                           if (parts.Length < 5) return null;
                           return new PurchaseRequest
                           {
                               Id = 0,
                               CreatedAt = DateTime.ParseExact(parts[1], "dd.MM.yy HH:mm", null),
                               CreatedByUserId = 1, // Буде оновлено після імпорту юзерів
                               Status = parts[3],
                               Comment = parts[4]
                           };
                       }).Where(r => r != null).ToList();
        }

        private List<PurchaseRequestItem> ReadItemsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return new List<PurchaseRequestItem>();
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           var parts = line.Split(';');
                           if (parts.Length < 5) return null;
                           return new PurchaseRequestItem
                           {
                               Id = 0,
                               PurchaseRequestId = int.Parse(parts[1]),
                               ItemName = parts[2],
                               Quantity = int.Parse(parts[3]),
                               EstimatedPrice = decimal.Parse(parts[4])
                           };
                       }).Where(i => i != null).ToList();
        }

        private List<PurchaseOrder> ReadOrdersFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return new List<PurchaseOrder>();
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           var parts = line.Split(';');
                           if (parts.Length < 4) return null;
                           return new PurchaseOrder
                           {
                               Id = 0,
                               OrderDate = DateTime.ParseExact(parts[1], "dd.MM.yy HH:mm", null),
                               Supplier = parts[2],
                               PurchaseRequestId = int.Parse(parts[3])
                           };
                       }).Where(o => o != null).ToList();
        }

        private List<AuditLog> ReadAuditLogsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return new List<AuditLog>();
            var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8).Skip(1);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           var parts = line.Split(';');
                           if (parts.Length < 5) return null;
                           return new AuditLog
                           {
                               Id = 0,
                               Action = parts[1],
                               UserId = int.Parse(parts[2]),
                               PurchaseRequestId = int.Parse(parts[3]),
                               ActionDate = DateTime.ParseExact(parts[4], "dd.MM.yy HH:mm", null)
                           };
                       }).Where(l => l != null).ToList();
        }


    }
}
