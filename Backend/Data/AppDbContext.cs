using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<FavoriteAccount> FavoriteAccounts { get; set; }
        public DbSet<BankLookup> BankLookups { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankLookup>().HasKey(b => b.Code);
        }
    }
}
