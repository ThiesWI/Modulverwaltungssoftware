namespace Modulverwaltungssoftware.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialeErstellung : DbMigration
    {
        public override void Up()
        {
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
                        Text = c.String(nullable: false, maxLength: 2147483647),
                        ErstellungsDatum = c.DateTime(),
                        GehoertZuModulVersionID = c.Int(nullable: false),
                        GehoertZuModulID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.KommentarID);
            
            CreateTable(
                "dbo.Moduls",
                c => new
                    {
                        ModulID = c.Int(nullable: false, identity: true),
                        ModulnameDE = c.String(nullable: false, maxLength: 200),
                        ModulnameEN = c.String(maxLength: 200),
                        EmpfohlenesSemester = c.Int(nullable: false),
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
                        GueltigAbSemester = c.String(nullable: false, maxLength: 25),
                        WorkloadPraesenz = c.Int(nullable: false),
                        WorkloadSelbststudium = c.Int(nullable: false),
                        EctsPunkte = c.Int(nullable: false),
                        Pruefungsform = c.String(nullable: false, maxLength: 100),
                        LernergebnisseDb = c.String(nullable: false, maxLength: 4000),
                        InhaltsgliederungDb = c.String(nullable: false, maxLength: 4000),
                        Ersteller_BenutzerID = c.Int(),
                    })
                .PrimaryKey(t => t.ModulVersionID)
                .ForeignKey("dbo.Benutzers", t => t.Ersteller_BenutzerID)
                .ForeignKey("dbo.Moduls", t => t.ModulId, cascadeDelete: true)
                .Index(t => t.ModulId)
                .Index(t => t.Ersteller_BenutzerID);
            
            CreateTable(
                "dbo.Studiengangs",
                c => new
                    {
                        StudiengangID = c.Int(nullable: false, identity: true),
                        Kuerzel = c.String(nullable: false, maxLength: 20),
                        NameDE = c.String(nullable: false, maxLength: 200),
                        NameEN = c.String(maxLength: 2147483647),
                        GesamtECTS = c.Int(nullable: false),
                        GueltigAb = c.DateTime(nullable: false),
                        Verantwortlicher_BenutzerID = c.Int(),
                    })
                .PrimaryKey(t => t.StudiengangID)
                .ForeignKey("dbo.Benutzers", t => t.Verantwortlicher_BenutzerID)
                .Index(t => t.Verantwortlicher_BenutzerID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Studiengangs", "Verantwortlicher_BenutzerID", "dbo.Benutzers");
            DropForeignKey("dbo.ModulVersions", "ModulId", "dbo.Moduls");
            DropForeignKey("dbo.ModulVersions", "Ersteller_BenutzerID", "dbo.Benutzers");
            DropIndex("dbo.Studiengangs", new[] { "Verantwortlicher_BenutzerID" });
            DropIndex("dbo.ModulVersions", new[] { "Ersteller_BenutzerID" });
            DropIndex("dbo.ModulVersions", new[] { "ModulId" });
            DropTable("dbo.Studiengangs");
            DropTable("dbo.ModulVersions");
            DropTable("dbo.Moduls");
            DropTable("dbo.Kommentars");
            DropTable("dbo.Benutzers");
        }
    }
}
