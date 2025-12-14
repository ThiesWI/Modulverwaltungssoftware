using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Sql;
using System.Data.SQLite.EF6;
using System.Data.SQLite.EF6.Migrations; // Hinzugefügt

namespace Modulverwaltungssoftware.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Modulverwaltungssoftware.Services.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            // Registrierung des SQLite MigrationSqlGenerators
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }
    }
}