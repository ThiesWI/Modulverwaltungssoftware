using System;

namespace Modulverwaltungssoftware
{
    public class WorkflowController
    {
        public void starteGenehmigung(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 2 || aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // Set Status to "In Prüfung durch Koordination" & Sende Benachrichtigung an Koordination
            }
        }
        public void lehneAb(string modulID, string kommentarText, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 2 || aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // Set Status to "Abgelehnt"
            }
        }
        public void leiteWeiter(string modulID, int aktuellerBenutzer) 
        {
            if (aktuellerBenutzer != 2 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // Set Status to "In Prüfung durch Gremium" & Sende Benachrichtigung an Gremium
            }
        }
        public void lehneFinalAb(string modulID, string kommentarText, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // Set Status to "Abgelehnt"
            }
        }
        public void schliesseGenehmigungAb(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // Set Status to "Freigegeben"
            }
        }
        public void archiviereVersion(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException(); // ModulVersion auf "Archiviert" setzen
            }
        }
        public void getModulDetails(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer == 0)
            {
                throw new NotImplementedException(); // Zeige nur Module mit Status "Freigegeben"
            }
            else
            {
                throw new NotImplementedException(); // Zeige alle Module und Versionen
            }
        }
    }
}
