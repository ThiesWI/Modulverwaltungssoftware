using System;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware.Services
{
    /// <summary>
    /// Service für Datenbank-Reparatur und -Wartung
    /// Aktuell nicht verwendet - Platzhalter für zukünftige Funktionalität
    /// </summary>
    public static class DatabaseRepairService
    {
        /// <summary>
        /// Repariert alle ModulVersionen ohne gültigen Ersteller
        /// Setzt Ersteller auf den aktuell angemeldeten Benutzer
        /// </summary>
        public static void RepairMissingErsteller()
        {
            try
            {
                string currentUser = Benutzer.CurrentUser?.Name;
                
                if (string.IsNullOrEmpty(currentUser))
                {
                    System.Diagnostics.Debug.WriteLine("? CurrentUser ist NULL - Reparatur nicht möglich");
                    return;
                }
                
                using (var db = new DatabaseContext())
                {
                    // Finde alle Versionen ohne Ersteller oder mit "Unbekannt"
                    var versionenOhneErsteller = db.ModulVersion
                        .Where(v => v.Ersteller == null || 
                                   v.Ersteller == "" || 
                                   v.Ersteller == "Unbekannt")
                        .ToList();
                    
                    if (versionenOhneErsteller.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("? Keine ModulVersionen ohne Ersteller gefunden");
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"?? REPARIERE {versionenOhneErsteller.Count} ModulVersionen ohne Ersteller...");
                    
                    foreach (var version in versionenOhneErsteller)
                    {
                        string alterErsteller = version.Ersteller ?? "(null)";
                        version.Ersteller = currentUser;
                        
                        System.Diagnostics.Debug.WriteLine($"   ? ModulVersionID {version.ModulVersionID}: '{alterErsteller}' ? '{currentUser}'");
                    }
                    
                    db.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine($"? REPARATUR ABGESCHLOSSEN: {versionenOhneErsteller.Count} Versionen aktualisiert");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Fehler bei Reparatur: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Zeigt Details zu allen ModulVersionen an (für Debugging)
        /// </summary>
        public static void DiagnoseAllErsteller()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var alleVersionen = db.ModulVersion
                        .Include("Modul")
                        .OrderBy(v => v.ModulId)
                        .ThenBy(v => v.Versionsnummer)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"\n?? DIAGNOSE: {alleVersionen.Count} ModulVersionen gefunden");
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????????????");
                    
                    foreach (var v in alleVersionen)
                    {
                        string modulName = v.Modul?.ModulnameDE ?? "Unbekannt";
                        string ersteller = string.IsNullOrEmpty(v.Ersteller) ? "(LEER)" : v.Ersteller;
                        string status = v.ModulStatus.ToString();
                        
                        string icon = string.IsNullOrEmpty(v.Ersteller) || v.Ersteller == "Unbekannt" ? "?" : "?";
                        
                        System.Diagnostics.Debug.WriteLine($"{icon} ModulID {v.ModulId} | Version {v.Versionsnummer / 10.0:0.0} | '{modulName}' | Ersteller: '{ersteller}' | Status: {status}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????????????\n");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Fehler bei Diagnose: {ex.Message}");
            }
        }
    }
}
