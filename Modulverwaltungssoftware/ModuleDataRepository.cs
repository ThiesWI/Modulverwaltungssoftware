using System;
using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Gemeinsames Repository für Modulversionen (später durch DB ersetzen)
    /// </summary>
    public static class ModuleDataRepository
    {
        // Gemeinsame Datenklasse für Modulversion
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

        // Gemeinsame Datenbank (später durch echte DB ersetzen)
        private static Dictionary<string, ModuleData> _moduleVersions = new Dictionary<string, ModuleData>();

        static ModuleDataRepository()
        {
            // Platzhalter-Daten initialisieren
            InitializePlaceholderData();
        }

        private static void InitializePlaceholderData()
        {
            _moduleVersions["v1.0"] = new ModuleData
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
            };

            _moduleVersions["v1.1"] = new ModuleData
            {
                Titel = "Softwareentwicklung Grundlagen (überarbeitet)",
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
            };

            _moduleVersions["v2.0"] = new ModuleData
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
            };
        }

        public static ModuleData GetVersion(string version)
        {
            return _moduleVersions.ContainsKey(version) ? _moduleVersions[version] : null;
        }

        public static void SaveVersion(string version, ModuleData data)
        {
            _moduleVersions[version] = data;
        }

        public static List<string> GetAllVersions()
        {
            return new List<string>(_moduleVersions.Keys);
        }
    }
}
