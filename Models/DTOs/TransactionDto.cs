namespace TransactionUploadService.Models.DTOs
{
    public class TransactionDto
    {
        public string? TransactionId { get; set; } // May be handled elsewhere
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? ReferenceNumber { get; set; }
        public long Quantity { get; set; }
        public string? Name { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Symbol { get; set; }
        public string? OrderSide { get; set; }
        public string? OrderStatus { get; set; }
    }
}

