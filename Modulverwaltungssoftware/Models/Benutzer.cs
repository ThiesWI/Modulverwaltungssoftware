using Modulverwaltungssoftware.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modulverwaltungssoftware
{
    public class Benutzer
    {
        public int BenutzerID { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(255)]
        public string Email { get; set; }
        [Required]
        [StringLength(55)]
        public string Passwort { get; set; }
        [Required]
        [StringLength(50)]
        public string RollenName { get; set; }
        [NotMapped]
        public Rolle AktuelleRolle
        {
            get { return RollenKonfiguration.GetRolleByName(this.RollenName); }
            set { this.RollenName = value?.RollenName; }
        }
        [NotMapped]
        public int AktuellerBenutzer { get; set; }
        public Benutzer()
        {
            RollenName = "Gast";
        }
        [NotMapped]
        public static Benutzer CurrentUser { get; set; }
    }
}
