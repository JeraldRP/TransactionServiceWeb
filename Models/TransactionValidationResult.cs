using System.Collections.Generic;

namespace TransactionUploadService.Models
{
    public class TransactionValidationResult
    {
        public bool IsValid { get; set; }  // To indicate if the validation passed or failed
        public List<string> ValidationMessages { get; set; } = new List<string>(); // To store any validation messages
        public int ValidTransactionCount { get; set; }  // Count of valid transactions
    }
}
