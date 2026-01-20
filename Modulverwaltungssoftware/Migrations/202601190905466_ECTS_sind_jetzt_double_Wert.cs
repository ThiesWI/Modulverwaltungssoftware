namespace Modulverwaltungssoftware.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ECTS_sind_jetzt_double_Wert : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Benachrichtigungs",
                c => new
                {
                    BenachrichtigungsID = c.Int(nullable: false, identity: true),
                    Empfaenger = c.String(nullable: false, maxLength: 2147483647),
                    Sender = c.String(nullable: false, maxLength: 2147483647),
                    Nachricht = c.String(nullable: false, maxLength: 2147483647),
                    GesendetAm = c.DateTime(nullable: false),
                    Gelesen = c.Boolean(nullable: false),
                    BetroffeneModulVersionID = c.Int(),
                })
                .PrimaryKey(t => t.BenachrichtigungsID);

            CreateTable(
                "dbo.Benutzers",
                c => new
                {
                    BenutzerID = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    Email = c.String(nullable: false, maxLength: 255),
                    Passwort = c.String(nullable: false, maxLength: 55),
                    RollenName = c.String(nullable: false, maxLength: 50),
                })
                .PrimaryKey(t => t.BenutzerID);

            CreateTable(
                "dbo.Kommentars",
                c => new
                {
                    KommentarID = c.Int(nullable: false, identity: true),
                    FeldName = c.String(maxLength: 100),
                    Text = c.String(nullable: false, maxLength: 2147483647),
                    ErstellungsDatum = c.DateTime(),
                    Ersteller = c.String(maxLength: 100),
                    GehoertZuModulVersionID = c.Int(nullable: false),
                    GehoertZuModulID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.KommentarID);

            CreateTable(
                "dbo.Moduls",
                c => new
                {
                    ModulID = c.Int(nullable: false, identity: true),
                    Studiengang = c.String(nullable: false, maxLength: 200),
                    ModulnameDE = c.String(nullable: false, maxLength: 200),
                    ModulnameEN = c.String(maxLength: 200),
                    Modultyp = c.Int(nullable: false),
                    Turnus = c.Int(nullable: false),
                    PruefungsForm = c.Int(nullable: false),
                    EmpfohlenesSemester = c.Int(nullable: false),
                    GueltigAb = c.DateTime(nullable: false),
                    DauerInSemestern = c.Int(nullable: false),
                    VoraussetzungenDb = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.ModulID);

            CreateTable(
                "dbo.ModulVersions",
                c => new
                {
                    ModulVersionID = c.Int(nullable: false, identity: true),
                    ModulId = c.Int(nullable: false),
                    Versionsnummer = c.Int(nullable: false),
                    hatKommentar = c.Boolean(nullable: false),
                    GueltigAbSemester = c.String(nullable: false, maxLength: 25),
                    ModulStatus = c.Int(nullable: false),
                    LetzteAenderung = c.DateTime(nullable: false),
                    WorkloadPraesenz = c.Int(nullable: false),
                    WorkloadSelbststudium = c.Int(nullable: false),
                    EctsPunkte = c.Double(nullable: false),
                    Pruefungsform = c.String(nullable: false, maxLength: 100),
                    Ersteller = c.String(maxLength: 2147483647),
                    LernergebnisseDb = c.String(nullable: false, maxLength: 4000),
                    InhaltsgliederungDb = c.String(nullable: false, maxLength: 4000),
                    LiteraturDb = c.String(maxLength: 4000),
                    Kommentar_KommentarID = c.Int(),
                })
                .PrimaryKey(t => t.ModulVersionID)
                .ForeignKey("dbo.Kommentars", t => t.Kommentar_KommentarID)
                .ForeignKey("dbo.Moduls", t => t.ModulId, cascadeDelete: true)
                .Index(t => t.ModulId)
                .Index(t => t.Kommentar_KommentarID);

            CreateTable(
                "dbo.Studiengangs",
                c => new
                {
                    StudiengangID = c.Int(nullable: false, identity: true),
                    Kuerzel = c.String(nullable: false, maxLength: 20),
                    NameDE = c.String(nullable: false, maxLength: 200),
                    NameEN = c.String(maxLength: 200),
                    GesamtECTS = c.Int(nullable: false),
                    GueltigAb = c.DateTime(nullable: false),
                    Verantwortlicher = c.String(maxLength: 2147483647),
                })
                .PrimaryKey(t => t.StudiengangID);

        }

        public override void Down()
        {
            DropForeignKey("dbo.ModulVersions", "ModulId", "dbo.Moduls");
            DropForeignKey("dbo.ModulVersions", "Kommentar_KommentarID", "dbo.Kommentars");
            DropIndex("dbo.ModulVersions", new[] { "Kommentar_KommentarID" });
            DropIndex("dbo.ModulVersions", new[] { "ModulId" });
            DropTable("dbo.Studiengangs");
            DropTable("dbo.ModulVersions");
            DropTable("dbo.Moduls");
            DropTable("dbo.Kommentars");
            DropTable("dbo.Benutzers");
            DropTable("dbo.Benachrichtigungs");
        }
    }
}
