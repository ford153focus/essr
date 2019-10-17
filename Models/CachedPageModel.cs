using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace essr.Models
{
    public class CachedPageContext : DbContext
    {
        public DbSet<CachedPage> CachedPages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CachedPage>()
                .HasIndex(page => page.Url)
                .IsUnique();
        }
    }

    public class CachedPage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public string Url { get; set; }
        public int TimeStamp { get; set; }
        public int AnswerHttpCode { get; set; }
        public string SourceCode { get; set; }
    }
}