using Modulverwaltungssoftware.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware.Services
{
    public class DatabaseInitializationService
    {
        public static void InitializeDatabase()
        {
            using (var db = new DatabaseContext())
            {
                // Ensure database is created
                db.Database.CreateIfNotExists();
                // Apply any pending migrations
                var migrator = new System.Data.Entity.Migrations.DbMigrator(new Migrations.Configuration());
                migrator.Update();
                if (db.Benutzer.Any())
                {
                    return; // DB has been seeded
                }
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var gast = new Benutzer
                        {
                            Name = "Gast",
                            Email = "gast@hochschule.de",
                            Passwort = "gast123",
                            RollenName = "Gast",
                        };
                        var dozent = new Benutzer
                        {
                            Name = "Dr. Max Mustermann",  // ✅ Geändert, um mit Auto-Login übereinzustimmen
                            Email = "max.mustermann@hs-example.de",
                            Passwort = "dozent123",
                            RollenName = "Dozent",
                        };
                        var koordination = new Benutzer
                        {
                            Name = "Sabine Beispiel",  // ✅ Geändert, um mit Auto-Login übereinzustimmen
                            Email = "sabine.beispiel@hs-example.de",
                            Passwort = "koordination123",
                            RollenName = "Koordination",
                        };
                        var gremium = new Benutzer
                        {
                            Name = "Prof. Erika Musterfrau",  // ✅ Geändert, um mit Auto-Login übereinzustimmen
                            Email = "erika.musterfrau@hs-example.de",
                            Passwort = "gremium123",
                            RollenName = "Gremium",
                        };
                        var admin = new Benutzer
                        {
                            Name = "Philipp Admin",  // ✅ Geändert, um mit Auto-Login übereinzustimmen
                            Email = "admin@hs-example.de",
                            Passwort = "admin123",
                            RollenName = "Admin",
                        };

                        db.Benutzer.Add(gast);
                        db.Benutzer.Add(dozent);
                        db.Benutzer.Add(koordination);
                        db.Benutzer.Add(gremium);
                        db.Benutzer.Add(admin);

                        db.SaveChanges();


                        // --- SCHRITT 2: STUDIENGANG ERSTELLEN ---
                        var studiengangsDaten = new[]
                        {
                            ("B.Sc. WI",  "Wirtschaftsinformatik", "Business Informatics", 180, dozent),
                            ("B.A. BWL",  "Betriebswirtschaftslehre", "Business Administration", 180, dozent),
                            ("B.Eng. MB", "Maschinenbau", "Mechanical Engineering", 210, admin),
                            ("M.Sc. CS",  "Informatik Master", "Computer Science", 120, dozent),
                            ("B.Sc. PSY", "Medieninformatik", "Media Informatics", 180, dozent)
                        };

                        foreach (var daten in studiengangsDaten)
                        {
                            var neuerStudiengang = new Studiengang
                            {
                                Kuerzel = daten.Item1,
                                NameDE = daten.Item2,
                                NameEN = daten.Item3,
                                GesamtECTS = daten.Item4,
                                GueltigAb = DateTime.Now,
                                Verantwortlicher = daten.Item5.Name
                            };

                            db.Studiengang.Add(neuerStudiengang);
                        }

                        // --- SCHRITT 3: MODUL ERSTELLEN ---

                        var neueModule = new List<Modul>
                        {
                            // --- MODUL: Software Engineering ---
                            new Modul
                            {
                                GueltigAb = DateTime.Now,
                                Studiengang = "B.Sc. WI",
                                ModulnameDE = "Software Engineering I",
                                ModulnameEN = "Software Engineering I",
                                EmpfohlenesSemester = 3,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "Programmierung I",
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    // Veraltete Version
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10 (nicht 1!)
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "WiSe 2021",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Klausur (90 min)",
                                        LernergebnisseDb = "Wasserfallmodell, V-Modell...",
                                        InhaltsgliederungDb = "1. Einführung\n2. Prozessmodelle",
                
                                        // Hier deine Anforderung: Zuweisung über den Namen
                                        Ersteller = dozent.Name
                                    },
                                    // Aktuelle Version (Entwurf)
                                    new ModulVersion
                                    {
                                        Versionsnummer = 20,  // ? FIX: 2.0 = 20 (nicht 2!)
                                        ModulStatus = ModulVersion.Status.Aenderungsbedarf,
                                        GueltigAbSemester = "WiSe 2324",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 45, WorkloadSelbststudium = 105,
                                        Pruefungsform = "Portfolio-Prüfung",
                                        LernergebnisseDb = "Scrum, Kanban, DevOps...",
                                        InhaltsgliederungDb = "1. Agile Werte\n2. Scrum Guide",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },

                            new Modul
                            {
                                GueltigAb = DateTime.Now.AddYears(-2),
                                Studiengang = "B.Sc. WI",
                                ModulnameDE = "Mathematik I",
                                ModulnameEN = "Mathematics I",
                                EmpfohlenesSemester = 1,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = null,
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.NurWintersemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "WiSe 2223",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Klausur (120 min)",
                                        LernergebnisseDb = "Analysis, Lineare Algebra...",
                                        InhaltsgliederungDb = "1. Mengenlehre\n2. Funktionen",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },

                            new Modul
                            {
                                GueltigAb = DateTime.Now.AddYears(-1),
                                Studiengang = "B.A. BWL",
                                ModulnameDE = "Projektmanagement",
                                ModulnameEN = "Project Management",
                                EmpfohlenesSemester = 4,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "BWL Grundlagen",
                                Modultyp = Modul.ModultypEnum.Wahlpflicht,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.SL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.InPruefungGremium,
                                        GueltigAbSemester = "SoSe 24",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 30, WorkloadSelbststudium = 120,
                                        Pruefungsform = "Hausarbeit",
                                        LernergebnisseDb = "Projektplanung, Risikomanagement...",
                                        InhaltsgliederungDb = "1. Projektphasen\n2. Stakeholder",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                GueltigAb = DateTime.Now.AddYears(-1),
                                Studiengang = "B.Sc. WI",
                                ModulnameDE = "Programming Basics",
                                ModulnameEN = "Programming Basics",
                                EmpfohlenesSemester = 1,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = null,
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.InPruefungKoordination,
                                        GueltigAbSemester = "SoSe 26",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Digitale Praesenzpruefung",
                                        LernergebnisseDb = "Grundlagen des Programmierens mit C#",
                                        InhaltsgliederungDb = "Schreiben von einfachen Konsolen-Apps",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                GueltigAb = DateTime.Now.AddMonths(-6),
                                Studiengang = "B.A. BWL",
                                ModulnameDE = "Rechnungswesen 2",
                                EmpfohlenesSemester = 2,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "Rechnungswesen 1",
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.NurSommersemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "SoSe 21",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 90, WorkloadSelbststudium = 60,
                                        Pruefungsform = "Klausur 120 Minuten",
                                        LernergebnisseDb = "Kostenrechnung, Controlling",
                                        InhaltsgliederungDb = "1. KoRe\n2. Controlling",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                ModulnameDE = "Akademisches Schreiben",
                                Studiengang = "B.Sc Angewandte Informatik / Wirtschaftsinformatik",
                                EmpfohlenesSemester = 4,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "Keine",
                                Modultyp = Modul.ModultypEnum.Wahlpflicht,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.SL,
                                GueltigAb = DateTime.Now,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "SoSe 15",
                                        EctsPunkte = 10,
                                        WorkloadPraesenz = 200, WorkloadSelbststudium = 100,
                                        Pruefungsform = "Hausarbeit",
                                        LernergebnisseDb = "Wissenschaftliche Fragestellungen zu formulieren und einzugrenzen.\nZitationsstile (IEEE, APA) korrekt anzuwenden und Plagiate zu vermeiden.\nWissenschaftliche Literatur systematisch zu recherchieren und zu bewerten.\nStrukturierte Gliederungen für Fachberichte und Thesis-Arbeiten zu erstellen.",
                                        InhaltsgliederungDb = "Grundlagen des wissenschaftlichen Arbeitens: Ethik und Standards.\nLiteraturrecherche: Umgang mit Bibliotheksdatenbanken, Google Scholar und ACM Digital Library.\nZitiertechnik: Direkte und indirekte Zitate, Quellenverwaltung mit BibTeX/Zotero.\nSchreibprozess: Vom Abstract bis zum Fazit – Stilistik und roter Faden.\nPräsentation von Ergebnissen: Poster-Sessions und wissenschaftliche Vorträge.",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                ModulnameDE = "Einführung in die Pyrotechnik",
                                ModulnameEN = "Pyrotechnics I",
                                Studiengang = "B.Sc. Sicherheitstechnik, B.Sc Veranstaltungstechnik",
                                EmpfohlenesSemester = 5,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "Grundlagen der Anorganischen Chemie, Mathematik I",
                                Modultyp = Modul.ModultypEnum.Wahlpflicht,
                                Turnus = Modul.TurnusEnum.NurSommersemester,
                                PruefungsForm = Modul.PruefungsFormEnum.SP,
                                GueltigAb = DateTime.Now,

                                ModulVersionen = new List<ModulVersion>
                                {
                                                                        new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Archiviert,
                                        GueltigAbSemester = "SoSe 9999",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Laborabnahmen (50%), Online-Klausur (50%)",
                                        LernergebnisseDb = "Die Studierenden kennen Grundlagen von Explosivstoffen. Sie können Gefährdungen beurteilen. Rechtliche Rahmenbedingungen werden besprochen.",
                                        InhaltsgliederungDb = "1. Stoffkunde, 2. Zündtechnik, 3. Sicherheitsmanagement, 4. Recht.",
                                        Ersteller = dozent.Name
                                    },
                                    new ModulVersion
                                    {
                                        Versionsnummer = 20,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Entwurf,
                                        GueltigAbSemester = "SoSe 26",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Laborabnahmen (50%), Online-Klausur (50%)",
                                        LernergebnisseDb = "Die Studierenden kennen die rechtlichen Rahmenbedingungen (SprengG) für den Umgang mit pyrotechnischen Gegenständen.\n" +
                                   "Sie verstehen die chemisch-physikalischen Vorgänge bei der Verbrennung von Sätzen.\n" +
                                   "Sie können Sicherheitsabstände berechnen und Gefährdungsanalysen erstellen.\n" +
                                   "Sie sind in der Lage, einfache pyrotechnische Effekte theoretisch zu planen.",
                                        InhaltsgliederungDb = "1. Historie und Entwicklung der Pyrotechnik\n" +
                                      "2. Rechtliche Grundlagen (Sprengstoffgesetz, BAM-Klassen)\n" +
                                      "3. Chemische Grundlagen (Oxidationsmittel, Reduktionsmittel, Binder)\n" +
                                      "4. Farberzeugung und akustische Effekte\n" +
                                      "5. Zündsysteme und elektrische Zündung\n" +
                                      "6. Sicherheitstechnik und Lagerung",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                GueltigAb = DateTime.Now.AddYears(-1),
                                ModulnameDE = "IT-Recht",
                                ModulnameEN = "IT-Law",
                                Studiengang = "B.Sc. Wirtschaftsinformatik",
                                EmpfohlenesSemester = 4,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = null,
                                Modultyp = Modul.ModultypEnum.Wahlpflicht,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "WiSe 2425",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 30, WorkloadSelbststudium = 120,
                                        Pruefungsform = "Klausur 120 Minuten",
                                        LernergebnisseDb = "Die Studierenden besitzen grundlegende Kenntnisse im Zivil- und Urheberrecht.\n" +
                                   "Sie können IT-Verträge (Werkvertrag, Dienstvertrag) rechtlich einordnen.\n" +
                                   "Sie verstehen die Prinzipien der DSGVO und können diese auf Softwareprojekte anwenden.\n" +
                                   "Sie kennen die Haftungsrisiken bei der Softwareentwicklung.",
                                        InhaltsgliederungDb = "1. Grundlagen des BGB und Vertragsrecht\n" +
                                      "2. Urheberrecht und Lizenzmodelle (Open Source vs. Proprietär)\n" +
                                      "3. Datenschutzrecht (DSGVO, BDSG)\n" +
                                      "4. E-Commerce Recht und Impressumspflicht\n" +
                                      "5. IT-Sicherheit und Strafrecht (Cybercrime)\n" +
                                      "6. Arbeitnehmerdatenschutz",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                ModulnameDE = "Thermodynamik I",
                                ModulnameEN = null,
                                Studiengang = "B.Eng. Maschinenbau",
                                EmpfohlenesSemester = 2,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = "Grundlagen Maschinenbau",
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.NurWintersemester,
                                PruefungsForm = Modul.PruefungsFormEnum.PL,
                                GueltigAb = DateTime.Now,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Aenderungsbedarf,
                                        GueltigAbSemester = "SoSe 21",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 30, WorkloadSelbststudium = 120,
                                        Pruefungsform = "Hausarbeit",
                                        LernergebnisseDb = "Die Studierenden beherrschen die Hauptsätze der Thermodynamik.\n" +
                                   "Sie verstehen das Verhalten idealer Gase und können Zustandsänderungen berechnen.\n" +
                                   "Sie können Kreisprozesse (z.B. Carnot) analysieren und Wirkungsgrade ermitteln.\n" +
                                   "Sie verstehen den Begriff der Entropie und Irreversibilität.",
                                        InhaltsgliederungDb = "1. Grundbegriffe und thermodynamische Systeme\n" +
                                      "2. Zustandsgrößen und Zustandsgleichungen idealer Gase\n" +
                                      "3. Der 1. Hauptsatz der Thermodynamik (Energieerhaltung)\n" +
                                      "4. Der 2. Hauptsatz der Thermodynamik (Entropie)\n" +
                                      "5. Kreisprozesse und thermische Maschinen\n" +
                                      "6. Wärmeübertragung (Grundlagen)",
                                        Ersteller = dozent.Name
                                    }
                                }
                            },
                            new Modul
                            {
                                ModulnameDE = "Grundlagen Maschinenbau",
                                ModulnameEN = null,
                                Studiengang = "B.Eng. Maschinenbau",
                                EmpfohlenesSemester = 1,
                                DauerInSemestern = 1,
                                VoraussetzungenDb = null,
                                Modultyp = Modul.ModultypEnum.Grundlagen,
                                Turnus = Modul.TurnusEnum.JedesSemester,
                                PruefungsForm = Modul.PruefungsFormEnum.SP,
                                GueltigAb = DateTime.Now,

                                ModulVersionen = new List<ModulVersion>
                                {
                                    new ModulVersion
                                    {
                                        Versionsnummer = 10,  // ? FIX: 1.0 = 10
                                        ModulStatus = ModulVersion.Status.Freigegeben,
                                        GueltigAbSemester = "WiSe 1819",
                                        EctsPunkte = 5,
                                        WorkloadPraesenz = 60, WorkloadSelbststudium = 90,
                                        Pruefungsform = "Praesentation, Abgaben",
                                        LernergebnisseDb = "Die Studierenden haben einen Überblick über die Teilgebiete des Maschinenbaus.\n" +
                                   "Sie kennen die wichtigsten Werkstoffgruppen (Metalle, Kunststoffe).\n" +
                                   "Sie können einfache technische Zeichnungen nach Norm lesen und erstellen.\n" +
                                   "Sie verstehen grundlegende Fertigungsverfahren (Urformen, Umformen, Trennen).",
                                        InhaltsgliederungDb = "1. Einführung und Berufsbild des Ingenieurs\n" +
                                      "2. Grundlagen der Werkstoffkunde (Eisen, Stahl, NE-Metalle)\n" +
                                      "3. Technische Kommunikation und Normung (DIN/ISO)\n" +
                                      "4. Maschinenelemente (Schrauben, Lager, Zahnräder)\n" +
                                      "5. Überblick Fertigungstechnik\n" +
                                      "6. CAD-Einführung",
                                        Ersteller = dozent.Name
                                    }
                                }
                            }
                    };


                        db.Modul.AddRange(neueModule);

                        // --- ABSCHLUSS ---

                        db.SaveChanges(); // Alles in die DB schreiben

                        var seModul = db.Modul.Include("ModulVersionen")
                .FirstOrDefault(m => m.ModulnameDE == "Software Engineering I");

                        var thermoModul = db.Modul.Include("ModulVersionen")
                                            .FirstOrDefault(m => m.ModulnameDE == "Thermodynamik I");

                        var pyroModul = db.Modul.Include("ModulVersionen")
                                          .FirstOrDefault(m => m.ModulnameDE == "Einführung in die Pyrotechnik");
                        if (seModul != null && thermoModul != null && pyroModul != null)
                        {
                            var kommentare = new List<Kommentar>
    {
        // Kommentar 1: Zu Software Engineering - Literatur
        new Kommentar
        {
            FeldName = "Literatur",
            Text = "Die angegebene Literaturliste ist veraltet und entspricht nicht mehr dem aktuellen Stand der Technik (Agile Methoden fehlen).",
            ErstellungsDatum = DateTime.Now.AddDays(-10),
            Ersteller = "Prof. Dr. Koordinator",
            GehoertZuModulID = seModul.ModulID,
            GehoertZuModulVersionID = seModul.ModulVersionen.Last().ModulVersionID
        },
        // Kommentar 2: Zu Software Engineering - Lernziele
        new Kommentar
        {
            FeldName = "Lernziele",
            Text = "Die Lernziele sind zu ungenau formuliert. Bitte konkreter beschreiben.",
            ErstellungsDatum = DateTime.Now.AddDays(-10),
            Ersteller = "Prof. Dr. Koordinator",
            GehoertZuModulID = seModul.ModulID,
            GehoertZuModulVersionID = seModul.ModulVersionen.Last().ModulVersionID
        },

        // Kommentar 3: Zu Thermodynamik - Prüfungsform
        new Kommentar
        {
            FeldName = "Prüfungsform",
            Text = "Die Prüfungsdichte im 2. Semester ist bereits sehr hoch. Eine 120-minütige Klausur wird als unverhältnismäßig angesehen. Wir fordern eine Änderung der Prüfungsform hin zu semesterbegleitenden Leistungen oder eine Reduktion des Stoffumfangs.",
            ErstellungsDatum = DateTime.Now.AddMonths(-2),
            Ersteller = "Gremium",
            GehoertZuModulID = thermoModul.ModulID,
            GehoertZuModulVersionID = thermoModul.ModulVersionen.First().ModulVersionID
        },

        // Kommentar 4: Zu Pyrotechnik - Voraussetzungen
        new Kommentar
        {
            FeldName = "Voraussetzungen",
            Text = "Es fehlt ein expliziter Vermerk, dass eine Sicherheitsbelehrung vor der ersten praktischen Übung verpflichtend ist. Bitte fügen Sie den entsprechenden Textbaustein aus dem Arbeitssicherheit-Handbuch ein.",
            ErstellungsDatum = DateTime.Now.AddDays(-2),
            Ersteller = "Prof. Dr. Koordinator",
            GehoertZuModulID = pyroModul.ModulID,
            GehoertZuModulVersionID = pyroModul.ModulVersionen.First().ModulVersionID
        }
    };

                            db.Kommentar.AddRange(kommentare);

                            // Module als kommentiert markieren
                            seModul.ModulVersionen.Last().hatKommentar = true;
                            thermoModul.ModulVersionen.First().hatKommentar = true;
                            pyroModul.ModulVersionen.First().hatKommentar = true;

                            db.SaveChanges();
                            Console.WriteLine("4 feldspezifische Kommentare erfolgreich angelegt.");
                        }
                        else
                        {
                            Console.WriteLine("WARNUNG: Konnte Module für Kommentare nicht finden (Namen prüfen!).");
                        }

                        var seedBenachrichtigungen = new List<Benachrichtigung>
{
    // --- GAST (Allgemeine Infos) ---
    new Benachrichtigung
    {
        Empfaenger = "Gast",
        Sender = "System",
        Nachricht = "Willkommen im Modul-Management-System. Sie haben Lesezugriff auf alle veröffentlichten Module.",
        GesendetAm = DateTime.Now.AddDays(-5),
        Gelesen = true, // Schon gelesen
        BetroffeneModulVersionID = null
    },
    new Benachrichtigung
    {
        Empfaenger = "Gast",
        Sender = "IT-Support",
        Nachricht = "Wartungsarbeiten: Das System ist am Sonntag zwischen 02:00 und 04:00 Uhr nicht erreichbar.",
        GesendetAm = DateTime.Now.AddHours(-2),
        Gelesen = false,
        BetroffeneModulVersionID = null
    },

    // --- DOZENT (Handlungsbedarf & Feedback) ---
    new Benachrichtigung
    {
        Empfaenger = "Dozent",
        Sender = "Koordination",
        Nachricht = "Ihr Modul 'Mathe I' wurde zur Überarbeitung zurückgegeben. Bitte prüfen Sie die ECTS-Verteilung.",
        GesendetAm = DateTime.Now.AddHours(-24),
        Gelesen = false,
        BetroffeneModulVersionID = 1 // Annahme: Es gibt eine Version 1
    },
    new Benachrichtigung
    {
        Empfaenger = "Dozent",
        Sender = "System",
        Nachricht = "Erinnerung: Die Einreichungsfrist für das kommende Semester endet in 3 Tagen.",
        GesendetAm = DateTime.Now.AddHours(-1),
        Gelesen = false,
        BetroffeneModulVersionID = null
    },
    new Benachrichtigung
    {
        Empfaenger = "Dozent",
        Sender = "Gremium",
        Nachricht = "Glückwunsch! Ihr Modul 'Informatik Grundlagen' wurde ohne Änderungen genehmigt.",
        GesendetAm = DateTime.Now.AddDays(-2),
        Gelesen = true,
        BetroffeneModulVersionID = 5 // Annahme: Version 5 existiert
    },

    // --- KOORDINATION (Prüfungsaufgaben) ---
    new Benachrichtigung
    {
        Empfaenger = "Koordination",
        Sender = "System",
        Nachricht = "Neues Modul eingereicht: 'Fortgeschrittene Algorithmen' wartet auf formale Prüfung.",
        GesendetAm = DateTime.Now.AddMinutes(-30),
        Gelesen = false,
        BetroffeneModulVersionID = 12
    },
    new Benachrichtigung
    {
        Empfaenger = "Koordination",
        Sender = "Dozent",
        Nachricht = "Ich habe die Änderungen an 'Physik II' vorgenommen. Können Sie kurz drüberschauen?",
        GesendetAm = DateTime.Now.AddHours(-5),
        Gelesen = false,
        BetroffeneModulVersionID = 8
    },

    // --- GREMIUM (Entscheidungen) ---
    new Benachrichtigung
    {
        Empfaenger = "Gremium",
        Sender = "Koordination",
        Nachricht = "Agenda für nächste Sitzung: 5 Module stehen zur finalen Genehmigung bereit.",
        GesendetAm = DateTime.Now.AddDays(-1),
        Gelesen = false,
        BetroffeneModulVersionID = null
    },
    new Benachrichtigung
    {
        Empfaenger = "Gremium",
        Sender = "Dekan",
        Nachricht = "Bitte beachten Sie die neue Prüfungsordnung bei der Genehmigung von Master-Modulen.",
        GesendetAm = DateTime.Now.AddDays(-7),
        Gelesen = true, // Alte Info
        BetroffeneModulVersionID = null
    },

    // --- ADMIN (Systemstatus) ---
    new Benachrichtigung
    {
        Empfaenger = "Admin",
        Sender = "System",
        Nachricht = "Datenbank-Backup erfolgreich erstellt.",
        GesendetAm = DateTime.Now.AddHours(-12),
        Gelesen = false,
        BetroffeneModulVersionID = null
    },
    new Benachrichtigung
    {
        Empfaenger = "Admin",
        Sender = "System",
        Nachricht = "Warnung: Hohe Serverlast festgestellt. Performance könnte beeinträchtigt sein.",
        GesendetAm = DateTime.Now.AddMinutes(-10),
        Gelesen = false,
        BetroffeneModulVersionID = null
    }
};

                        // Speichern in die DB
                        db.Benachrichtigung.AddRange(seedBenachrichtigungen);
                        db.SaveChanges();

                        transaction.Commit(); // Transaktion bestätigen

                        Console.WriteLine("Datenbank erfolgreich befüllt!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Bei Fehler alles rückgängig machen
                        Console.WriteLine("Fehler bei der Initialisierung: " + ex.Message);
                        // Optional: Exception weiterwerfen, damit die App crasht und man den Fehler sieht
                        throw;
                    }
                }
            }
        }
    }
}
