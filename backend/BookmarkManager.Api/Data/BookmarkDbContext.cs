using BookmarkManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarkManager.Api.Data;

public class BookmarkDbContext(DbContextOptions<BookmarkDbContext> options) : DbContext(options)
{
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bookmark>()
            .Property(b => b.Tags)
            .HasConversion(
                v => string.Join('\x1F', v),
                v => v == string.Empty ? new List<string>() : v.Split('\x1F', StringSplitOptions.None).ToList());
    }
}
