using Microsoft.EntityFrameworkCore;
using ProcurementSystem.Data;
using ProcurementSystem.Models;
using ProcurementSystem.Services;

public class AuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

   
    public List<AuditLog> GetAll()
    {
        return _db.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.ActionDate)
            .ToList();
    }   

    public void Log(string action, int? purchaseRequestId = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            UserId = UserSession.CurrentUser.Id,
            PurchaseRequestId = purchaseRequestId,
            ActionDate = DateTime.UtcNow
        });
        _db.SaveChanges();
    }

    public List<AuditLog> GetAllLogs()
    {
        return _db.AuditLogs
            .Include(l => l.User)
            .Include(l => l.PurchaseRequest)
            .OrderByDescending(l => l.ActionDate)
            .Take(200)
            .ToList();
    }

    public List<AuditLog> GetMyRoleLogs()
    {
        if (UserSession.CurrentUser?.Role == null)
            return new List<AuditLog>();

        return _db.AuditLogs
            .Include(l => l.User)
            .Include(l => l.PurchaseRequest)
            .Where(l => l.User.Role.Name == UserSession.CurrentUser.Role.Name)
            .OrderByDescending(l => l.ActionDate)
            .Take(100)
            .ToList();
    }
}
