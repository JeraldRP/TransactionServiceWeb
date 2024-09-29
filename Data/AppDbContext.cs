using Microsoft.EntityFrameworkCore;
using TransactionUploadService.Models.Entities;

namespace TransactionUploadService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureDecimalPrecision(modelBuilder);
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
               .Property(t => t.Amount)
               .HasColumnType("decimal(18,2)");
        }
    }
}
