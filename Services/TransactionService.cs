using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using TransactionUploadService.Models.Entities;
using System.ComponentModel.DataAnnotations;
using TransactionUploadService.Models;

namespace TransactionUploadService.Services
{
    public class TransactionService
    {
        public TransactionValidationResult ProcessTransactionsFromCsv(string filePath)
        {
            var transactions = new List<Transaction>();
            var invalidRecords = new List<string>();
            var validationResult = new TransactionValidationResult();

            try
            {
                // Read the CSV file line by line
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var fields = line.Split(',');

                    // Create a new transaction object
                    var transaction = new Transaction
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

                    // Validate the transaction
                    var validationContext = new ValidationContext(transaction);
                    var validationResults = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(transaction, validationContext, validationResults, true);

                    if (isValid)
                    {
                        transactions.Add(transaction);
                    }
                    else
                    {
                        // Log invalid records
                        invalidRecords.Add(line);
                        // Add validation messages to the result
                        foreach (var validationResultItem in validationResults)
                        {
                            // Check if ErrorMessage is not null before adding it
                            if (validationResultItem.ErrorMessage != null)
                            {
                                validationResult.ValidationMessages.Add(validationResultItem.ErrorMessage);
                            }
                        }
                    }
                }

                // Set the validation result properties
                validationResult.IsValid = invalidRecords.Count == 0; // Set IsValid to true if no invalid records
                validationResult.ValidTransactionCount = transactions.Count; // Set valid transaction count
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file not found, format issues)
                Console.WriteLine($"Error processing file: {ex.Message}");
                validationResult.ValidationMessages.Add($"Error processing file: {ex.Message}");
            }

            // Return the validation result
            return validationResult;
        }
    }

}

