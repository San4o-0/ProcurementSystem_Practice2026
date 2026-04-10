using Azure.Core;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProcurementSystem.Data;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using ProcurementSystem.Wpf.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProcurementSystem.Services
{
    public class PurchaseRequestService
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;

        public PurchaseRequestService(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public int CreateWithItems(string comment, List<PurchaseRequestItem> items)
        {
            var request = new PurchaseRequest
            {
                CreatedByUserId = UserSession.CurrentUser.Id,
                Status = "Submitted",
                Comment = comment,
                Items = items
            };

            _db.PurchaseRequests.Add(request);
            _db.SaveChanges();

            foreach (var item in items)
                item.PurchaseRequestId = request.Id;

            _db.SaveChanges();
            _audit.Log($"Створено заявку на закупівлю #{request.Id}", request.Id);
            return request.Id;
        }

        public List<PurchaseRequest> GetMy()
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.Id <= 0)
                return new List<PurchaseRequest>();

            return _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .Where(r => r.CreatedByUserId == UserSession.CurrentUser.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<PurchaseRequest> GetPendingApproval()
        {
            return _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .Where(r => r.Status == "Submitted")
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public void Approve(int requestId)
        {
            var request = _db.PurchaseRequests.Include(r => r.Items).FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                throw new InvalidOperationException($"Заявка #{requestId} не існує");
            }

            request.Status = "Approved";
            _db.SaveChanges();
            _audit.Log($"Заявку #{requestId} погоджено", requestId);
        }

        public void Reject(int requestId, string reason)
        {
            var request = _db.PurchaseRequests.Find(requestId);
            request.Status = "Rejected";
            request.Comment += "\nВідхилено: " + reason;
            _db.SaveChanges();
            _audit.Log($"Заявку #{requestId} відхилено", requestId);
        }

        public List<PurchaseRequest> GetApprovedForPurchasing()
        {
            return _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .Where(r => r.Status == "Approved")
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public void CreatePurchaseOrder(int requestId, string supplier)
        {
            var request = _db.PurchaseRequests
                .Include(r => r.Items)
                .FirstOrDefault(r => r.Id == requestId && r.Status == "Approved");

            if (request == null)
                throw new InvalidOperationException($"❌ Заявка #{requestId} не Approved");

            if (request.CreatedByUserId <= 0)
                throw new InvalidOperationException($"❌ CreatedByUserId: {request.CreatedByUserId}");

            request.Status = "Ordered";
            _db.SaveChanges();

            var order = new PurchaseOrder
            {
                PurchaseRequestId = requestId,
                Supplier = supplier ?? "Не вказано",
                OrderDate = DateTime.UtcNow
            };
            _db.PurchaseOrders.Add(order);
            _db.SaveChanges();

            try
            {
                _audit.Log($"✅ Замовлення для заявки #{requestId} створено. Постачальник {supplier}" , requestId);
            }
            catch
            {

            }
        }

        public decimal GetRequestTotal(int requestId)
        {
            return _db.PurchaseRequestItems
                .Where(i => i.PurchaseRequestId == requestId)
                .Sum(i => i.Quantity * i.EstimatedPrice);
        }

        public int Create(string comment)
        {
            var request = new PurchaseRequest
            {
                CreatedByUserId = UserSession.CurrentUser.Id,
                Status = "Submitted",
                Comment = comment
            };

            _db.PurchaseRequests.Add(request);
            _db.SaveChanges();
            _audit.Log($"Створено заявку на закупівлю #{request.Id}");

            return request.Id;
        }

        public void AddItem(int requestId, string name, int qty, decimal price)
        {
            _db.PurchaseRequestItems.Add(new PurchaseRequestItem
            {
                PurchaseRequestId = requestId,
                ItemName = name,
                Quantity = qty,
                EstimatedPrice = price
            });
            _db.SaveChanges();
        }

        public List<PurchaseOrder> GetMyPurchaseOrders()
        {
            return _db.PurchaseOrders
                .Include(o => o.PurchaseRequest)
                    .ThenInclude(r => r.CreatedByUser)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }
        public List<PurchaseRequest> GetAllRequests()
        {
            return _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<PurchaseOrder> GetAllOrders()
        {
            return _db.PurchaseOrders
                .Include(o => o.PurchaseRequest)
                    .ThenInclude(r => r.CreatedByUser)
                .Include(o => o.PurchaseRequest.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }
        public PurchaseRequest GetById(int id)
        {
            return _db.PurchaseRequests
                .Include(r => r.Items)     
                .Include(r => r.CreatedByUser)
                .FirstOrDefault(r => r.Id == id);
        }
        public List<PurchaseRequest> GetTopExpensiveRequests(int count = 10)
        {
            return _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .OrderByDescending(r => r.Items.Sum(i => i.Quantity * i.EstimatedPrice))
                .Take(count)
                .ToList();
        }

        public List<UserStats> GetUserStatistics()
        {
            var stats = _db.PurchaseRequests
                .Include(r => r.CreatedByUser)
                .GroupBy(r => r.CreatedByUser.FullName)
                .Select(g => new UserStats
                {
                    FullName = g.Key,
                    RequestsCount = g.Count(),
                    TotalAmount = g.Sum(r => r.Items.Sum(i => i.Quantity * i.EstimatedPrice)),
                    ApprovedCount = g.Count(r => r.Status == "Approved")
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            foreach (var stat in stats)
            {
                stat.AvgAmount = stat.RequestsCount > 0 ? stat.TotalAmount / stat.RequestsCount : 0;
                stat.ApprovalRate = stat.RequestsCount > 0 ? (decimal)stat.ApprovedCount / stat.RequestsCount : 0;
            }
            return stats;
        }

        public List<StatusStats> GetStatusStatistics(DateTime? from = null, DateTime? to = null)
        {
            var query = _db.PurchaseRequests.AsQueryable();

            if (from.HasValue) query = query.Where(r => r.CreatedAt.Date >= from.Value.Date);
            if (to.HasValue) query = query.Where(r => r.CreatedAt.Date <= to.Value.Date);

            return query.GroupBy(r => r.Status)
                .Select(g => new StatusStats
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(r => r.Items.Sum(i => i.Quantity * i.EstimatedPrice))
                })
                .OrderByDescending(s => s.Count)
                .ToList();
        }

        public class UserStats
        {
            public string FullName { get; set; }
            public int RequestsCount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal AvgAmount { get; set; }
            public int ApprovedCount { get; set; }
            public decimal ApprovalRate { get; set; }
        }

        public class StatusStats
        {
            public string Status { get; set; }
            public int Count { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public string GetDatabaseName()
        {
            return _db.Database.GetDbConnection().Database;
        }

        public void DatabaseBackup(string dbName, string backupPath)
        {
            string sql = $@"
        BACKUP DATABASE [{dbName}] 
        TO DISK = N'{backupPath.Replace(@"\", @"\\")}' 
        WITH FORMAT, 
             MEDIANAME = N'ProcurementDB-Full',
             NAME = N'ProcurementDB-Full Backup',
             STATS = 10";

            _db.Database.ExecuteSqlRaw(sql);
        }
        public void DatabaseRestore(string dbName, string backupPath)
        {
            string connectionString = _db.Database.GetConnectionString();

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $@"
        RESTORE DATABASE [{dbName}] 
        FROM DISK = N'{backupPath.Replace(@"\", @"\\")}' 
        WITH REPLACE, RECOVERY";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
            cmd.ExecuteNonQuery();
        }


        public void ImportRequest(PurchaseRequest request)
        {
            var existing = _db.PurchaseRequests.Find(request.Id);
            if (existing == null)
            {
                request.Id = 0;
                _db.PurchaseRequests.Add(request);
                _db.SaveChanges();
            }
        }

        public void ImportOrder(PurchaseOrder order)
        {
            var existing = _db.PurchaseOrders.Find(order.Id);
            if (existing == null)
            {
                order.Id = 0;
                _db.PurchaseOrders.Add(order);
                _db.SaveChanges();
            }
        }

        public void ImportUser(User user)
        {
            var existing = _db.Users.Find(user.Id);
            if (existing == null)
            {
                user.Id = 0;
                _db.Users.Add(user);
                _db.SaveChanges();
            }
        }

        public void ImportItem(PurchaseRequestItem item)
        {
            var existing = _db.PurchaseRequestItems.Find(item.Id);
            if (existing == null)
            {
                item.Id = 0;
                _db.PurchaseRequestItems.Add(item);
                _db.SaveChanges();
            }
        }

        public void ImportAuditLog(AuditLog log)
        {
            var existing = _db.AuditLogs.Find(log.Id);
            if (existing == null)
            {
                log.Id = 0;
                _db.AuditLogs.Add(log);
                _db.SaveChanges();
            }
        }

        public void CreateOrUpdateOrder(PurchaseOrder order)
        {
            var existing = _db.PurchaseOrders.Find(order.Id);
            if (existing != null)
            {
                _db.Entry(existing).CurrentValues.SetValues(order);
            }
            else
            {
                _db.PurchaseOrders.Add(order);
            }
            _db.SaveChanges();
        }

        public void CreateOrUpdateRequest(PurchaseRequest request)
        {
            var existing = _db.PurchaseRequests.Find(request.Id);
            if (existing != null)
            {
                _db.Entry(existing).CurrentValues.SetValues(request);
            }
            else
            {
                request.Id = 0;
                _db.PurchaseRequests.Add(request);
            }
            _db.SaveChanges();
        }
        public List<User> GetAllUsers()
        {
            return _db.Users
                .Include(u => u.Role)
                .ToList();
        }
        public List<PurchaseRequestItem> GetAllRequestItems() => _db.PurchaseRequestItems.Include(i => i.PurchaseRequest).ToList();
        public void CreateOrUpdateUser(User user)
        {
            var existing = _db.Users.Find(user.Id);
            if (existing != null)
            {
                _db.Entry(existing).CurrentValues.SetValues(user);
            }
            else
            {
                user.Id = 0;
                _db.Users.Add(user);
            }
            _db.SaveChanges();
        }

        public void CreateOrUpdateItem(PurchaseRequestItem item)
        {
            var existing = _db.PurchaseRequestItems.Find(item.Id);
            if (existing != null)
                _db.Entry(existing).CurrentValues.SetValues(item);
            else
                _db.PurchaseRequestItems.Add(item);
            _db.SaveChanges();
        }
        public List<AuditLog> GetAllAuditLogs()
        {
            return _db.AuditLogs
                .Include(a => a.User)
                .Include(a => a.PurchaseRequest)
                .ToList();
        }
        public void FullResetUsers() { _db.Users.RemoveRange(_db.Users); _db.SaveChanges(); }
        public void FullResetRequests() { _db.PurchaseRequests.RemoveRange(_db.PurchaseRequests); _db.SaveChanges(); }
        public void FullResetItems() { _db.PurchaseRequestItems.RemoveRange(_db.PurchaseRequestItems); _db.SaveChanges(); }
        public void FullResetOrders() { _db.PurchaseOrders.RemoveRange(_db.PurchaseOrders); _db.SaveChanges(); }
        public void FullResetAuditLogs() { _db.AuditLogs.RemoveRange(_db.AuditLogs); _db.SaveChanges(); }

        public void BulkImportUsers(List<User> users)
        {
            if (users.Any()) { _db.Users.AddRange(users); _db.SaveChanges(); }
        }
        public void BulkImportRequests(List<PurchaseRequest> requests)
        {
            if (requests.Any()) { _db.PurchaseRequests.AddRange(requests); _db.SaveChanges(); }
        }
        public void BulkImportItems(List<PurchaseRequestItem> items)
        {
            if (items.Any()) { _db.PurchaseRequestItems.AddRange(items); _db.SaveChanges(); }
        }
        public void BulkImportOrders(List<PurchaseOrder> orders)
        {
            if (orders.Any()) { _db.PurchaseOrders.AddRange(orders); _db.SaveChanges(); }
        }
        public void BulkImportAuditLogs(List<AuditLog> logs)
        {
            if (logs.Any()) { _db.AuditLogs.AddRange(logs); _db.SaveChanges(); }
        }

        public void FullResetAndImportUsers(List<User> users)
        {
            _db.Users.RemoveRange(_db.Users);
            _db.SaveChanges();
            if (users.Any())
            {
                _db.Users.AddRange(users);
                _db.SaveChanges();
            }
        }

        public void FullResetAndImportRequests(List<PurchaseRequest> requests)
        {
            _db.PurchaseRequests.RemoveRange(_db.PurchaseRequests);
            _db.SaveChanges();
            if (requests.Any())
            {
                _db.PurchaseRequests.AddRange(requests);
                _db.SaveChanges();
            }
        }

        public void FullResetAndImportItems(List<PurchaseRequestItem> items)
        {
            _db.PurchaseRequestItems.RemoveRange(_db.PurchaseRequestItems);
            _db.SaveChanges();
            if (items.Any())
            {
                _db.PurchaseRequestItems.AddRange(items);
                _db.SaveChanges();
            }
        }

        public void FullResetAndImportOrders(List<PurchaseOrder> orders)
        {
            _db.PurchaseOrders.RemoveRange(_db.PurchaseOrders);
            _db.SaveChanges();
            if (orders.Any())
            {
                _db.PurchaseOrders.AddRange(orders);
                _db.SaveChanges();
            }
        }

        public void FullResetAndImportAuditLogs(List<AuditLog> logs)
        {
            _db.AuditLogs.RemoveRange(_db.AuditLogs);
            _db.SaveChanges();
            if (logs.Any())
            {
                _db.AuditLogs.AddRange(logs);
                _db.SaveChanges();
            }
        }

        public void SafeImportRequest(PurchaseRequest request)
        {
            var existing = _db.PurchaseRequests.Find(request.Id);
            if (existing == null)
            {
                request.Id = 0;
                _db.PurchaseRequests.Add(request);
                _db.SaveChanges();
            }
        }

    }
}