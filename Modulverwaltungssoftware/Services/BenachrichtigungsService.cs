
using Modulverwaltungssoftware.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class BenachrichtigungsService
    {
        public static void SendeBenachrichtigung(string empfaenger, string nachricht, int betroffeneModulVersionID = 0)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    if (betroffeneModulVersionID == 0)
                    {
                        var benachrichtigung = new Benachrichtigung
                        {
                            Sender = Benutzer.CurrentUser.Name,
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
                            Sender = Benutzer.CurrentUser.Name,
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Benachrichtigung "senden" (in DB speichern)
        public static List<Benachrichtigung> EmpfangeBenachrichtigung()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var benachrichtigungen = db.Benachrichtigung
                        .Where(b => b.Empfaenger == Benutzer.CurrentUser.Name && b.Gelesen == false)
                        .OrderByDescending(b => b.GesendetAm);
                    return benachrichtigungen.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // Alle Benachrichtigungen mit istGelesen == false für aktuellen Benuter aus DB abfragen
        public static void MarkiereAlsGelesen()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var ungelesene = db.Benachrichtigung
                        .Where(b => b.Empfaenger == Benutzer.CurrentUser.Name && b.Gelesen == false)
                        .ToList();

                    foreach (var n in ungelesene)
                    {
                        n.Gelesen = true;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        }   // Alle Benachrichtigungen für aktuellen Benutzer als gelesen markieren
    }
}
