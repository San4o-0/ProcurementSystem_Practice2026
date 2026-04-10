using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProcurementSystem.Models
{
    [Table("PurchaseRequestItems")]
    public class PurchaseRequestItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PurchaseRequestId { get; set; }

        [ForeignKey(nameof(PurchaseRequestId))]
        public PurchaseRequest PurchaseRequest { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal EstimatedPrice { get; set; }
        public decimal Total => Quantity * EstimatedPrice;

    }
}
