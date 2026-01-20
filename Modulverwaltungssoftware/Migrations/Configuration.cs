using System.Data.Entity.Migrations;
using System.Data.SQLite.EF6.Migrations; // Hinzugefügt

namespace Modulverwaltungssoftware.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Modulverwaltungssoftware.Services.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            // Registrierung des SQLite MigrationSqlGenerators
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }
    }
}