using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class Kommentar
    {
        public int KommentarID { get; set; }

        [StringLength(100)]
        public string FeldName { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime? ErstellungsDatum { get; set; }

        [StringLength(100)]
        public string Ersteller { get; set; }

        [Required]
        public int GehoertZuModulVersionID { get; set; }

        [Required]
        public int GehoertZuModulID { get; set; }

        /// <summary>
        /// Fügt einen einfachen Kommentar zur Datenbank hinzu (Legacy-Methode).
        /// </summary>
        public static void addKommentar(int modulID, int modulVersionID, string text)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false)
            { MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Kommentieren."); return; }
            using (var db = new Services.DatabaseContext())
            {
                var neuerKommentar = new Kommentar
                {
                    FeldName = null,
                    Text = text,
                    ErstellungsDatum = DateTime.Now,
                    Ersteller = "System",
                    GehoertZuModulID = modulID,
                    GehoertZuModulVersionID = modulVersionID
                };
                db.Kommentar.Add(neuerKommentar);

                var modulVersion = db.ModulVersion.Find(modulVersionID);
                if (modulVersion != null)
                {
                    modulVersion.hatKommentar = true;
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Speichert feldspezifische Kommentare und erstellt eine neue Modulversion.
        /// </summary>
        public static int addFeldKommentareMitNeuerVersion(int modulID, int altModulVersionID, List<FeldKommentar> feldKommentare, string ersteller)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false) { MessageBox.Show("Fehlende Berechtigungen zum Kommentieren"); return 0; }
            if (feldKommentare == null || feldKommentare.Count == 0)
                throw new ArgumentException("Es müssen mindestens ein Kommentar angegeben werden.", nameof(feldKommentare));

            using (var db = new Services.DatabaseContext())
            {
                var alteVersion = db.ModulVersion
                    .Include("Modul")
                    .FirstOrDefault(v => v.ModulVersionID == altModulVersionID && v.ModulId == modulID);

                if (alteVersion == null)
                    throw new InvalidOperationException("Die zu kommentierende Modulversion wurde nicht gefunden.");

                var hoechsteVersionsnummer = db.ModulVersion
                    .Where(v => v.ModulId == modulID)
                    .Max(v => (int?)v.Versionsnummer) ?? 10;

                int neueVersionsnummer = hoechsteVersionsnummer + 1;

                if (db.ModulVersion.Any(v => v.ModulId == modulID && v.Versionsnummer == neueVersionsnummer))
                    throw new InvalidOperationException($"Version {neueVersionsnummer / 10.0:0.0} existiert bereits!");

                var neueVersion = new ModulVersion
                {
                    ModulId = alteVersion.ModulId,
                    Versionsnummer = neueVersionsnummer,
                    GueltigAbSemester = alteVersion.GueltigAbSemester,
                    ModulStatus = ModulVersion.Status.Aenderungsbedarf,
                    LetzteAenderung = DateTime.Now,
                    WorkloadPraesenz = alteVersion.WorkloadPraesenz,
                    WorkloadSelbststudium = alteVersion.WorkloadSelbststudium,
                    EctsPunkte = alteVersion.EctsPunkte,
                    Pruefungsform = alteVersion.Pruefungsform,
                    Literatur = alteVersion.Literatur != null ? new List<string>(alteVersion.Literatur) : new List<string>(),
                    Ersteller = alteVersion.Ersteller,
                    Lernergebnisse = alteVersion.Lernergebnisse != null ? new List<string>(alteVersion.Lernergebnisse) : new List<string>(),
                    Inhaltsgliederung = alteVersion.Inhaltsgliederung != null ? new List<string>(alteVersion.Inhaltsgliederung) : new List<string>(),
                    hatKommentar = true
                };

                db.ModulVersion.Add(neueVersion);
                db.SaveChanges();

                var erstellungsDatum = DateTime.Now;
                foreach (var feldKommentar in feldKommentare)
                {
                    var neuerKommentar = new Kommentar
                    {
                        FeldName = feldKommentar.FeldName,
                        Text = feldKommentar.Text,
                        ErstellungsDatum = erstellungsDatum,
                        Ersteller = ersteller,
                        GehoertZuModulID = modulID,
                        GehoertZuModulVersionID = neueVersion.ModulVersionID
                    };
                    db.Kommentar.Add(neuerKommentar);
                }

                db.SaveChanges();
                return neueVersion.ModulVersionID;
            }
        }

        /// <summary>
        /// Speichert mehrere feldspezifische Kommentare zu einer Modulversion.
        /// </summary>
        public static void addFeldKommentare(int modulID, int modulVersionID, List<FeldKommentar> feldKommentare, string ersteller)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false) { MessageBox.Show("Fehlende Berechtigungen zum Kommentieren."); return; }
            if (feldKommentare == null || feldKommentare.Count == 0)
            { MessageBox.Show("Es muss mindestens ein Kommentar angegeben werden.", nameof(feldKommentare)); return; }

            using (var db = new Services.DatabaseContext())
            {
                var erstellungsDatum = DateTime.Now;

                foreach (var feldKommentar in feldKommentare)
                {
                    var neuerKommentar = new Kommentar
                    {
                        FeldName = feldKommentar.FeldName,
                        Text = feldKommentar.Text,
                        ErstellungsDatum = erstellungsDatum,
                        Ersteller = ersteller,
                        GehoertZuModulID = modulID,
                        GehoertZuModulVersionID = modulVersionID
                    };
                    db.Kommentar.Add(neuerKommentar);
                }

                var modulVersion = db.ModulVersion.Find(modulVersionID);
                if (modulVersion != null)
                {
                    modulVersion.hatKommentar = true;
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Lädt alle Kommentare für ein Modul.
        /// </summary>
        public static List<Kommentar> getKommentare(int modulID)
        {
            using (var db = new Services.DatabaseContext())
            {
                return db.Kommentar
                    .Where(k => k.GehoertZuModulID == modulID)
                    .OrderByDescending(k => k.ErstellungsDatum)
                    .ToList();
            }
        }

        /// <summary>
        /// Lädt alle Kommentare für eine spezifische Modulversion.
        /// </summary>
        public static List<Kommentar> getKommentareFuerVersion(int modulID, int modulVersionID)
        {
            using (var db = new Services.DatabaseContext())
            {
                return db.Kommentar
                    .Where(k => k.GehoertZuModulID == modulID && k.GehoertZuModulVersionID == modulVersionID)
                    .OrderBy(k => k.FeldName)
                    .ThenBy(k => k.ErstellungsDatum)
                    .ToList();
            }
        }
        public static void SaveCommentsToDatabase(List<Kommentar.FeldKommentar> feldKommentare, int modulId)
        {

            using (var db = new Services.DatabaseContext())
            {
                var modulVersion = db.ModulVersion
                    .FirstOrDefault(v => v.ModulId == modulId);

                if (modulVersion == null)
                    throw new InvalidOperationException("Modulversion nicht gefunden.");

                string currentUser = Benutzer.CurrentUser?.Name ?? "Unbekannt";

                int neueVersionID = Kommentar.addFeldKommentareMitNeuerVersion(
                    modulId,
                    modulVersion.ModulVersionID,
                    feldKommentare,
                    currentUser
                );
            }
        }

        // Hilfsklasse für feldspezifische Kommentare
        public class FeldKommentar
        {
            public string FeldName { get; set; }
            public string Text { get; set; }
        }
    }
}