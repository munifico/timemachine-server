using Microsoft.EntityFrameworkCore;

namespace TimeMachine.Server.DB
{
    public class QTContext : DbContext
    {
        public DbSet<Subject> Universe { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(
                @"server=10.127.38.17;port=20306;database=qt;uid=admin;password=Lineabiz123!;Max Pool Size=10");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>()
                .HasKey(e => e.AssetCode);

            modelBuilder.Entity<Stock>()
           .HasKey(e => e.AssetCode);
        }
    }
}
