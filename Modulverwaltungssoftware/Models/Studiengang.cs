using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Documents;

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
        public void addModul(Modul modul)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    if (modul == null)
                    {
                        throw new ArgumentNullException(nameof(modul));
                    }
                    if (modul.GueltigAb == default)
                    {
                        modul.GueltigAb = DateTime.Now;
                    }
                    db.Modul.Add(modul);
                    db.SaveChanges();
                }
            }
            catch (Exception ex) { throw; }
            }
        public void removeModul(int modulID)
        {
            using (var db = new Services.DatabaseContext()) 
            { 
                var modul = db.Modul.Find(modulID);
                if (modul == null) 
                {
                    throw new KeyNotFoundException($"Modul mit ID {nameof(modul)} nicht gefunden.");
                }
                db.Modul.Remove(modul);
                db.SaveChanges();
            }
        }
        public void removeModulVersion(int modulID, int modulVersion) 
        {
            using (var db = new Services.DatabaseContext())
            {
                var version = db.ModulVersion.Find(modulID, modulVersion);
                if (version == null)
                {
                    throw new KeyNotFoundException($"ModulVersion mit ID {modulVersion} und/oder ModulID {modulID} nicht gefunden.");
                }
                db.ModulVersion.Remove(version);
                db.SaveChanges();
            }
        }
    }
}
