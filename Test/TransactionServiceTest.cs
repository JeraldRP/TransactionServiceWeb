using TransactionUploadService.Services;
using TransactionUploadService.Data;
using Xunit;

namespace TransactionUploadService.Tests
{
    public class TransactionServiceTests
    {
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            // Initialize your TransactionService with a context
            this._service = new TransactionService(new AppDbContext());
        }
        [Fact]
        public async Task ProcessTransactionsFromCsv_ShouldReturnValidResult_WhenTransactionsAreValid()
        {
            // Arrange: Prepare a valid CSV file content.
            string[] csvContent = new[]
            {
            "123,10,100.00,Test Transaction,01/01/2024 10:00:00,USD,Buy,Completed",
            "124,20,200.00,Another Transaction,01/01/2024 11:00:00,EUR,Sell,Completed"
        };

            string filePath = Path.Combine(Path.GetTempPath(), "valid.csv");
            await File.WriteAllLinesAsync(filePath, csvContent); // Use async method to write the file

            // Act: Call the method being tested.
            var result = await _service.ProcessTransactionsFromCsvAsync(filePath); // Await the async method

            // Assert: Check if the result is as expected.
            Assert.True(result.IsValid);
            Assert.Equal(2, result.ValidTransactionCount);
        }

        [Fact]
        public async Task ProcessTransactionsFromCsv_ShouldReturnInvalidResult_WhenTransactionsAreInvalid()
        {
            // Arrange: Prepare a file path with invalid content.
            string[] csvContent = new[]
            {
            "123,invalid,100.00,Test Transaction,01/01/2024 10:00:00,USD,Buy,Completed",
            "124,20,200.00,Another Transaction,not_a_date,EUR,Sell,Completed"
        };

            string filePath = Path.Combine(Path.GetTempPath(), "invalid.csv");
            await File.WriteAllLinesAsync(filePath, csvContent); // Use async method to write the file

            // Act
            var result = await _service.ProcessTransactionsFromCsvAsync(filePath); // Await the async method

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(0, result.ValidTransactionCount);
            Assert.NotEmpty(result.ValidationMessages);
        }
    }
}
