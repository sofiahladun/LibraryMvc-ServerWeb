using LibraryMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Reader> Readers => Set<Reader>();
        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Зв'язок Book (1) -> (N) Loan
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Зв'язок Reader (1) -> (N) Loan
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Reader)
                .WithMany(r => r.Loans)
                .HasForeignKey(l => l.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Унікальні індекси для ISBN та номера читацького квитка
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<Reader>()
                .HasIndex(r => r.LibraryCardNumber)
                .IsUnique();
        }
    }
}
