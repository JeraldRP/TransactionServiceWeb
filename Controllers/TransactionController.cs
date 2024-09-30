using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionUploadService.Models.Entities;
using TransactionUploadService.Data; // Adjust this namespace based on your project structure

namespace TransactionUploadService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Retrieve all transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        // 2. Retrieve transactions by symbol
        [HttpGet("bySymbol")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsBySymbol(string symbol)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Symbol == symbol)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound("No transactions found for the specified symbol.");
            }

            return Ok(transactions);
        }

        // 3. Retrieve transactions within a specified date range
        [HttpGet("byDateRange")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound("No transactions found in the specified date range.");
            }

            return Ok(transactions);
        }

        // 4. Retrieve transactions by order side
        [HttpGet("byOrderSide")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByOrderSide(string orderSide)
        {
            // Validate orderSide
            if (orderSide != "Buy" && orderSide != "Sell")
            {
                return BadRequest("Invalid order side. Valid values are 'Buy' or 'Sell'.");
            }

            var transactions = await _context.Transactions
                .Where(t => t.OrderSide == orderSide)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound("No transactions found for the specified order side.");
            }

            return Ok(transactions);
        }

        // 5. Retrieve transactions by order status
        [HttpGet("byOrderStatus")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByOrderStatus(string orderStatus)
        {
            // Validate orderStatus
            var validOrderStatuses = new List<string> { "Open", "Matched", "Cancelled" };
            if (!validOrderStatuses.Contains(orderStatus))
            {
                return BadRequest("Invalid order status. Valid values are 'Open', 'Matched', or 'Cancelled'.");
            }

            var transactions = await _context.Transactions
                .Where(t => t.OrderStatus == orderStatus)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound("No transactions found for the specified order status.");
            }

            return Ok(transactions);
        }

        // 6. Retrieve filtered transactions based on multiple criteria (optional, if needed)
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions(string? symbol, DateTime? startDate, DateTime? endDate, string? orderSide, string? orderStatus)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(symbol))
            {
                query = query.Where(t => t.Symbol == symbol);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value && t.TransactionDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(orderSide))
            {
                // Validate orderSide
                if (orderSide != "Buy" && orderSide != "Sell")
                {
                    return BadRequest("Invalid order side. Valid values are 'Buy' or 'Sell'.");
                }
                query = query.Where(t => t.OrderSide == orderSide);
            }

            if (!string.IsNullOrEmpty(orderStatus))
            {
                // Validate orderStatus
                var validOrderStatuses = new List<string> { "Open", "Matched", "Cancelled" };
                if (!validOrderStatuses.Contains(orderStatus))
                {
                    return BadRequest("Invalid order status. Valid values are 'Open', 'Matched', or 'Cancelled'.");
                }
                query = query.Where(t => t.OrderStatus == orderStatus);
            }

            var transactions = await query.ToListAsync();

            if (!transactions.Any())
            {
                return NotFound("No transactions found with the provided filters.");
            }

            return Ok(transactions);
        }
    }
}
