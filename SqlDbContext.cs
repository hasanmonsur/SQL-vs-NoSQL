using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace InlineArryBenchmark
{
    public class SqlDbContext : DbContext
    {
        // Required constructor for DI
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

        // Add this additional constructor for manual creation
        public SqlDbContext() : base() { }

        // Your DbSets
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Server=localhost;Database=postgres;User Id=postgres;Password=postgres;");
            }
        }
    }
}
