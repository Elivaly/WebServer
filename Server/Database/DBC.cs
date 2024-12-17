using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Database
{
    internal class DBC : DbContext
    {

        public DbSet<Сustomers> customers { get; set; } = null!;

        public DBC()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=users;Username=postgres;Password=1");
        }
    }
}
