using System;
using System.ComponentModel.DataAnnotations;

namespace Modulverwaltungssoftware
{
    public class Studiengang
    {
        public int StudiengangID { get; set; }
        [Required]
        [StringLength(20)]
        public string Kuerzel { get; set; }
        [Required]
        [StringLength(200)]
        public string NameDE { get; set; }
        public string NameEN { get; set; }
        public int GesamtECTS { get; set; }
        public DateTime GueltigAb { get; set; }
        public string Verantwortlicher { get; set; }
        public void getAktuelleModule() 
        {
            // Get alle Module mit dem Status "Freigegeben"
        }
        public void addModul(Modul modul)
        {
            // Instanz von Modul und ModulVersion in DB hinzufügen
        }
        public void removeModul(Modul modul)
        {
            // Modul und alle zugehörigen ModulVersionen aus DB entfernen -> Override für Löschen von Versionen erstellen
        }
        public bool istKomplett()
        {
            throw new NotImplementedException(); // Redundant, PlausibilitätsService erledigt den Job bei der Eingabe im UI, DB so konfigurieren, dass relevante Daten nicht null sein dürfen!
        }
    }
}
