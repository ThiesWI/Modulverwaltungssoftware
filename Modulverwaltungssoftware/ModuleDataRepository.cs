using System;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Repository für Module und deren Versionen (später durch DB ersetzen)
    /// </summary>
    public static class ModuleDataRepository
    {
        // Gemeinsame Datenklasse für Modulversion-Daten
        public class ModuleData
        {
            public string Titel { get; set; }
            public List<string> Modultypen { get; set; }
            public string Studiengang { get; set; }
            public List<string> Semester { get; set; }
            public List<string> Pruefungsformen { get; set; }
            public List<string> Turnus { get; set; }
            public int Ects { get; set; }
            public int WorkloadPraesenz { get; set; }
            public int WorkloadSelbststudium { get; set; }
            public string Verantwortlicher { get; set; }
            public string Voraussetzungen { get; set; }
            public string Lernziele { get; set; }
            public string Lehrinhalte { get; set; }
            public string Literatur { get; set; }
        }

        // Klasse für eine Modulversion
        public class ModuleVersion
        {
            public string VersionsNummer { get; set; }  // z.B. "1.0", "1.1"
            public ModuleData Daten { get; set; }
            public DateTime ErstellDatum { get; set; }
            public string Status { get; set; }          // "Entwurf", "Eingereicht", "Freigegeben"
        }

        // Klasse für ein Modul (kann mehrere Versionen haben)
        public class Module
        {
            public string ModulId { get; set; }         // Eindeutige ID
            public string ModulName { get; set; }       // Anzeigename
            public string Ersteller { get; set; }       // User, der das Modul erstellt hat
            public List<ModuleVersion> Versionen { get; set; } = new List<ModuleVersion>();
        }

        // Gemeinsame Datenbank: ModulID ? Modul
        private static Dictionary<string, Module> _modules = new Dictionary<string, Module>();
        private static int _nextModulId = 1;

        static ModuleDataRepository()
        {
            InitializePlaceholderData();
        }

        private static void InitializePlaceholderData()
        {
            // Modul 1: Softwareentwicklung Grundlagen (2 Versionen)
            var modul1 = new Module
            {
                ModulId = "MOD001",
                ModulName = "Softwareentwicklung Grundlagen",
                Ersteller = "P. Brandenburg",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-6),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "Softwareentwicklung Grundlagen",
                            Modultypen = new List<string> { "Pflichtmodul", "Grundlagenmodul" },
                            Studiengang = "Wirtschaftsinformatik",
                            Semester = new List<string> { "1", "2" },
                            Pruefungsformen = new List<string> { "Klausur", "Projektarbeit (Belegarbeit)" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 6,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 120,
                            Verantwortlicher = "Prof. Dr. Müller",
                            Voraussetzungen = "Keine formalen Voraussetzungen, Grundkenntnisse in Programmierung von Vorteil.",
                            Lernziele = "Die Studierenden können objektorientierte Programmierkonzepte anwenden und eigenständig kleine Softwareprojekte umsetzen.",
                            Lehrinhalte = "Einführung in OOP, UML-Diagramme, Design Patterns, Versionskontrolle mit Git, Agile Methoden.",
                            Literatur = "Gamma et al.: Design Patterns (1994), Martin: Clean Code (2008)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-2),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Softwareentwicklung Grundlagen",
                            Modultypen = new List<string> { "Pflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik, Angewandte Informatik",
                            Semester = new List<string> { "1" },
                            Pruefungsformen = new List<string> { "Klausur" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 5,
                            WorkloadPraesenz = 45,
                            WorkloadSelbststudium = 105,
                            Verantwortlicher = "Prof. Dr. Müller, Dr. Schmidt",
                            Voraussetzungen = "Keine",
                            Lernziele = "Erweiterte OOP-Kenntnisse, TDD-Ansätze verstehen.",
                            Lehrinhalte = "OOP-Vertiefung, Test-Driven Development, Refactoring.",
                            Literatur = "Fowler: Refactoring (2018), Beck: TDD by Example (2002)"
                        }
                    }
                }
            };

            // Modul 2: Advanced Software Engineering (1 Version)
            var modul2 = new Module
            {
                ModulId = "MOD002",
                ModulName = "Advanced Software Engineering",
                Ersteller = "Dr. K. Schmidt",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-1),
                        Status = "Eingereicht",
                        Daten = new ModuleData
                        {
                            Titel = "Advanced Software Engineering",
                            Modultypen = new List<string> { "Spezialisierungsmodul", "Wahlmodul (Freies Wahlfach)" },
                            Studiengang = "Master Wirtschaftsinformatik",
                            Semester = new List<string> { "3", "4" },
                            Pruefungsformen = new List<string> { "Hausarbeit (Seminararbeit)", "Präsentation" },
                            Turnus = new List<string> { "Jährlich (SoSe)" },
                            Ects = 8,
                            WorkloadPraesenz = 48,
                            WorkloadSelbststudium = 192,
                            Verantwortlicher = "Prof. Dr. Lange",
                            Voraussetzungen = "Abgeschlossenes Modul Softwareentwicklung Grundlagen, Kenntnisse in Java/C#.",
                            Lernziele = "Architekturen großer Systeme entwerfen, Microservices entwickeln, CI/CD implementieren.",
                            Lehrinhalte = "Architekturmuster, Microservices, Docker/Kubernetes, CI/CD-Pipelines, Cloud-Deployment.",
                            Literatur = "Newman: Building Microservices (2021), Fowler: Patterns of Enterprise Application Architecture (2002)"
                        }
                    }
                }
            };

            _modules["MOD001"] = modul1;
            _modules["MOD002"] = modul2;
            _nextModulId = 3;
        }

        // --- NEUE API ---

        /// <summary>Gibt alle Module zurück</summary>
        public static List<Module> GetAllModules()
        {
            return _modules.Values.ToList();
        }

        /// <summary>Gibt Module zurück, die einem User gehören</summary>
        public static List<Module> GetModulesByUser(string userName)
        {
            return _modules.Values.Where(m => m.Ersteller == userName).ToList();
        }

        /// <summary>Gibt ein Modul anhand der ID zurück</summary>
        public static Module GetModule(string modulId)
        {
            return _modules.ContainsKey(modulId) ? _modules[modulId] : null;
        }

        /// <summary>Gibt eine bestimmte Version eines Moduls zurück</summary>
        public static ModuleData GetModuleVersion(string modulId, string versionNummer)
        {
            var modul = GetModule(modulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);
            return version?.Daten;
        }

        /// <summary>Erstellt ein neues Modul mit Version 1.0</summary>
        public static string CreateModule(string modulName, string ersteller, ModuleData daten)
        {
            string modulId = $"MOD{_nextModulId:D3}";
            _nextModulId++;

            var neuesModul = new Module
            {
                ModulId = modulId,
                ModulName = modulName,
                Ersteller = ersteller,
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now,
                        Status = "Entwurf",
                        Daten = daten
                    }
                }
            };

            _modules[modulId] = neuesModul;
            return modulId;
        }

        /// <summary>Aktualisiert eine bestehende Version eines Moduls</summary>
        public static void UpdateModuleVersion(string modulId, string versionNummer, ModuleData daten)
        {
            var modul = GetModule(modulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);
            if (version != null)
            {
                version.Daten = daten;
            }
        }

        /// <summary>Erstellt eine neue Version für ein bestehendes Modul</summary>
        public static void CreateNewVersion(string modulId, string versionNummer, ModuleData daten)
        {
            var modul = GetModule(modulId);
            if (modul != null)
            {
                modul.Versionen.Add(new ModuleVersion
                {
                    VersionsNummer = versionNummer,
                    ErstellDatum = DateTime.Now,
                    Status = "Entwurf",
                    Daten = daten
                });
            }
        }

        // --- ALTE API (für Abwärtskompatibilität, DEPRECATED) ---

        [Obsolete("Bitte GetModuleVersion() verwenden")]
        public static ModuleData GetVersion(string version)
        {
            // Alte Methode: durchsucht alle Module nach Version
            foreach (var modul in _modules.Values)
            {
                var v = modul.Versionen.FirstOrDefault(ver => ver.VersionsNummer == version);
                if (v != null) return v.Daten;
            }
            return null;
        }

        [Obsolete("Bitte UpdateModuleVersion() oder CreateModule() verwenden")]
        public static void SaveVersion(string version, ModuleData data)
        {
            // Alte Methode: speichert unter erster passender Version
            foreach (var modul in _modules.Values)
            {
                var v = modul.Versionen.FirstOrDefault(ver => ver.VersionsNummer == version);
                if (v != null)
                {
                    v.Daten = data;
                    return;
                }
            }
        }

        [Obsolete("Bitte GetAllModules() verwenden und dann Versionen auslesen")]
        public static List<string> GetAllVersions()
        {
            var versions = new List<string>();
            foreach (var modul in _modules.Values)
            {
                versions.AddRange(modul.Versionen.Select(v => v.VersionsNummer));
            }
            return versions;
        }
    }
}
