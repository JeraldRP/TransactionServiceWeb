using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TransactionUploadService.Models.Entities;
using System.Text;
using System.Transactions;
using ExcelDataReader;
using TransactionUploadService.Data;
using TransactionUploadService.Models;
using Transaction = TransactionUploadService.Models.Entities.Transaction;

namespace TransactionUploadService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult UploadExcel()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "File is empty.";
                return View();
            }

            // Limit file size to a maximum of 1 MB
            if (file.Length > 1048576) // 1 MB in bytes
            {
                ViewBag.Message = "File size exceeds the 1 MB limit.";
                return View();
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (fileExtension == ".xlsx" || fileExtension == ".xls")
                {
                    using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            await ProcessExcelReader(reader);
                        }
                    }
                }
                else if (fileExtension == ".csv")
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        await ProcessCsvReader(reader);
                    }
                }
                else
                {
                    ViewBag.Message = "Unsupported file format. Please upload a CSV or Excel file.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred while processing the file: {ex.Message}";
                return View();
            }

            ViewBag.Message = "File uploaded and processed successfully.";
            return View();
        }
        public IActionResult ViewLogs()
        {
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "InvalidRecordsLog.txt");

            if (!System.IO.File.Exists(logFilePath))
            {
                ViewBag.Message = "No logs available.";
                return View();
            }

            var logs = System.IO.File.ReadAllLines(logFilePath);
            return View(logs);
        }


        private async Task ProcessExcelReader(IExcelDataReader reader)
        {
            bool isHeaderSkipped = false;
            bool hasInvalidRecords = false; // Flag for overall file validity
            List<string> invalidRecords = new List<string>(); // List to store invalid records

            while (reader.Read())
            {
                if (!isHeaderSkipped)
                {
                    isHeaderSkipped = true;
                    continue; // Skip the header line
                }

                // Read values from the current row
                var referenceNumber = reader.GetValue(1)?.ToString() ?? string.Empty;
                var quantity = reader.GetValue(2);
                var amount = reader.GetValue(3);
                var name = reader.GetValue(4)?.ToString() ?? string.Empty;
                var transactionDate = reader.GetValue(5);
                var symbol = reader.GetValue(6)?.ToString() ?? string.Empty;
                var orderSide = reader.GetValue(7)?.ToString() ?? string.Empty;
                var orderStatus = reader.GetValue(8)?.ToString() ?? string.Empty;

                // Check for mandatory fields
                if (string.IsNullOrWhiteSpace(referenceNumber) ||
                    quantity == null ||
                    amount == null ||
                    string.IsNullOrWhiteSpace(name) ||
                    transactionDate == null ||
                    string.IsNullOrWhiteSpace(symbol) ||
                    string.IsNullOrWhiteSpace(orderSide) ||
                    string.IsNullOrWhiteSpace(orderStatus))
                {
                    hasInvalidRecords = true; // Mark as having invalid records
                    invalidRecords.Add(reader.GetString(0)); // Add the whole row or a specific identifier to log
                    continue; // Skip processing this record
                }

                try
                {
                    Transaction t = new Transaction
                    {
                        ReferenceNumber = referenceNumber,
                        Quantity = ConvertToLong(quantity),
                        Amount = ConvertToDecimal(amount),
                        Name = name,
                        TransactionDate = ConvertToDateTime(transactionDate),
                        Symbol = symbol,
                        OrderSide = orderSide,
                        OrderStatus = orderStatus,
                    };

                    _context.Add(t);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    hasInvalidRecords = true; // Mark as having invalid records
                    invalidRecords.Add(reader.GetString(0)); // Log the record
                    Console.WriteLine($"Exception: {ex.Message}"); // Log exception to console
                }
            }

            // If there are any invalid records, set the overall message and log them
            if (hasInvalidRecords)
            {
                ViewBag.Message = "The file contains invalid records and cannot be imported.";
                LogInvalidRecords(invalidRecords); // Call method to log invalid records
                return; // Exit early, do not proceed with valid import
            }

            ViewBag.Message = "success"; // Optional: Success message for valid records
        }


        private async Task ProcessCsvReader(StreamReader reader)
        {
            string? line;
            bool isHeaderSkipped = false;
            bool hasInvalidRecords = false; // Flag for overall file validity
            List<string> invalidRecords = new List<string>(); // List to store invalid records

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!isHeaderSkipped)
                {
                    isHeaderSkipped = true;
                    continue; // Skip the header line
                }

                var values = line.Split(',');

                // Check for mandatory fields; assume there are 9 columns (adjust as needed)
                if (values.Length < 9 || values.Any(string.IsNullOrWhiteSpace))
                {
                    hasInvalidRecords = true; // Mark as having invalid records
                    invalidRecords.Add(line); // Log the invalid record
                    continue; // Skip processing this record
                }

                try
                {
                    Transaction t = new Transaction
                    {
                        ReferenceNumber = values[1]?.Trim() ?? string.Empty,
                        Quantity = ConvertToLong(values[2]),
                        Amount = ConvertToDecimal(values[3]),
                        Name = values[4]?.Trim() ?? string.Empty,
                        TransactionDate = ConvertToDateTime(values[5]),
                        Symbol = values[6]?.Trim() ?? string.Empty,
                        OrderSide = values[7]?.Trim() ?? string.Empty,
                        OrderStatus = values[8]?.Trim() ?? string.Empty,
                    };

                    _context.Add(t);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    hasInvalidRecords = true; // Mark as having invalid records
                    invalidRecords.Add(line); // Log the invalid record
                    Console.WriteLine($"Exception: {ex.Message}"); // Log exception to console
                }
            }

            // If there are any invalid records, set the overall message and log them
            if (hasInvalidRecords)
            {
                ViewBag.Message = "The file contains invalid records and cannot be imported.";
                LogInvalidRecords(invalidRecords); // Call method to log invalid records
                return; // Exit early, do not proceed with valid import
            }

            ViewBag.Message = "success"; // Optional: Success message for valid records
        }

        private void LogInvalidRecords(List<string> invalidRecords)
        {
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "InvalidRecordsLog.txt");

            using (var writer = new StreamWriter(logFilePath, true)) // Append mode
            {
                foreach (var record in invalidRecords)
                {
                    writer.WriteLine(record);
                }
            }
        }



        // Helper methods to convert types
        private decimal ConvertToDecimal(object value)
        {
            if (value == null) return 0m; // Handle null value, return 0 as decimal
            decimal result;

            // Attempt to convert the value to decimal
            if (decimal.TryParse(value.ToString(), out result))
            {
                return result;
            }

            // Optionally, you can handle the case where conversion fails
            throw new FormatException($"Unable to convert '{value}' to decimal.");
        }

        private long ConvertToLong(object value)
        {
            if (value == null) return 0; // Handle null value
            long result;

            // Attempt to convert the value to decimal first, then to long
            if (decimal.TryParse(value.ToString(), out decimal decimalValue))
            {
                // You can choose to round or truncate the decimal
                result = Convert.ToInt64(decimalValue); // Rounding
                                                        // result = (long)decimalValue; // Truncating
                return result;
            }

            // Optionally, you can handle the case where conversion fails
            throw new FormatException($"Unable to convert '{value}' to long.");
        }


        private DateTime ConvertToDateTime(object value)
        {
            if (value == null) return DateTime.MinValue; // Handle null value
            return Convert.ToDateTime(value);
        }
    }
}