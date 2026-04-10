using ProcurementSystem.Data;
using ProcurementSystem.Models;
using ProcurementSystem.Services;

public static class DbInitializer
{
    public static void EnsureAdmin(AppDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Employee" },
                new Role { Name = "Manager" },
                new Role { Name = "Procurement" }
            );
            db.SaveChanges();
        }

        var adminRole = db.Roles.First(r => r.Name == "Admin");

        if (!db.Users.Any(u => u.RoleId == adminRole.Id))
        {
            db.Users.Add(new User
            {
                FullName = "System Administrator",
                Login = "admin",
                PasswordHash = PasswordService.Hash("admin"),
                RoleId = adminRole.Id,
                IsActive = true
            });

            db.SaveChanges();
        }
    }
}
