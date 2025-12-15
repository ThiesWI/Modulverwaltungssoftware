using System;
using System.ComponentModel.DataAnnotations;

namespace Modulverwaltungssoftware.Models
{
    public class Benachrichtigung
    {
        [Key]
        public int BenachrichtigungsID { get; set; }
        [Required]
        public string Empfaenger { get; set; }
        [Required]
        public string Sender { get; set; }
        [Required]
        public string Nachricht { get; set; }
        public DateTime GesendetAm { get; set; } = DateTime.Now;
        public bool Gelesen { get; set; } = false;
        public int? BetroffeneModulVersionID { get; set; }
    }
}
