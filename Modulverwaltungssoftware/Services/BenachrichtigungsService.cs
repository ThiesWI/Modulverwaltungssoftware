
using Modulverwaltungssoftware.Models;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware
{
    public class BenachrichtigungsService
    {
        public static void SendeBenachrichtigung(string benutzer, string empfaenger, string nachricht, int betroffeneModulVersionID = 0)
        {
            using (var db = new Services.DatabaseContext()) 
            {
                if (betroffeneModulVersionID == 0)
                {
                    var benachrichtigung = new Benachrichtigung
                    {
                        Sender = benutzer,
                        Empfaenger = empfaenger,
                        Nachricht = nachricht,
                        GesendetAm = System.DateTime.Now,
                        Gelesen = false
                    };
                    db.Benachrichtigung.Add(benachrichtigung);
                }
                else
                {
                    var benachrichtigung = new Benachrichtigung
                    {
                        BetroffeneModulVersionID = betroffeneModulVersionID,
                        Sender = benutzer,
                        Empfaenger = empfaenger,
                        Nachricht = nachricht,
                        GesendetAm = System.DateTime.Now,
                        Gelesen = false
                    };
                    db.Benachrichtigung.Add(benachrichtigung);
                }
                db.SaveChanges();
            }
        }
        public static List<Benachrichtigung> EmpfangeBenachrichtigung(string aktuellerBenutzer)
        {
            using (var db = new Services.DatabaseContext())
            {
                var benachrichtigungen = db.Benachrichtigung
                    .Where(b => b.Empfaenger == aktuellerBenutzer && b.Gelesen == false)
                    .OrderByDescending(b => b.GesendetAm);
                return benachrichtigungen.ToList();
            }
        }
        public static void MarkiereAlsGelesen(string aktuellerBenutzer)
        {
            using (var db = new Services.DatabaseContext())
            {
                var ungelesene = db.Benachrichtigung
                    .Where(b => b.Empfaenger == aktuellerBenutzer && b.Gelesen == false)
                    .ToList();

                foreach (var n in ungelesene)
                {
                    n.Gelesen = true;
                }
                db.SaveChanges();
            }
        }
    }
}
