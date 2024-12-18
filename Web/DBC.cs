using System.Collections.Generic;
using AuthService;
using Microsoft.EntityFrameworkCore;

namespace Web
{
    public class DBC : DbContext
    {
        public DbSet<Customers> customers { get; set; } = null!;

        public DBC()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5104;Database=users;Username=postgres;Password=1;SSL Mode=Disable; Timeout = 1024");
        }
    }
}
