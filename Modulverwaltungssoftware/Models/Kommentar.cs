using System;
using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    internal class Kommentar
    {
        
        public int KommentarID { get; private set; }
        public string Text { get; private set; }
        public DateTime ErstellungsDatum { get; private set; }
        public Kommentar(int kommentarID, string text, DateTime erstellungsDatum)
        {
            this.KommentarID = kommentarID;
            this.Text = text;
            this.ErstellungsDatum = DateTime.Now;
        }
    }
}
