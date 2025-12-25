using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class Kommentar
    {   
        public int KommentarID { get; set; }
        
        [StringLength(100)]
        public string FeldName { get; set; }  // z.B. "Titel", "ECTS", "Modultyp"
        
        [Required]
        public string Text { get; set; }
        
        public DateTime? ErstellungsDatum { get; set; }
        
        [StringLength(100)]
        public string Ersteller { get; set; }  // Wer hat kommentiert
        
        [Required]
        public int GehoertZuModulVersionID { get; set; }
        
        [Required]
        public int GehoertZuModulID { get; set; }

        // Legacy-Methode (für Rückwärtskompatibilität)
        public static void addKommentar(int modulID, int modulVersionID, string text)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false) // Berechtigungsabfrage
            { MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Kommentieren."); return; }
            using (var db = new Services.DatabaseContext())
            {
                var neuerKommentar = new Kommentar // Kommentar anlegen
                {
                    FeldName = null,
                    Text = text,
                    ErstellungsDatum = DateTime.Now,
                    Ersteller = "System",
                    GehoertZuModulID = modulID,
                    GehoertZuModulVersionID = modulVersionID
                };
                db.Kommentar.Add(neuerKommentar);

                var modulVersion = db.ModulVersion.Find(modulVersionID); // ModulVersion als kommentiert markieren, falls diese existiert
                if (modulVersion != null)
                {
                    modulVersion.hatKommentar = true;
                }

                db.SaveChanges();
            }
        }

        // Neue Methode: Speichere feldspezifische Kommentare und erstelle neue Version
        public static int addFeldKommentareMitNeuerVersion(int modulID, int altModulVersionID, List<FeldKommentar> feldKommentare, string ersteller)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false) { MessageBox.Show("Fehlende Berechtigungen zum Kommentieren"); return 0; }
            if (feldKommentare == null || feldKommentare.Count == 0)
                throw new ArgumentException("Es müssen mindestens ein Kommentar angegeben werden.", nameof(feldKommentare));

            using (var db = new Services.DatabaseContext())
            {
                // Alte Version laden
                var alteVersion = db.ModulVersion
                    .Include("Modul")
                    .FirstOrDefault(v => v.ModulVersionID == altModulVersionID && v.ModulId == modulID);

                if (alteVersion == null)
                    throw new InvalidOperationException("Die zu kommentierende Modulversion wurde nicht gefunden.");

                // ✅ Problem 4 Fix: Höchste Versionsnummer für dieses Modul finden
                var hoechsteVersionsnummer = db.ModulVersion
                    .Where(v => v.ModulId == modulID)
                    .Max(v => (int?)v.Versionsnummer) ?? 10;
                
                int neueVersionsnummer = hoechsteVersionsnummer + 1;
                
                // ✅ Problem 3 Fix: Prüfen ob Version bereits existiert
                if (db.ModulVersion.Any(v => v.ModulId == modulID && v.Versionsnummer == neueVersionsnummer))
                    throw new InvalidOperationException($"Version {neueVersionsnummer / 10.0:0.0} existiert bereits!");

                // Neue Version erstellen (Kopie der alten)
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
                db.SaveChanges(); // Speichern, um ModulVersionID zu erhalten

                // Kommentare zur neuen Version hinzufügen
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

        // Neue Methode: Speichere mehrere feldspezifische Kommentare
        public static void addFeldKommentare(int modulID, int modulVersionID, List<FeldKommentar> feldKommentare, string ersteller)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfKommentieren == false) { MessageBox.Show("Fehlende Berechtigungen zum Kommentieren."); return; }
            if (feldKommentare == null || feldKommentare.Count == 0)
            { MessageBox.Show("Es muss mindestens ein Kommentar angegeben werden.", nameof(feldKommentare)); return; }

            using (var db = new Services.DatabaseContext())
            {
                var erstellungsDatum = DateTime.Now;
                
                // Batch-Insert: Alle Kommentare erst sammeln, dann einmal SaveChanges
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

                // ModulVersion als kommentiert markieren
                var modulVersion = db.ModulVersion.Find(modulVersionID);
                if (modulVersion != null)
                {
                    modulVersion.hatKommentar = true;
                }

                db.SaveChanges();
            }
        }

        // Lade alle Kommentare für ein Modul
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

        // Lade alle Kommentare für eine spezifische Modulversion
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

                string currentUser = Benutzer.CurrentUser?.Name ?? "Unbekannt";  // ✅ FIX: Aktueller User!

                // Neue Version mit Kommentaren erstellen
                int neueVersionID = Kommentar.addFeldKommentareMitNeuerVersion(
                    modulId,
                    modulVersion.ModulVersionID,
                    feldKommentare,
                    currentUser  // ← Statt fest codiert!
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