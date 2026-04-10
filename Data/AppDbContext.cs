using Microsoft.EntityFrameworkCore;
using ProcurementSystem.Models;

namespace ProcurementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Login).IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Users_Roles");
            });



            modelBuilder.Entity<PurchaseRequestItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PurchaseRequestId);

                entity.Property(e => e.EstimatedPrice)
                    .HasPrecision(10, 2);

                entity.HasOne(e => e.PurchaseRequest)
                    .WithMany(pr => pr.Items)
                    .HasForeignKey(e => e.PurchaseRequestId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PurchaseRequestItems_PurchaseRequests");
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderDate);

                entity.Property(e => e.OrderDate)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.HasOne(e => e.PurchaseRequest)
                    .WithOne(pr => pr.PurchaseOrder)
                    .HasForeignKey<PurchaseOrder>(e => e.PurchaseRequestId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PurchaseOrders_PurchaseRequests");
            });

            modelBuilder.Entity<PurchaseRequest>(entity =>
            {
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.PurchaseRequests) 
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Cascade)  
                    .HasConstraintName("FK_PurchaseRequests_Users");
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)      
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AuditLogs_Users");
            });
        }
    }
}
