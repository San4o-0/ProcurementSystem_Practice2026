using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProcurementSystem.Models
{
    [Table("PurchaseOrders")]
    public class PurchaseOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PurchaseRequestId { get; set; }

        [ForeignKey(nameof(PurchaseRequestId))]
        public PurchaseRequest PurchaseRequest { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Supplier { get; set; }
    }
}
