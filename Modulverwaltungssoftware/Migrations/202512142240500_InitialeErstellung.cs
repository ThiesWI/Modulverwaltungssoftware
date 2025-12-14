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
                        Name = c.String(maxLength: 2147483647),
                        Email = c.String(maxLength: 2147483647),
                        Passwort = c.String(maxLength: 2147483647),
                        RollenName = c.String(maxLength: 2147483647),
                        AktuellerBenutzer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BenutzerID);
            
            CreateTable(
                "dbo.Kommentars",
                c => new
                    {
                        KommentarID = c.Int(nullable: false, identity: true),
                        Text = c.String(maxLength: 2147483647),
                        ErstellungsDatum = c.DateTime(nullable: false),
                        GehoertZuModulVersionID = c.Int(nullable: false),
                        GehoertZuModulID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.KommentarID);
            
            CreateTable(
                "dbo.Moduls",
                c => new
                    {
                        ModulID = c.Int(nullable: false, identity: true),
                        ModulnameDE = c.String(maxLength: 2147483647),
                        ModulnameEN = c.String(maxLength: 2147483647),
                        EmpfohlenesSemester = c.Int(nullable: false),
                        DauerInSemestern = c.Int(nullable: false),
                        VoraussetzungenDb = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.ModulID);
            
            CreateTable(
                "dbo.ModulVersions",
                c => new
                    {
                        ModulVersionID = c.Int(nullable: false, identity: true),
                        ModulId = c.Int(nullable: false),
                        GueltigAbSemester = c.String(maxLength: 2147483647),
                        WorkloadPraesenz = c.Int(nullable: false),
                        WorkloadSelbststudium = c.Int(nullable: false),
                        EctsPunkte = c.Int(nullable: false),
                        Pruefungsform = c.String(maxLength: 2147483647),
                        LernergebnisseDb = c.String(maxLength: 2147483647),
                        InhaltsgliederungDb = c.String(maxLength: 2147483647),
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
                        Kuerzel = c.String(maxLength: 2147483647),
                        NameDE = c.String(maxLength: 2147483647),
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
