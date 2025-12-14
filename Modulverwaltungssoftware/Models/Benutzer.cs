namespace Modulverwaltungssoftware
{
    public class Benutzer
    {
        public string BenutzerID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Passwort { get; set; }
        public Rolle Rolle { get; set; }
        public int AktuellerBenutzer { get; set; }
    }
}
