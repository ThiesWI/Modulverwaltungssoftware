using System.Collections.Generic;
using Modulverwaltungssoftware.Helpers;

namespace Modulverwaltungssoftware
{
    public class PlausibilitaetsService
    {
        private const int ECTS_MIN = 2;
        private const int ECTS_MAX = 30;
        private const int Workload_Max = 900; // 30 ECTS * 30 Stunden
        private const double HOURS_PER_ECTS_STANDARD_MIN = 28.0;
        private const double HOURS_PER_ECTS_STANDARD_MAX = 32.0;
        public static string pruefeWorkloadStandard(int stunden, int ects)
        {
            // Verhindere Division durch 0
            if (ects <= 0)
            {
                return "ECTS-Punkte müssen größer als 0 sein.";
            }
            
            double stundenProEcts = (double)stunden / ects;
            
            // ✅ KORRIGIERT: 28-32 Stunden pro ECTS IST der Standard (entspricht dem 30h-Standard)
            if (stundenProEcts >= HOURS_PER_ECTS_STANDARD_MIN && stundenProEcts <= HOURS_PER_ECTS_STANDARD_MAX)
            {
                return "Der Workload entspricht dem Standard."; // ✅ KORRIGIERT
            }
            // Plausibilitätsprüfung: 75-450 Stunden Gesamtworkload (2.5-15 ECTS)
            else if (stunden >= 60 && stunden <= 450)
            {
                return "Der Workload liegt im akzeptablen Bereich.";
            }
            // Warnung bei sehr hohem Workload (450-900 Stunden = 15-30 ECTS)
            else if (stunden >= 450 && stunden <= Workload_Max)
            {
                return "Ungewöhnlich hoher Workload. Bitte stellen Sie sicher, dass dies beabsichtigt ist.";
            }
            // Fehler bei unrealistischem Workload
            else
            {
                return "Der Workload liegt außerhalb des üblichen Bereichs. Bitte prüfen Sie, ob ein Eingabefehler vorliegt.";
            }
        }
        internal static bool pruefeWorkloadStandardIntern(int stunden, int ects)
        {
            if (ects <= 0) {
                return false;
            }
            double stundenProEcts = (double)stunden / (double)ects;
            if (ects < ECTS_MIN || ects > ECTS_MAX)
            {
                return false;
            }
            if (stunden <= 0 || stunden > Workload_Max)
            {
                return false;
            }
            if (stundenProEcts >= HOURS_PER_ECTS_STANDARD_MIN && stundenProEcts <= HOURS_PER_ECTS_STANDARD_MAX)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string pruefeForm(ModulVersion v)
        {
            string modultyp = v.Modul.Modultyp.ToString();
            int semester = v.Modul.EmpfohlenesSemester;
            string pruefungsform = v.Pruefungsform;
            string turnus = v.Modul.Turnus.ToString();
            int ects = v.EctsPunkte;
            int workloadPraesenz = v.WorkloadPraesenz;
            int workloadSelbststudium = v.WorkloadSelbststudium;
            string verantwortlicher = v.Ersteller;
            List<string> lernziele = v.Lernergebnisse;
            List<string> lehrinhalte = v.Inhaltsgliederung;
            int version = v.Versionsnummer;
            int workloadGesamt = workloadPraesenz + workloadSelbststudium;
            bool workloadTest = pruefeWorkloadStandardIntern(workloadGesamt, ects);
            bool typTest;
            bool semesterTest;
            bool pruefungsformTest;
            bool turnusTest;
            bool verantwortlicherTest;
            bool lehrinhaltTest;
            bool lernzieleTest;
            if (modultyp == "Grundlagen" || modultyp == "Wahlpflicht")
            {
                typTest = true;
            }
            else
            {
                typTest = false;
            }
            if (semester >= 1 && semester <= 8)
            {
                semesterTest = true;
            }
            else
            {
                semesterTest = false;
            }

            // Akzeptiere alle Prüfungsformen, die in der EditingView angeboten werden
            var gueltigePruefungsformen = new List<string>
            {
                "PL", "SP", "SL", "Klausur", "Mündliche Prüfung", "Projektarbeit", "Hausarbeit", "Präsentation", "Portfolio", "Referat"
            };
            pruefungsformTest = !string.IsNullOrWhiteSpace(pruefungsform) && gueltigePruefungsformen.Contains(pruefungsform);

            if (turnus == "JedesSemester" || turnus == "NurWintersemester" || turnus == "NurSommersemester")
            {
                turnusTest = true;
            }
            else
            {
                turnusTest = false;
            }
            if (!string.IsNullOrEmpty(verantwortlicher))
            {
                verantwortlicherTest = true;
            }
            else
            {
                verantwortlicherTest = false;
            }
            if (lernziele != null && lernziele.Count > 0)
            {
                lernzieleTest = true;
            }
            else
            {
                lernzieleTest = false;
            }
            if (lehrinhalte != null && lehrinhalte.Count > 0)
            {
                lehrinhaltTest = true;
            }
            else
            {
                lehrinhaltTest = false;
            }
            string fehler = fehlerListe(workloadTest, typTest, semesterTest, pruefungsformTest, turnusTest, verantwortlicherTest, lehrinhaltTest, lernzieleTest);

            return fehler;
        }
        private static string fehlerListe(bool workload, bool typ, bool semester, bool pruefung, bool turnus, bool verantwortlicher, bool lehrinhalte, bool lernziele)
        {
            string fehlerMeldungen = "Fehler gefunden in folgenden Bereichen:\n";
            if (workload && typ && semester && pruefung && turnus && verantwortlicher && lehrinhalte && lernziele == true) 
            {
                return "Keine Fehler gefunden.";
            }
            else
            if (!workload)
            {
                fehlerMeldungen += "Workload entspricht nicht dem Standard.\n";
            }
            if (!typ)
            {
                fehlerMeldungen += "Ungültiger Modultyp.\n";
            }
            if (!semester)
            {
                fehlerMeldungen += "Empfohlenes Semester außerhalb des gültigen Bereichs.\n";
            }
            if (!pruefung)
            {
                fehlerMeldungen += "Ungültige Prüfungsform.\n";
            }
            if (!turnus)
            {
                fehlerMeldungen += "Ungültiger Turnus.\n";
            }
            if (!verantwortlicher)
            {
                fehlerMeldungen += "Kein Verantwortlicher angegeben.\n";
            }
            if (!lehrinhalte)
            {
                fehlerMeldungen += "Keine Lehrinhalte angegeben.\n";
            }
            if (!lernziele)
            {
                fehlerMeldungen += "Keine Lernziele angegeben.\n";
            }

            return fehlerMeldungen;
        }
        /// <summary>
        /// ✅ NEU: Validiert eine ModulVersion und gibt detaillierte Feldfehler zurück
        /// </summary>
        public static ValidationResult ValidateModulVersion(ModulVersion v)
        {
            var result = new ValidationResult();
            
            if (v == null || v.Modul == null)
            {
                result.IsValid = false;
                result.GlobalMessage = "Modulversion oder Modul-Daten fehlen.";
                return result;
            }

            // Modultyp prüfen
            string modultyp = v.Modul.Modultyp.ToString();
            if (modultyp != "Grundlagen" && modultyp != "Wahlpflicht")
            {
                result.AddError("Modultyp", "Ungültiger Modultyp. Nur 'Grundlagen' oder 'Wahlpflicht' erlaubt.");
            }

            // Semester prüfen (1-8)
            int semester = v.Modul.EmpfohlenesSemester;
            if (semester < 1 || semester > 8)
            {
                result.AddError("Semester", "Empfohlenes Semester muss zwischen 1 und 8 liegen.");
            }

            // Prüfungsform prüfen
            var gueltigePruefungsformen = new List<string>
            {
                "PL", "SP", "SL", "Klausur", "Mündliche Prüfung", "Projektarbeit", 
                "Hausarbeit", "Präsentation", "Portfolio", "Referat"
            };
            if (string.IsNullOrWhiteSpace(v.Pruefungsform) || !gueltigePruefungsformen.Contains(v.Pruefungsform))
            {
                result.AddError("Pruefungsform", "Ungültige Prüfungsform.");
            }

            // Turnus prüfen
            string turnus = v.Modul.Turnus.ToString();
            if (turnus != "JedesSemester" && turnus != "NurWintersemester" && turnus != "NurSommersemester")
            {
                result.AddError("Turnus", "Ungültiger Turnus.");
            }

            // Verantwortlicher prüfen
            if (string.IsNullOrEmpty(v.Ersteller))
            {
                result.AddError("Verantwortlicher", "Kein Verantwortlicher angegeben.");
            }

            // Lernziele prüfen
            if (v.Lernergebnisse == null || v.Lernergebnisse.Count == 0)
            {
                result.AddError("Lernziele", "Keine Lernziele angegeben.");
            }

            // Lehrinhalte prüfen
            if (v.Inhaltsgliederung == null || v.Inhaltsgliederung.Count == 0)
            {
                result.AddError("Lehrinhalte", "Keine Lehrinhalte angegeben.");
            }

            // Workload prüfen
            int workloadGesamt = v.WorkloadPraesenz + v.WorkloadSelbststudium;
            int ects = v.EctsPunkte;
            
            if (ects <= 0)
            {
                result.AddError("ECTS", "ECTS-Punkte müssen größer als 0 sein.");
            }
            else if (!pruefeWorkloadStandardIntern(workloadGesamt, ects))
            {
                result.AddError("Workload", "Workload entspricht nicht dem Standard (28-32 Stunden/ECTS).");
            }

            return result;
        }
    }
}