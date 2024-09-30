using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TransactionUploadService.Services
{
    public class CustomDateFormatAttribute : ValidationAttribute
    {
        private readonly string _dateFormat;

        public CustomDateFormatAttribute(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var dateString = value.ToString();
                if (DateTime.TryParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"Invalid date format. Please use the format {_dateFormat}.");
                }
            }

            return new ValidationResult("Transaction date is required.");
        }
    }
}
