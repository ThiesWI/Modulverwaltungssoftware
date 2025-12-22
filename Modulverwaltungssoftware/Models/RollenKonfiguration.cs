using System;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware.Models
{
    public static class RollenKonfiguration
    {
        public static readonly List<Rolle> Rollen = new List<Rolle>
        {
            new Rolle
            {
                RollenName = "Gast",
                DarfBearbeiten = false,
                DarfFreigeben = false,
                DarfKommentieren = false,
                DarfStatusAendern = false
            },
            new Rolle
            {
                RollenName = "Dozent",
                DarfBearbeiten = true,
                DarfFreigeben = false,
                DarfKommentieren = false,
                DarfStatusAendern = false
            },
            new Rolle
            {
                RollenName = "Koordination",
                DarfBearbeiten = false,
                DarfFreigeben = false,
                DarfKommentieren = true,
                DarfStatusAendern = true
            },
            new Rolle
            {
                RollenName = "Gremium",
                DarfBearbeiten = false,
                DarfFreigeben = true,
                DarfKommentieren = true,
                DarfStatusAendern = true
            },
            new Rolle
            {
                RollenName = "Admin",
                DarfBearbeiten = true,
                DarfFreigeben = true,
                DarfKommentieren = true,
                DarfStatusAendern = true
            }
        }; // Liste aller vorhandenen Rollen und Berechtigungen
        public static Rolle GetRolleByName(string rollenName)
        {
            return Rollen.FirstOrDefault(r => r.RollenName.Equals(rollenName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
