using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Models.Auth;
using MyBookShop.Models.Identity;
using MyBookShop.Models.Library.Authors;
using MyBookShop.Models.Library.Books;
using MyBookShop.Models.Payment;
using MyBookShop.Models.Shop.Cart;
using MyBookShop.Models.Shop.Order;

namespace MyBookShop.Data.Context
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<MyApplicationUser>(options)
    {
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookImage> BookImages => Set<BookImage>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<AuthorToBook> AuthorsToBooks => Set<AuthorToBook>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Transaction> Transactions => Set<Transaction>();


        override protected void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MyApplicationUser>().HasIndex(u => u.NationalCode).IsUnique();

            builder.Entity<AuthorToBook>().HasKey(k => new { k.AuthorId, k.BookId });

            builder.Entity<Transaction>()
            .HasOne(p => p.Cart)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CartId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
