using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProcurementSystem.Models
{
    [Table("PurchaseRequests")]
    public class PurchaseRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public User CreatedByUser { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        // Created, Submitted, Approved, Rejected, Ordered

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseRequestItem> Items { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}
