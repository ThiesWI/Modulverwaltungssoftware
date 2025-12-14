using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace Modulverwaltungssoftware.Services
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=MySqliteConnection")
        {
        }
        public DbSet<Benutzer> Benutzer { get; set; }
        public DbSet<Kommentar> Kommentar { get; set; }
        public DbSet<Modul> Modul { get; set; }
        public DbSet<ModulVersion> ModulVersion { get; set; }
        public DbSet<Studiengang> Studiengang { get; set; }
    }
}
