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
        try
        {
            sqlConnection.Open();
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }

    public DBC(DbContextOptions<DBC> options) : base(options) 
    {
        var connectionString = "host=localhost port=5432 database=users username=postgres password=1 connect_timeout=10 sslmode=disable";
        NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString);
        try
        {
            sqlConnection.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    { 
        var connectionString = "Host=localhost;Port=5432; Database=users;Username=postgres;Password=1;Timeout=10;SslMode=Disable";
        optionsBuilder.UseNpgsql(connectionString);
    }


}