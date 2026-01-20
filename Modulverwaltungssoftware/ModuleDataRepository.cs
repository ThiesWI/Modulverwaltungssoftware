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
            public double Ects { get; set; }
            public int WorkloadPraesenz { get; set; }
            public int WorkloadSelbststudium { get; set; }
            public string Verantwortlicher { get; set; }
            public string Voraussetzungen { get; set; }
            public string Lernziele { get; set; }
            public string Lehrinhalte { get; set; }
            public string Literatur { get; set; }
        }

        // Klasse für Kommentardaten zu einem Feld
        public class FieldComment
        {
            public string FieldName { get; set; }       // z.B. "Titel", "Modultyp"
            public string Comment { get; set; }         // Kommentartext
            public DateTime CommentDate { get; set; }   // Wann wurde kommentiert
            public string Commenter { get; set; }       // Wer hat kommentiert
        }

        // Klasse für alle Kommentare zu einer Version
        public class CommentData
        {
            public List<FieldComment> FieldComments { get; set; } = new List<FieldComment>();
            public DateTime SubmittedDate { get; set; }  // Wann wurden Kommentare eingereicht
            public string SubmittedBy { get; set; }      // Wer hat eingereicht
        }

        // Klasse für eine Modulversion
        public class ModuleVersion
        {
            public string VersionsNummer { get; set; }  // z.B. "1.0", "1.1"
            public ModuleData Daten { get; set; }
            public DateTime ErstellDatum { get; set; }
            public string Status { get; set; }          // "Entwurf", "Eingereicht", "Freigegeben"
            public bool HasComments { get; set; }       // Wurde kommentiert?
            public CommentData Kommentare { get; set; } // Kommentardaten
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
            // MODUL 1: Datenbanksysteme (3 Versionen)
            _modules["MOD001"] = new Module
            {
                ModulId = "MOD001",
                ModulName = "Datenbanksysteme",
                Ersteller = "P. Brandenburg",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-12),
                        Status = "Freigegeben",
                        HasComments = false,
                        Kommentare = null,
                        Daten = new ModuleData
                        {
                            Titel = "Datenbanksysteme",
                            Modultypen = new List<string> { "Pflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik",
                            Semester = new List<string> { "2" },
                            Pruefungsformen = new List<string> { "Klausur" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 5,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 90,
                            Verantwortlicher = "Prof. Dr. Weber",
                            Voraussetzungen = "Grundkenntnisse in Informatik und Mathematik.",
                            Lernziele = "Studierende können relationale Datenbanken entwerfen, SQL-Abfragen formulieren und Transaktionskonzepte anwenden.",
                            Lehrinhalte = "ER-Modellierung, Relationales Modell, SQL (DDL, DML, DQL), Normalformen, Indexierung, Transaktionen, ACID-Prinzipien.",
                            Literatur = "Elmasri/Navathe: Fundamentals of Database Systems (2015), Date: An Introduction to Database Systems (2003)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-6),
                        Status = "Eingereicht",
                        HasComments = true,  // Diese Version wurde kommentiert
                        Kommentare = new CommentData
                        {
                            SubmittedDate = DateTime.Now.AddMonths(-5).AddDays(-15),
                            SubmittedBy = "Prof. Dr. Lange",
                            FieldComments = new List<FieldComment>
                            {
                                new FieldComment
                                {
                                    FieldName = "Studiengang",
                                    Comment = "Sollte auch für Data Science-Studierende geöffnet werden.",
                                    CommentDate = DateTime.Now.AddMonths(-5).AddDays(-15),
                                    Commenter = "Prof. Dr. Lange"
                                },
                                new FieldComment
                                {
                                    FieldName = "ECTS",
                                    Comment = "6 ECTS erscheinen angemessen für den erweiterten Umfang.",
                                    CommentDate = DateTime.Now.AddMonths(-5).AddDays(-15),
                                    Commenter = "Prof. Dr. Lange"
                                },
                                new FieldComment
                                {
                                    FieldName = "Lehrinhalte",
                                    Comment = "Sehr gute Erweiterung um NoSQL! Bitte auch Graph-Datenbanken ergänzen.",
                                    CommentDate = DateTime.Now.AddMonths(-5).AddDays(-15),
                                    Commenter = "Prof. Dr. Lange"
                                }
                            }
                        },
                        Daten = new ModuleData
                        {
                            Titel = "Datenbanksysteme",
                            Modultypen = new List<string> { "Pflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik, Angewandte Informatik",
                            Semester = new List<string> { "2", "3" },
                            Pruefungsformen = new List<string> { "Klausur", "Projektarbeit (Belegarbeit)" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 6,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 120,
                            Verantwortlicher = "Prof. Dr. Weber, M.Sc. Fischer",
                            Voraussetzungen = "Grundkenntnisse in Informatik, erfolgreicher Abschluss des Moduls 'Programmierung 1'.",
                            Lernziele = "Erweiterte Datenbankkonzepte verstehen, NoSQL-Datenbanken anwenden, Performance-Tuning durchführen.",
                            Lehrinhalte = "ER-Modellierung, SQL (erweitert), NoSQL-Datenbanken (MongoDB, Redis), Query-Optimierung, Replikation, Sharding.",
                            Literatur = "Elmasri/Navathe: Fundamentals of Database Systems (2015), Redmond/Wilson: Seven Databases in Seven Weeks (2018)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "2.0",
                        ErstellDatum = DateTime.Now.AddMonths(-1),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Moderne Datenbanksysteme und Big Data",
                            Modultypen = new List<string> { "Pflichtmodul", "Spezialisierungsmodul" },
                            Studiengang = "Wirtschaftsinformatik, Data Science",
                            Semester = new List<string> { "3" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Präsentation" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 7,
                            WorkloadPraesenz = 75,
                            WorkloadSelbststudium = 135,
                            Verantwortlicher = "Prof. Dr. Weber, Dr. Chen",
                            Voraussetzungen = "Programmierung 1 und 2, Grundlagen der Datenstrukturen.",
                            Lernziele = "Cloud-Datenbanken verwalten, Big-Data-Technologien einsetzen, Data Warehouses konzipieren.",
                            Lehrinhalte = "Cloud-Datenbanken (AWS RDS, Azure SQL), Big Data (Hadoop, Spark), Data Warehousing, Graph-Datenbanken (Neo4j), Time-Series DBs.",
                            Literatur = "Kleppmann: Designing Data-Intensive Applications (2017), White: Hadoop - The Definitive Guide (2020)"
                        }
                    }
                }
            };

            // MODUL 2: Web-Entwicklung (3 Versionen)
            _modules["MOD002"] = new Module
            {
                ModulId = "MOD002",
                ModulName = "Web-Entwicklung",
                Ersteller = "Dr. K. Schmidt",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-10),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "Web-Entwicklung",
                            Modultypen = new List<string> { "Wahlpflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik",
                            Semester = new List<string> { "4", "5" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)" },
                            Turnus = new List<string> { "Jährlich (SoSe)" },
                            Ects = 6,
                            WorkloadPraesenz = 48,
                            WorkloadSelbststudium = 132,
                            Verantwortlicher = "Dr. Schmidt",
                            Voraussetzungen = "Grundkenntnisse in HTML, CSS und JavaScript.",
                            Lernziele = "Moderne Webanwendungen mit Frontend-Frameworks entwickeln, RESTful APIs gestalten, responsives Design umsetzen.",
                            Lehrinhalte = "HTML5, CSS3, JavaScript (ES6+), React/Vue.js, REST APIs, Responsive Design, Browser DevTools.",
                            Literatur = "Duckett: HTML and CSS (2011), Simpson: You Don't Know JS (2015), Flanagan: JavaScript - The Definitive Guide (2020)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-4),
                        Status = "Eingereicht",
                        Daten = new ModuleData
                        {
                            Titel = "Web-Entwicklung",
                            Modultypen = new List<string> { "Wahlpflichtmodul", "Spezialisierungsmodul" },
                            Studiengang = "Wirtschaftsinformatik, Medieninformatik",
                            Semester = new List<string> { "4", "5", "6" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Präsentation" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 7,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 150,
                            Verantwortlicher = "Dr. Schmidt, M.Sc. Bauer",
                            Voraussetzungen = "Programmierung 1, Grundkenntnisse in HTML/CSS/JavaScript.",
                            Lernziele = "Full-Stack-Anwendungen entwickeln, State Management implementieren, Performance optimieren, Testing durchführen.",
                            Lehrinhalte = "React/Angular, Node.js, Express, State Management (Redux), Testing (Jest, Cypress), Build Tools (Webpack, Vite), Performance-Optimierung.",
                            Literatur = "Haverbeke: Eloquent JavaScript (2018), Banks/Porcello: Learning React (2020), Young: Fullstack React (2019)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "2.0",
                        ErstellDatum = DateTime.Now.AddDays(-14),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Modern Web Development & Cloud Deployment",
                            Modultypen = new List<string> { "Spezialisierungsmodul" },
                            Studiengang = "Master Wirtschaftsinformatik, Medieninformatik",
                            Semester = new List<string> { "1", "2" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Portfolio" },
                            Turnus = new List<string> { "Jährlich (SoSe)" },
                            Ects = 8,
                            WorkloadPraesenz = 64,
                            WorkloadSelbststudium = 176,
                            Verantwortlicher = "Dr. Schmidt, Prof. Dr. Neumann",
                            Voraussetzungen = "Web-Entwicklung Grundlagen, Kenntnisse in JavaScript und mindestens einem Framework.",
                            Lernziele = "Progressive Web Apps erstellen, Serverless-Architekturen nutzen, CI/CD-Pipelines implementieren, Cloud-native entwickeln.",
                            Lehrinhalte = "Next.js/Nuxt.js, TypeScript, GraphQL, Serverless (AWS Lambda, Vercel), PWAs, Docker, Kubernetes, CI/CD (GitHub Actions), Monitoring.",
                            Literatur = "Addy Osmani: Learning JavaScript Design Patterns (2020), Sam Newman: Building Microservices (2021), Cloud Native Patterns (2019)"
                        }
                    }
                }
            };

            // MODUL 3: Künstliche Intelligenz (3 Versionen)
            _modules["MOD003"] = new Module
            {
                ModulId = "MOD003",
                ModulName = "Künstliche Intelligenz",
                Ersteller = "Prof. Dr. Neumann",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-15),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "Künstliche Intelligenz - Grundlagen",
                            Modultypen = new List<string> { "Wahlmodul (Freies Wahlfach)" },
                            Studiengang = "Informatik, Wirtschaftsinformatik",
                            Semester = new List<string> { "5", "6" },
                            Pruefungsformen = new List<string> { "Klausur", "Mündliche Prüfung" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 5,
                            WorkloadPraesenz = 45,
                            WorkloadSelbststudium = 105,
                            Verantwortlicher = "Prof. Dr. Neumann",
                            Voraussetzungen = "Mathematik für Informatiker, Algorithmen und Datenstrukturen.",
                            Lernziele = "Grundlegende KI-Konzepte verstehen, einfache Suchverfahren implementieren, Machine Learning-Basics anwenden.",
                            Lehrinhalte = "Geschichte der KI, Suchverfahren (BFS, DFS, A*), Wissensrepräsentation, Logik, Einführung Machine Learning (Regression, Klassifikation).",
                            Literatur = "Russell/Norvig: Artificial Intelligence - A Modern Approach (2020), Mitchell: Machine Learning (1997)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-8),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "Künstliche Intelligenz und Machine Learning",
                            Modultypen = new List<string> { "Wahlpflichtmodul", "Spezialisierungsmodul" },
                            Studiengang = "Informatik, Wirtschaftsinformatik, Data Science",
                            Semester = new List<string> { "5", "6", "7" },
                            Pruefungsformen = new List<string> { "Klausur", "Projektarbeit (Belegarbeit)" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 7,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 150,
                            Verantwortlicher = "Prof. Dr. Neumann, Dr. Park",
                            Voraussetzungen = "Lineare Algebra, Statistik, Programmierung in Python.",
                            Lernziele = "Supervised und Unsupervised Learning anwenden, neuronale Netze trainieren, Model Evaluation durchführen.",
                            Lehrinhalte = "Supervised Learning (SVM, Decision Trees, Random Forest), Unsupervised Learning (K-Means, PCA), Neuronale Netze, Backpropagation, TensorFlow/PyTorch.",
                            Literatur = "Goodfellow et al.: Deep Learning (2016), Géron: Hands-On Machine Learning with Scikit-Learn and TensorFlow (2019)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "2.0",
                        ErstellDatum = DateTime.Now.AddDays(-21),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Deep Learning und KI-Anwendungen",
                            Modultypen = new List<string> { "Spezialisierungsmodul" },
                            Studiengang = "Master Informatik, Data Science",
                            Semester = new List<string> { "2", "3" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Portfolio" },
                            Turnus = new List<string> { "Jährlich (SoSe)" },
                            Ects = 9,
                            WorkloadPraesenz = 72,
                            WorkloadSelbststudium = 198,
                            Verantwortlicher = "Prof. Dr. Neumann, Dr. Park, M.Sc. Lee",
                            Voraussetzungen = "Machine Learning Grundlagen, Python-Kenntnisse, Lineare Algebra und Statistik.",
                            Lernziele = "CNNs und RNNs entwickeln, Transformer-Modelle nutzen, Reinforcement Learning implementieren, KI-Ethik reflektieren.",
                            Lehrinhalte = "Convolutional Neural Networks (CNNs), Recurrent Neural Networks (RNNs), Transformer (BERT, GPT), Reinforcement Learning, GANs, KI-Ethik.",
                            Literatur = "Zhang et al.: Dive into Deep Learning (2021), Sutton/Barto: Reinforcement Learning (2018), Vaswani et al.: Attention is All You Need (2017)"
                        }
                    }
                }
            };

            // MODUL 4: IT-Sicherheit (3 Versionen)
            _modules["MOD004"] = new Module
            {
                ModulId = "MOD004",
                ModulName = "IT-Sicherheit",
                Ersteller = "P. Brandenburg",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-9),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "IT-Sicherheit - Grundlagen",
                            Modultypen = new List<string> { "Pflichtmodul" },
                            Studiengang = "Informatik",
                            Semester = new List<string> { "3" },
                            Pruefungsformen = new List<string> { "Klausur" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 5,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 90,
                            Verantwortlicher = "Prof. Dr. Stein",
                            Voraussetzungen = "Grundlagen der Informatik, Netzwerktechnologien.",
                            Lernziele = "Sicherheitsbedrohungen erkennen, Verschlüsselungsverfahren anwenden, sichere Systeme gestalten.",
                            Lehrinhalte = "CIA-Triad, Kryptographie (symmetrisch/asymmetrisch), Hashfunktionen, PKI, Netzwerksicherheit, Firewalls, VPN.",
                            Literatur = "Stallings: Cryptography and Network Security (2016), Anderson: Security Engineering (2020)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-5),
                        Status = "Eingereicht",
                        Daten = new ModuleData
                        {
                            Titel = "IT-Sicherheit und Datenschutz",
                            Modultypen = new List<string> { "Pflichtmodul", "Wahlpflichtmodul" },
                            Studiengang = "Informatik, Wirtschaftsinformatik",
                            Semester = new List<string> { "3", "4" },
                            Pruefungsformen = new List<string> { "Klausur", "Hausarbeit (Seminararbeit)" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 6,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 120,
                            Verantwortlicher = "Prof. Dr. Stein, RA Müller",
                            Voraussetzungen = "Netzwerktechnologien, Betriebssysteme.",
                            Lernziele = "Sicherheitsarchitekturen entwerfen, Penetration Testing durchführen, DSGVO-Compliance sicherstellen.",
                            Lehrinhalte = "Kryptographie (erweitert), Web Security (XSS, CSRF, SQL Injection), Penetration Testing, DSGVO, ISO 27001, Incident Response.",
                            Literatur = "Stallings: Cryptography and Network Security (2016), Stuttard/Pinto: The Web Application Hacker's Handbook (2011)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "2.0",
                        ErstellDatum = DateTime.Now.AddDays(-10),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Cybersecurity und Ethical Hacking",
                            Modultypen = new List<string> { "Spezialisierungsmodul" },
                            Studiengang = "Master Informatik, IT-Security",
                            Semester = new List<string> { "1", "2" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Präsentation" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 8,
                            WorkloadPraesenz = 64,
                            WorkloadSelbststudium = 176,
                            Verantwortlicher = "Prof. Dr. Stein, Dr. Black",
                            Voraussetzungen = "IT-Sicherheit Grundlagen, Netzwerkprotokolle, Linux-Kenntnisse.",
                            Lernziele = "Offensive Security-Techniken anwenden, Security Operations Center (SOC) verstehen, Malware analysieren, Zero Trust implementieren.",
                            Lehrinhalte = "Advanced Penetration Testing, Exploit Development, Malware Analysis, Reverse Engineering, SOC/SIEM, Zero Trust Architecture, Cloud Security.",
                            Literatur = "Erickson: Hacking - The Art of Exploitation (2008), Sikorski/Honig: Practical Malware Analysis (2012), NIST Cybersecurity Framework (2023)"
                        }
                    }
                }
            };

            // MODUL 5: Projektmanagement (3 Versionen)
            _modules["MOD005"] = new Module
            {
                ModulId = "MOD005",
                ModulName = "Projektmanagement",
                Ersteller = "Dr. K. Schmidt",
                Versionen = new List<ModuleVersion>
                {
                    new ModuleVersion
                    {
                        VersionsNummer = "1.0",
                        ErstellDatum = DateTime.Now.AddMonths(-14),
                        Status = "Freigegeben",
                        Daten = new ModuleData
                        {
                            Titel = "Projektmanagement - Grundlagen",
                            Modultypen = new List<string> { "Wahlpflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik, BWL",
                            Semester = new List<string> { "4", "5" },
                            Pruefungsformen = new List<string> { "Klausur" },
                            Turnus = new List<string> { "Jährlich (SoSe)" },
                            Ects = 5,
                            WorkloadPraesenz = 45,
                            WorkloadSelbststudium = 105,
                            Verantwortlicher = "Prof. Dr. Wagner",
                            Voraussetzungen = "Keine.",
                            Lernziele = "Projektmanagement-Grundlagen verstehen, klassische PM-Methoden anwenden, Projektpläne erstellen.",
                            Lehrinhalte = "Projektdefinition, Phasenmodelle, Projektplanung (Gantt, Netzplan), Risikomanagement, Stakeholder-Management, MS Project.",
                            Literatur = "PMI: PMBOK Guide (2021), Kerzner: Project Management (2017)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "1.1",
                        ErstellDatum = DateTime.Now.AddMonths(-7),
                        Status = "Eingereicht",
                        Daten = new ModuleData
                        {
                            Titel = "Projektmanagement: Klassisch und Agil",
                            Modultypen = new List<string> { "Wahlpflichtmodul", "Pflichtmodul" },
                            Studiengang = "Wirtschaftsinformatik, Informatik, BWL",
                            Semester = new List<string> { "4", "5", "6" },
                            Pruefungsformen = new List<string> { "Klausur", "Projektarbeit (Belegarbeit)" },
                            Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                            Ects = 6,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 120,
                            Verantwortlicher = "Prof. Dr. Wagner, M.Sc. Klein",
                            Voraussetzungen = "Grundkenntnisse in Betriebswirtschaft oder Informatik.",
                            Lernziele = "Klassische und agile PM-Methoden anwenden, Scrum/Kanban implementieren, hybride Ansätze nutzen.",
                            Lehrinhalte = "Wasserfall, V-Modell, Scrum, Kanban, SAFe, Hybrides PM, Jira/Confluence, Team-Management, Retrospektiven.",
                            Literatur = "Schwaber/Sutherland: The Scrum Guide (2020), Anderson: Kanban (2010), PMI: Agile Practice Guide (2017)"
                        }
                    },
                    new ModuleVersion
                    {
                        VersionsNummer = "2.0",
                        ErstellDatum = DateTime.Now.AddDays(-7),
                        Status = "Entwurf",
                        Daten = new ModuleData
                        {
                            Titel = "Advanced Project & Product Management",
                            Modultypen = new List<string> { "Spezialisierungsmodul" },
                            Studiengang = "Master Wirtschaftsinformatik, MBA",
                            Semester = new List<string> { "2", "3" },
                            Pruefungsformen = new List<string> { "Projektarbeit (Belegarbeit)", "Portfolio", "Präsentation" },
                            Turnus = new List<string> { "Jährlich (WiSe)" },
                            Ects = 8,
                            WorkloadPraesenz = 60,
                            WorkloadSelbststudium = 180,
                            Verantwortlicher = "Prof. Dr. Wagner, Dr. Braun",
                            Voraussetzungen = "Projektmanagement Grundlagen, praktische Projekterfahrung.",
                            Lernziele = "Portfoliomanagement durchführen, OKRs definieren, Product Discovery betreiben, Lean Startup anwenden.",
                            Lehrinhalte = "Portfoliomanagement, OKRs, Product Management, Lean Startup, Design Thinking, A/B Testing, Data-Driven Decision Making, Change Management.",
                            Literatur = "Cagan: Inspired (2017), Ries: The Lean Startup (2011), Doerr: Measure What Matters (2018), Kotter: Leading Change (2012)"
                        }
                    }
                }
            };

            _nextModulId = 6;
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
                        Daten = daten,
                        HasComments = false,
                        Kommentare = null
                    }
                }
            };

            _modules[modulId] = neuesModul;
            return modulId;
        }

        /// <summary>Aktualisiert eine bestehende Version eines Moduls und synchronisiert den Modulnamen</summary>
        public static void UpdateModuleVersion(string modulId, string versionNummer, ModuleData daten)
        {
            var modul = GetModule(modulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);
            if (version != null)
            {
                version.Daten = daten;

                // Modulname synchronisieren mit dem Titel der neuesten Version
                var neuesteVersion = modul.Versionen.OrderByDescending(v => v.ErstellDatum).FirstOrDefault();
                if (neuesteVersion != null && neuesteVersion.Daten != null)
                {
                    modul.ModulName = neuesteVersion.Daten.Titel;
                }
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
                    Daten = daten,
                    HasComments = false,
                    Kommentare = null
                });
            }
        }

        /// <summary>Speichert Kommentare für eine Modulversion</summary>
        public static void SaveComments(string modulId, string versionNummer, List<FieldComment> fieldComments, string commenter)
        {
            var modul = GetModule(modulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);

            if (version != null)
            {
                version.HasComments = true;
                version.Kommentare = new CommentData
                {
                    FieldComments = fieldComments,
                    SubmittedDate = DateTime.Now,
                    SubmittedBy = commenter
                };
            }
        }

        /// <summary>Gibt Kommentare für eine Modulversion zurück</summary>
        public static CommentData GetComments(string modulId, string versionNummer)
        {
            var modul = GetModule(modulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);
            return version?.Kommentare;
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
