using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class WorkflowController
    {
        public static void starteGenehmigung(int versionsnummer, int modulID)
        {
            try
            {
                // ✅ FIX: Prüfe ob User der Ersteller ist ODER Admin, nicht DarfBearbeiten
                string currentUser = Benutzer.CurrentUser?.Name;
                string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
                
                System.Diagnostics.Debug.WriteLine($"🔍 starteGenehmigung GESTARTET:");
                System.Diagnostics.Debug.WriteLine($"   CurrentUser Name: '{currentUser}'");
                System.Diagnostics.Debug.WriteLine($"   CurrentUser Rolle: '{rolle}'");
                System.Diagnostics.Debug.WriteLine($"   Versionsnummer: {versionsnummer}");
                System.Diagnostics.Debug.WriteLine($"   ModulID: {modulID}");
                
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion
                        .Include("Modul")
                        .FirstOrDefault(v => v.ModulId == modulID && v.Versionsnummer == versionsnummer);
                    
                    if (modulVersion == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ MODULVERSION NICHT GEFUNDEN! (ModulID={modulID}, Versionsnummer={versionsnummer})");
                        MessageBox.Show("Fehler: Modulversion nicht gefunden.");
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ ModulVersion gefunden:");
                    System.Diagnostics.Debug.WriteLine($"   Modul: '{modulVersion.Modul.ModulnameDE}'");
                    System.Diagnostics.Debug.WriteLine($"   Ersteller: '{modulVersion.Ersteller}'");
                    System.Diagnostics.Debug.WriteLine($"   Ersteller Länge: {modulVersion.Ersteller?.Length ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"   CurrentUser Länge: {currentUser?.Length ?? 0}");
                    
                    // ✅ BERECHTIGUNGSPRÜFUNG: Ersteller ODER Admin
                    bool istErsteller = !string.IsNullOrEmpty(modulVersion.Ersteller) &&
                                       !string.IsNullOrEmpty(currentUser) &&
                                       modulVersion.Ersteller.Trim().Equals(currentUser.Trim(), StringComparison.OrdinalIgnoreCase);
                    
                    bool istAdmin = rolle == "Admin";
                    
                    System.Diagnostics.Debug.WriteLine($"📊 BERECHTIGUNGSPRÜFUNG:");
                    System.Diagnostics.Debug.WriteLine($"   istErsteller: {istErsteller}");
                    System.Diagnostics.Debug.WriteLine($"   istAdmin: {istAdmin}");
                    System.Diagnostics.Debug.WriteLine($"   Ersteller (trimmed): '{modulVersion.Ersteller?.Trim()}'");
                    System.Diagnostics.Debug.WriteLine($"   CurrentUser (trimmed): '{currentUser?.Trim()}'");
                    
                    if (!istErsteller && !istAdmin)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ BERECHTIGUNG VERWEIGERT!");
                        MessageBox.Show($"Sie können dieses Modul nicht einreichen.\n\nNur der Ersteller '{modulVersion.Ersteller}' oder ein Admin können dieses Modul zur Genehmigung freigeben.");
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ BERECHTIGUNG ERTEILT (Ersteller: {istErsteller}, Admin: {istAdmin})");
                    
                    // Status auf "InPruefungKoordination" setzen
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungKoordination);
                    
                    // Benachrichtigung an alle Koordinatoren senden
                    BenachrichtigungsService.SendeBenachrichtigung(
                        "Koordination",
                        $"{currentUser} hat das Modul '{modulVersion.Modul.ModulnameDE}' (Version {versionsnummer / 10.0:0.0}) zur Prüfung eingereicht.",
                        modulVersion.ModulVersionID
                    );
                    
                    System.Diagnostics.Debug.WriteLine($"📤 Modul '{modulVersion.Modul.ModulnameDE}' erfolgreich eingereicht (Status: InPruefungKoordination)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPTION in starteGenehmigung: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack Trace: {ex.StackTrace}");
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return;
            }
        } // Modul zur Prüfung einreichen für Dozent und Admin
        public static void lehneAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    using (var db = new Services.DatabaseContext())
                    {
                        var modulVersion = db.ModulVersion
                            .Include("Modul")
                            .FirstOrDefault(v => v.Versionsnummer == versionsnummer && v.ModulId == modulID);
                        
                        if (modulVersion != null)
                        {
                            ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                            
                            // ✅ FIX: Benachrichtigung an ERSTELLER senden, nicht an "Dozent" (Rolle)
                            BenachrichtigungsService.SendeBenachrichtigung(
                                modulVersion.Ersteller,  // ✅ Benutzername des Erstellers
                                $"{Benutzer.CurrentUser.Name} hat Ihr Modul '{modulVersion.Modul.ModulnameDE}' abgelehnt. Bitte überarbeiten Sie das Modul entsprechend der Kommentare.",
                                modulVersion.ModulVersionID
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return;
            }
        } // Ablehnen für Koordination + Admin
        public static void leiteWeiter(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Koordination" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungGremium);
                    BenachrichtigungsService.SendeBenachrichtigung("Gremium", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} zur Prüfung durch das Gremium weitergeleitet.", versionsnummer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Modul-Entwurf an Gremium weiterleiten (Koordination + Admin)
        public static void lehneFinalAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    using (var db = new Services.DatabaseContext())
                    {
                        var modulVersion = db.ModulVersion
                            .Include("Modul")
                            .FirstOrDefault(v => v.Versionsnummer == versionsnummer && v.ModulId == modulID);
                        
                        if (modulVersion != null)
                        {
                            ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                            
                            // ✅ FIX: Benachrichtigung an ERSTELLER senden, nicht an "Dozent" (Rolle)
                            BenachrichtigungsService.SendeBenachrichtigung(
                                modulVersion.Ersteller,  // ✅ Benutzername des Erstellers
                                $"{Benutzer.CurrentUser.Name} (Gremium) hat Ihr Modul '{modulVersion.Modul.ModulnameDE}' final abgelehnt. Bitte überarbeiten Sie das Modul entsprechend der Kommentare.",
                                modulVersion.ModulVersionID
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Ablehnen für Gremium + Admin
        public static void schliesseGenehmigungAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" && Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    using (var db = new Services.DatabaseContext())
                    {
                        var modulVersion = db.ModulVersion
                            .Include("Modul")
                            .FirstOrDefault(v => v.Versionsnummer == versionsnummer && v.ModulId == modulID);
                        
                        if (modulVersion != null)
                        {
                            ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Freigegeben);
                            
                            // ✅ FIX: Benachrichtigung an ERSTELLER senden, nicht an "Dozent" (Rolle)
                            BenachrichtigungsService.SendeBenachrichtigung(
                                modulVersion.Ersteller,  // ✅ Benutzername des Erstellers
                                $"Glückwunsch! Ihr Modul '{modulVersion.Modul.ModulnameDE}' wurde von {Benutzer.CurrentUser.Name} (Gremium) freigegeben und ist jetzt offiziell veröffentlicht.",
                                modulVersion.ModulVersionID
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return;
            }
        } // Gremium + Admin only -> Modul freigeben
        public static void archiviereVersion(int modulID, int versionID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfStatusAendern == false)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Archiviert);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        } // Status auf Archiviert setzen
        public static Modul getModulDetails(int modulID) // Modul aus DB abrufen
        {try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modul = db.Modul
                        .Where(m => m.ModulID == modulID)
                        .FirstOrDefault();

                    if (modul == null)
                    {
                        MessageBox.Show($"Modul mit ID {modulID} nicht gefunden.");
                        return null;
                    }
                    else
                    {
                        return modul;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");;
                return null;
            }
        }
    }
}