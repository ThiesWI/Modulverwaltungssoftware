using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    public class PlausibilitaetsService
    {
        public static string pruefeWorkloadStandard(int stunden, int ects)
        {
            if (stunden / ects >= 28 && stunden / ects <= 32)
            {
                return "Der Workload entspricht nicht dem Standard von 30 Stunden pro ECTS.";
            }
            else if (stunden / 30 >= 2.5 && stunden / 30 <= 15)
            {
                return "Der Workload entspricht dem Standard.";
            }
            else if (stunden / 30 >= 15 && stunden / 30 <= 30)
            {
                return "Ungewöhnlich hoher Workload. Bitte stellen Sie sicher, dass dies beabsichtigt ist.";
            }
            else
            {
                return "Der Workload liegt außerhalb des üblichen Bereichs. Bitte prüfen Sie, ob ein Eingabefehler vorliegt.";
            }
        }
        internal static bool pruefeWorkloadStandardIntern(int stunden, int ects)
        {
            if (stunden / ects < 28 || stunden / ects > 32)
            {
                return false;
            }
            else if (stunden / 30 >= 2.5 && stunden / 30 <= 15)
            {
                return true;
            }
            else if (stunden / 30 >= 15 && stunden / 30 <= 30)
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
            if (pruefungsform == "PL" || pruefungsform == "SP" || pruefungsform == "SL")
            {
                pruefungsformTest = true;
            }
            else
            {
                pruefungsformTest = false;
            }
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
    }
}