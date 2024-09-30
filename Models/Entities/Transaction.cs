using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransactionUploadService.Services;

namespace TransactionUploadService.Models.Entities
{
    public class Transaction
    {
        [Key]
        [StringLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Reference number must be alphanumeric.")]
        public string? ReferenceNumber { get; set; } // Alphanumeric, max length 20

        [Required]
        public long Quantity { get; set; } // Long

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; } // Decimal

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Invalid name format.")]
        public string? Name { get; set; } // Name validation

        [Required]
        [CustomDateFormat("dd/MM/yyyy HH:mm:ss")]  // Custom date format validation
        public DateTime TransactionDate { get; set; } // Date format dd/MM/yyyy hh:mm:ss (UTC)


        [Required]
        [StringLength(5, MinimumLength = 3)]
        public string? Symbol { get; set; } // Min length 3, max length 5

        [Required]
        [RegularExpression(@"^(Buy|Sell)$", ErrorMessage = "Order side must be 'Buy' or 'Sell'.")]
        public string? OrderSide { get; set; } // Buy or Sell

        [Required]
        [RegularExpression(@"^(Open|Matched|Cancelled)$", ErrorMessage = "Invalid order status.")]
        public string? OrderStatus { get; set; } // Open, Matched, Cancelled
    }
}
