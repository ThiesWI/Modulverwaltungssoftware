using System;

namespace Modulverwaltungssoftware
{
    internal class Kommentar
    {
        public int kommentarID { get; private set; }
        public string text { get; private set; }
        public DateTime erstellungsDatum { get; private set; }
    }
}
