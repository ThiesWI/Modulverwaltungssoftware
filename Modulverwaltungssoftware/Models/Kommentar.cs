using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Modulverwaltungssoftware
{
    public class Kommentar
    {
        
        public int KommentarID { get; private set; }
        public string Text { get; private set; }
        public DateTime ErstellungsDatum { get; private set; }
        public int GehoertZuModulVersionID { get; set; }
        public int GehoertZuModulID { get; set; }
    }
}
