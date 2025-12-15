using System;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware
{
    public class WorkflowController
    {
        public void starteGenehmigung(int versionID, int modulID, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Dozent" || aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.InPruefungKoordination); // Set Status to "In Prüfung durch Koordination" & Sende Benachrichtigung an Koordination
                BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Koordination", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} zur Prüfung eingereicht.", versionID);
            }
        }
        public void lehneAb(int modulID, int versionID, string kommentarText, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Aenderungsbedarf);
                BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} abgelehnt. Kommentar: {kommentarText}", versionID);
                Kommentar.addKommentar(modulID, versionID, kommentarText);
            }
        }
        public void leiteWeiter(int modulID, int versionID, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.InPruefungGremium);
                BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Gremium", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} zur Prüfung durch das Gremium weitergeleitet.", versionID);
            }
        }
        public void lehneFinalAb(int modulID, int versionID, string kommentarText, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Aenderungsbedarf);
                BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} final abgelehnt. Kommentar: {kommentarText}", versionID);
                Kommentar.addKommentar(modulID, versionID, kommentarText);
            }
        }
        public void schliesseGenehmigungAb(int modulID, int versionID, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Freigegeben);
                BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} freigegeben.", versionID);
            }
        }
        public void archiviereVersion(int modulID, int versionID, string aktuellerBenutzer)
        {
            if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Archiviert);
            }
        }
        public Modul getModulDetails(int modulID)
        {
            using (var db = new Services.DatabaseContext())
            {
                var modul = db.Modul
                    .Where(m => m.ModulID == modulID)
                    .FirstOrDefault();

                if (modul == null)
                {
                    throw new KeyNotFoundException($"Modul mit ID {modulID} nicht gefunden.");
                }
                else
                {
                    return modul;
                }
            }
        }
    }
}