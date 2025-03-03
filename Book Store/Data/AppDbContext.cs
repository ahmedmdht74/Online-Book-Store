using Book_Store.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Book_Store.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<OrderStatus> orderStatuses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetails> CartDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        




        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<Favourite>().HasKey(e => new { e.UserId, e.BookId });

            builder.Entity<Genre>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Book>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Author>().HasQueryFilter(x => !x.IsDeleted); 
            builder.Entity<Cart>().HasQueryFilter(x => !x.IsDeleted); 
            builder.Entity<CartDetails>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<OrderDetails>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Contact>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<State>().HasQueryFilter(x => !x.IsDeleted ?? false);
            builder.Entity<City>().HasQueryFilter(x => !x.IsDeleted ?? false);
            base.OnModelCreating(builder);
        }
    }
}
