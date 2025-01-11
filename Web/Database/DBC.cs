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

    private IConfiguration _configuration;
    public DBC(IConfiguration config)
    {
        _configuration = config;
        var connectionString = config["ConnectionStrings:DefaultConnection"];
        NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString);
    }
      

    public DBC(DbContextOptions<DBC> options,IConfiguration config) : base(options) 
    {
        _configuration = config;
        var connectionString = config["ConnectionStrings:DefaultConnection"];
        NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString);
        sqlConnection.Open();
        Console.WriteLine(sqlConnection.ConnectionString);
        sqlConnection.Close();        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    {
        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        optionsBuilder.UseNpgsql(connectionString);
    }
}