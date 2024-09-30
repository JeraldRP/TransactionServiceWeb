using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using TransactionUploadService.Services;
using TransactionUploadService.Models; 


namespace TransactionUploadService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTransactionFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Check file size (limit to 1 MB)
            if (file.Length > 1 * 1024 * 1024) // 1 MB
            {
                return BadRequest("File size exceeds the maximum limit of 1 MB.");
            }

            if (!file.FileName.EndsWith(".csv"))
            {
                return BadRequest("Invalid file type. Please upload a CSV file.");
            }

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);

            // Save the file to a temporary location
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Process the transactions and get validation results
            var validationResult = _transactionService.ProcessTransactionsFromCsv(filePath);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ValidationMessages);
            }

            return Ok(new { message = $"{validationResult.ValidTransactionCount} transactions processed successfully." });
        }
    }
}
