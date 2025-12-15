using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

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
                db.SaveChanges();
            }
        }
    }
}
