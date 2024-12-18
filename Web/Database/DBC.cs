using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Database
{
    public class DBC : DbContext
    {
        public DbSet<User> users { get; set; } = null!;

        public DBC() 
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        { 
            
             optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=users;Username=postgres;Password=1;SSL Mode=Disable; Timeout=1024"); 
              
        }
        
    }
}
