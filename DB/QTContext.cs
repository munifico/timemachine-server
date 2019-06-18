using Microsoft.EntityFrameworkCore;

namespace TimeMachineServer.DB
{
    public class QTContext : DbContext
    {
        public DbSet<Subject> Universe { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<KoreaStock> KoreaStocks { get; set; }

        public DbSet<FX1D> FX1D { get; set; }
        public DbSet<FX1W> FX1W { get; set; }
        public DbSet<FX60M> FX60M { get; set; }

        public DbSet<Index> Indices { get; set; }
        public DbSet<KoreaIndex> KoreaIndices { get; set; }
        public DbSet<TradingCalendar> TradingCalendars { get; set; }
        public DbSet<Split> Splits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(
                @"server=10.127.38.17;port=20306;database=qt;uid=admin;password=Lineabiz123!;Max Pool Size=10");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>()
                .HasKey(e => new { e.AssetCode, e.Exchange });

            modelBuilder.Entity<Stock>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<KoreaStock>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<FX1D>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<FX1W>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<FX60M>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<Index>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<KoreaIndex>()
                .HasKey(e => new { e.CreatedAt, e.AssetCode });

            modelBuilder.Entity<TradingCalendar>()
                .HasKey(e => new { e.TradingDate, e.IsoCode });

            modelBuilder.Entity<Split>()
                .HasKey(e => new { e.AssetCode, e.SplitDate });
        }
    }
}
