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
    }


}