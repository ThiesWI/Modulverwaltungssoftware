using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Modulverwaltungssoftware
{
    internal class Kommentar
    {
        
        public int KommentarID { get; private set; }
        public string Text { get; private set; }
        public DateTime ErstellungsDatum { get; private set; }
        public int GehoertZuModulVersionID { get; set; }
        public int GehoertZuModulID { get; set; }
        public Kommentar(int kommentarID, string text, DateTime erstellungsDatum, int gehoertZuVersion, int gehoertZuModul)
        {
            this.KommentarID = kommentarID;
            this.Text = text;
            this.ErstellungsDatum = DateTime.Now;
            this.GehoertZuModulID = gehoertZuModul;
            this.GehoertZuModulVersionID = gehoertZuVersion;
        }
    }
}
