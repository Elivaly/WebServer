using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebSocketServer.Schems;

namespace WebSocketServer.Database;

public class DBC : DbContext
{
    public DbContext context;
    public DbSet<Message> Messages {  get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    private IConfiguration _configuration;
    public DBC(IConfiguration configuration) 
    {
        _configuration = configuration;
        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    }

    public DBC(DbContextOptions<DBC> options, IConfiguration configuration) : base(options) 
    {
        _configuration = configuration;
        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        connection.Open();
        Console.WriteLine("Строка подключения : {0}",connection.ConnectionString);
        connection.Close();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        optionsBuilder.UseNpgsql(connectionString);
    }
}
