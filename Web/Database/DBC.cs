using System.Collections.Generic;
using System.Data;
using AuthService.Schems;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuthService.Handler;

public class DBC : DbContext
{
    public DbContext context;
    public DbSet<User> users { get; set; } = null!;

    public DBC()
    {
        var connectionString = "host=localhost port=5432 database=users username=postgres password=1 connect_timeout=10 sslmode=disable";
        NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString);

        //sqlConnection.Open();
    }
      

    public DBC(DbContextOptions<DBC> options) : base(options) 
    {
        var connectionString = "host=localhost port=5432 database=users username=postgres password=1 connect_timeout=10 sslmode=disable";
        NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString);
       
        sqlConnection.Open();

        using (var command = new NpgsqlCommand("SELECT * FROM users", sqlConnection))
        {
            using (var read = command.ExecuteReader())
            {
                while (read.Read())
                {
                    var userId = read.GetInt32(0);
                    var userName = read.GetString(1);
                    Console.WriteLine($"User ID: {userId}, User Name: {userName}");
                }
            }
        }
        sqlConnection.Close();
      
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    { 
        var connectionString = "Host=localhost;Port=5432; Database=users;Username=postgres;Password=1;Timeout=10;SslMode=Disable";
        optionsBuilder.UseNpgsql(connectionString);
    }
}