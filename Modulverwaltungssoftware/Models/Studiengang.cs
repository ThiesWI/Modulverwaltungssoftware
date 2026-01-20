using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class Studiengang
    {
        public int StudiengangID { get; set; }
        [Required]
        [StringLength(20)]
        public string Kuerzel { get; set; }
        [Required]
        [StringLength(200)]
        public string NameDE { get; set; }
        [StringLength(200)]
        public string NameEN { get; set; }
        public int GesamtECTS { get; set; }
        public DateTime GueltigAb { get; set; }
        public string Verantwortlicher { get; set; }

        /// <summary>
        /// Ruft alle Module ab, die mindestens eine freigegebene Version haben.
        /// </summary>
        public List<Modul> getAktuelleModule()
        {
            using (var db = new Services.DatabaseContext())
            {
                var query = db.Modul
                    .Where(modul => modul.ModulVersionen.Any(modulVersion =>
                        modulVersion.GueltigAbSemester != null &&
                        modulVersion.ModulStatus == ModulVersion.Status.Freigegeben))
                    .OrderBy(m => m.ModulnameDE);
                return query.ToList();
            }
        }

        /// <summary>
        /// Fügt ein neues Modul zur Datenbank hinzu.
        /// </summary>
        public static int addModul(Modul modul)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false)
            {
                MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Anlegen von Modulen.");
                return -1;
            }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    if (modul == null)
                    {
                        MessageBox.Show($"Modul hat keinen Inhalt: {nameof(modul)}");
                        return -1;
                    }
                    if (modul.GueltigAb == default)
                    {
                        modul.GueltigAb = DateTime.Now;
                    }
                    db.Modul.Add(modul);
                    db.SaveChanges();
                    return modul.ModulID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Löscht ein Modul und alle zugehörigen Versionen.
        /// </summary>
        public void removeModul(int modulID)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false && Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false)
            {
                MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Löschen von Modulen.");
                return;
            }
            using (var db = new Services.DatabaseContext())
            {
                var modul = db.Modul.Find(modulID);
                if (modul == null)
                {
                    MessageBox.Show("Modul existiert nicht!");
                }
                db.Modul.Remove(modul);
                db.ModulVersion.RemoveRange(db.ModulVersion.Where(mv => mv.ModulId == modulID));
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Löscht eine spezifische Version eines Moduls.
        /// </summary>
        public void removeModulVersion(int modulID, int versionsnummer)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false && Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Löschen von Modulen.");
                    return;
                }
                using (var db = new Services.DatabaseContext())
                {
                    var version = db.ModulVersion
                        .FirstOrDefault(v => v.ModulId == modulID && v.Versionsnummer == versionsnummer);
                    if (version == null)
                    {
                        MessageBox.Show($"ModulVersion mit ID {versionsnummer} und/oder ModulID {modulID} nicht gefunden.");
                        return;
                    }
                    db.ModulVersion.Remove(version);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        }
    }
}

