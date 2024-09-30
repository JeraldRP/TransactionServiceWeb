using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using TransactionUploadService.Models.Entities;
using System.ComponentModel.DataAnnotations;
using TransactionUploadService.Models;
using TransactionUploadService.Data;
using TransactionUploadService.Models.DTOs; // Include the DTO namespace
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using System.Text;

namespace TransactionUploadService.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        // CSV Processing Logic - Remains the same
        public async Task<TransactionValidationResult> ProcessTransactionsFromCsvAsync(string filePath)
        {
            var transactions = new List<Transaction>();
            var invalidRecords = new List<string>();
            var validationResult = new TransactionValidationResult();

            try
            {
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // Support for ExcelDataReader
                var lines = await File.ReadAllLinesAsync(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var fields = line.Split(',');

                    if (fields.Length != 8)
                    {
                        invalidRecords.Add(line);
                        validationResult.ValidationMessages.Add("Invalid record format: " + line);
                        continue;
                    }

                    var transactionDto = CreateTransactionDto(fields);
                    var validation = ValidateTransaction(transactionDto);
                    if (validation.IsValid)
                    {
                        var transaction = MapToEntity(transactionDto);
                        transactions.Add(transaction);
                    }
                    else
                    {
                        invalidRecords.Add(line);
                        validationResult.ValidationMessages.AddRange(validation.ValidationMessages);
                    }
                }

                if (invalidRecords.Count > 0)
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationMessages.Add($"Total invalid records: {invalidRecords.Count}");
                    return validationResult;
                }

                if (transactions.Count > 0)
                {
                    await _context.Transactions.AddRangeAsync(transactions);
                    await _context.SaveChangesAsync();
                }

                validationResult.IsValid = true;
                validationResult.ValidTransactionCount = transactions.Count;
            }
            catch (Exception ex)
            {
                validationResult.ValidationMessages.Add($"Error processing file: {ex.Message}");
            }

            return validationResult;
        }

        // XLSX Processing Logic
        public async Task<TransactionValidationResult> ProcessTransactionsFromXlsxAsync(string filePath)
        {
            var transactions = new List<Transaction>();
            var invalidRecords = new List<string>();
            var validationResult = new TransactionValidationResult();

            try
            {
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // Support for ExcelDataReader
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        // Skip header row
                        if (reader.Depth == 0) continue;

                        var fields = new string[8];
                        for (int i = 0; i < 8; i++)
                        {
                            fields[i] = reader.GetValue(i)?.ToString() ?? string.Empty;
                        }

                        if (fields.Length != 8)
                        {
                            invalidRecords.Add(string.Join(",", fields));
                            validationResult.ValidationMessages.Add("Invalid record format: " + string.Join(",", fields));
                            continue;
                        }

                        var transactionDto = CreateTransactionDto(fields);
                        var validation = ValidateTransaction(transactionDto);
                        if (validation.IsValid)
                        {
                            var transaction = MapToEntity(transactionDto);
                            transactions.Add(transaction);
                        }
                        else
                        {
                            invalidRecords.Add(string.Join(",", fields));
                            validationResult.ValidationMessages.AddRange(validation.ValidationMessages);
                        }
                    }
                }

                if (invalidRecords.Count > 0)
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationMessages.Add($"Total invalid records: {invalidRecords.Count}");
                    return validationResult;
                }

                if (transactions.Count > 0)
                {
                    await _context.Transactions.AddRangeAsync(transactions);
                    await _context.SaveChangesAsync();
                }

                validationResult.IsValid = true;
                validationResult.ValidTransactionCount = transactions.Count;
            }
            catch (Exception ex)
            {
                validationResult.ValidationMessages.Add($"Error processing file: {ex.Message}");
            }

            return validationResult;
        }

        private TransactionDto CreateTransactionDto(string[] fields)
        {
            return new TransactionDto
            {
                ReferenceNumber = fields[0],
                Quantity = long.Parse(fields[1]),
                Amount = decimal.Parse(fields[2]),
                Name = fields[3],
                TransactionDate = DateTime.ParseExact(fields[4], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                Symbol = fields[5],
                OrderSide = fields[6],
                OrderStatus = fields[7]
            };
        }

        private Transaction MapToEntity(TransactionDto dto)
        {
            return new Transaction
            {
                ReferenceNumber = dto.ReferenceNumber,
                Quantity = dto.Quantity,
                Amount = dto.Amount,
                Name = dto.Name,
                TransactionDate = dto.TransactionDate,
                Symbol = dto.Symbol,
                OrderSide = dto.OrderSide,
                OrderStatus = dto.OrderStatus
            };
        }


        public TransactionValidationResult ValidateTransaction(TransactionDto transaction)
        {
            var validationResult = new TransactionValidationResult { IsValid = true };

            // Validate Reference Number
            if (string.IsNullOrWhiteSpace(transaction.ReferenceNumber) ||
                transaction.ReferenceNumber.Length > 20 ||
                !Regex.IsMatch(transaction.ReferenceNumber, @"^[a-zA-Z0-9]+$"))
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Reference Number must be alphanumeric and maximum 20 characters long.");
            }

            // Validate Quantity
            if (transaction.Quantity <= 0)
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Quantity must be a positive long value.");
            }

            // Validate Amount
            if (transaction.Amount < 0)
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Amount must be a non-negative decimal value.");
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(transaction.Name))
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Name is required and must be valid text.");
            }

            // Validate Transaction Date
            if (transaction.TransactionDate == default(DateTime))
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Transaction Date must be a valid date.");
            }

            // Validate Symbol
            if (string.IsNullOrWhiteSpace(transaction.Symbol) ||
                transaction.Symbol.Length < 3 ||
                transaction.Symbol.Length > 5)
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Symbol must be alphanumeric with a length between 3 and 5 characters.");
            }

            // Validate Order Side
            if (transaction.OrderSide != "Buy" && transaction.OrderSide != "Sell")
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Order Side must be 'Buy' or 'Sell'.");
            }

            // Validate Order Status
            if (transaction.OrderStatus != "Open" &&
                transaction.OrderStatus != "Matched" &&
                transaction.OrderStatus != "Cancelled")
            {
                validationResult.IsValid = false;
                validationResult.ValidationMessages.Add("Order Status must be 'Open', 'Matched', or 'Cancelled'.");
            }

            validationResult.ValidTransactionCount = validationResult.IsValid ? 1 : 0; // Adjust this based on actual count logic

            return validationResult;
        }


        public IEnumerable<Transaction> GetTransactionsBySymbol(string symbol)
        {
            return _context.Transactions.Where(t => t.Symbol == symbol).ToList();
        }

        public IEnumerable<Transaction> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();
        }

        public IEnumerable<Transaction> GetTransactionsByOrderSide(string orderSide)
        {
            return _context.Transactions.Where(t => t.OrderSide == orderSide).ToList();
        }

        public IEnumerable<Transaction> GetTransactionsByOrderStatus(string orderStatus)
        {
            return _context.Transactions.Where(t => t.OrderStatus == orderStatus).ToList();
        }
        // In TransactionService.cs
        public IEnumerable<Transaction> GetAllTransactions()
        {
            return _context.Transactions.ToList(); // Assuming _dbContext is your database context and Transactions is the DbSet
        }

    }
}
