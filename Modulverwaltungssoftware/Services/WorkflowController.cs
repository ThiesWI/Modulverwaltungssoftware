using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class WorkflowController
    {
        public static void starteGenehmigung(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungKoordination); // Set Status to "In Prüfung durch Koordination" & Sende Benachrichtigung an Koordination
                    BenachrichtigungsService.SendeBenachrichtigung("Koordination", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} zur Prüfung eingereicht.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Modul zur Prüfung einreichen für Dozent und Admin
        public static void lehneAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung ("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} abgelehnt.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Ablehnen für Koordination + Admin
        public static void leiteWeiter(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Koordination" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungGremium);
                    BenachrichtigungsService.SendeBenachrichtigung("Gremium", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} zur Prüfung durch das Gremium weitergeleitet.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Modul-Entwurf an Gremium weiterleiten (Koordination + Admin)
        public static void lehneFinalAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} final abgelehnt.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Ablehnen für Gremium + Admin
        public static void schliesseGenehmigungAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Freigegeben);
                    BenachrichtigungsService.SendeBenachrichtigung("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} freigegeben.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Gremium + Admin only -> Modul freigeben
        public static void archiviereVersion(int modulID, int versionID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfStatusAendern == false)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Archiviert);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Status auf Archiviert setzen
        public static Modul getModulDetails(int modulID) // Modul aus DB abrufen
        {try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modul = db.Modul
                        .Where(m => m.ModulID == modulID)
                        .FirstOrDefault();

                    if (modul == null)
                    {
                        MessageBox.Show($"Modul mit ID {modulID} nicht gefunden.");
                        return null;
                    }
                    else
                    {
                        return modul;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");;
                return null;
            }
        }
    }
}