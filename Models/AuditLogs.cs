using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProcurementSystem.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Action { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int? PurchaseRequestId { get; set; }

        [ForeignKey(nameof(PurchaseRequestId))]
        public PurchaseRequest PurchaseRequest { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}
