using Modulverwaltungssoftware.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modulverwaltungssoftware
{
    public class Benutzer
    {
        public int BenutzerID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Passwort { get; set; }
        public string RollenName { get; set; }
        [NotMapped]
        public Rolle AktuelleRolle
        {
            get { return RollenKonfiguration.GetRolleByName(this.RollenName); }
            set { this.RollenName = value?.RollenName; }
        }
        public int AktuellerBenutzer { get; set; }
        public Benutzer()
        {
            RollenName = "Gast";
        }
    }
}
