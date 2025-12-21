using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation; // Für Validierungsfehler
using System.Data.Entity.Infrastructure; // Für Concurrency (Gleichzeitigkeit)
using System.Data.Entity.Core;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq; // Für DB-Verbindungsfehler

namespace Modulverwaltungssoftware
{
    public class Kommentar
    {   
        public int KommentarID { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime? ErstellungsDatum { get; set; }
        [Required]
        public int GehoertZuModulVersionID { get; set; }
        [Required]
        public int GehoertZuModulID { get; set; }
        public static void addKommentar(int modulID, int modulVersionID, string text)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    Kommentar neuerKommentar = new Kommentar
                    {
                        Text = text,
                        ErstellungsDatum = DateTime.Now,
                        GehoertZuModulID = modulID,
                        GehoertZuModulVersionID = modulVersionID
                    };
                    db.Kommentar.Add(neuerKommentar);

                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.ModulVersionID == modulVersionID);
                    if (modulVersion != null)
                    {
                        modulVersion.hatKommentar = true;
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex) { throw; }
        }
        public static List<Kommentar> getKommentare(int modulID)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var kommentare = db.Kommentar
                        .Where(k => k.GehoertZuModulID == modulID)
                        .OrderByDescending(k => k.ErstellungsDatum)
                        .ToList();
                    if (kommentare == null || kommentare.Count == 0)
                        return null;
                    else
                        return kommentare;
                }
            }
            catch (Exception ex) { throw; }
        }
    }
}