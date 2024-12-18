using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Database;

public class DBC : DbContext
{
    public DbSet<User> users { get; set; } = null!;
    public DBC(DbContextOptions<DBC> options) : base(options) 
    { 
        Console.WriteLine("Initializing database context...");
        try 
        { 
            InitializeDatabase();
        } 
        catch (Exception ex) 
        { 
            Console.WriteLine($"Initialization failed: {ex.Message}");
        }
        Console.WriteLine("Database context initialized.");
    }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    { 
        if (!optionsBuilder.IsConfigured) 
        { 
            Console.WriteLine("Configuring database options..."); 
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=users;Username=postgres;Password=1;SSL Mode=Disable; Timeout=1024"); 
            Console.WriteLine("Database options configured."); 
        } 
    }
        private void InitializeDatabase() 
    { 
        try 
        { 
            if (Database.GetPendingMigrations().Any()) 
            { 
                Console.WriteLine("Applying pending migrations...");
                Database.Migrate(); 
                Console.WriteLine("Migrations applied.");
            } 
            else 
            {
                Console.WriteLine("No pending migrations. Database is up to date."); 
            } 
        } 
        catch (Exception ex)
        { 
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
            throw;
        } 
    }
}