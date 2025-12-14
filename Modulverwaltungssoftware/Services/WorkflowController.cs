using System;

namespace Modulverwaltungssoftware
{
    internal class WorkflowController
    {
        public void starteGenehmigung(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer != 2 || aktuellerBenutzer != 3 || aktuellerBenutzer != 99)
            {
                throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
            }
            else
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }
        public void getModulDetails(string modulID, int aktuellerBenutzer)
        {
            if (aktuellerBenutzer == 0)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
