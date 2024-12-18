using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuthService.Database;

public class DBC : DbContext
{
    public DbSet<User> users { get; set; } = null!;

    public DBC()
    {
        NpgsqlConnection sqlConnection = new ();
        sqlConnection.Open ();
        NpgsqlCommand sqlCommand = new();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandType = CommandType.Text;
        sqlCommand.CommandText = "SELECT * FROM users";
        NpgsqlDataReader dataReader = sqlCommand.ExecuteReader ();
        if (dataReader.HasRows) 
        {
            Database.EnsureCreatedAsync();
        }
        sqlCommand.Dispose();
        sqlConnection.Close ();

    }
}