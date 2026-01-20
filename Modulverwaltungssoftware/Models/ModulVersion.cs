using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class ModulVersion
    {
        public int ModulVersionID { get; set; }
        [Required]
        public int ModulId { get; set; }
        [Required]
        public int Versionsnummer { get; set; }
        [Required]
        public bool hatKommentar { get; set; } = false;
        public virtual Kommentar Kommentar { get; set; }
        public virtual Modul Modul { get; set; }
        [Required]
        [StringLength(25)]
        public string GueltigAbSemester { get; set; } = "SoSe 9999";
        public enum Status { Entwurf = 0, InPruefungKoordination = 1, InPruefungGremium = 2, Aenderungsbedarf = 3, Freigegeben = 4, Archiviert = 5 }
        [Required]
        public Status ModulStatus { get; set; } = Status.Entwurf;
        [Required]
        public DateTime LetzteAenderung { get; set; } = DateTime.Now;
        [Required]
        public int WorkloadPraesenz { get; set; }
        [Required]
        public int WorkloadSelbststudium { get; set; }
        [Required]
        public double EctsPunkte { get; set; }
        [Required]
        [StringLength(100)]
        public string Pruefungsform { get; set; } = "Klausur";
        public string Ersteller { get; set; }
        [NotMapped]
        public List<string> Lernergebnisse { get; set; }
        [NotMapped]
        public List<string> Inhaltsgliederung { get; set; }
        [NotMapped]
        public List<string> Literatur { get; set; }
        [Required]
        [StringLength(4000)]
        public string LernergebnisseDb
        {
            get => JsonConvert.SerializeObject(Lernergebnisse);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Lernergebnisse = new List<string>();
                }
                else
                {
                    try
                    {
                        Lernergebnisse = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Lernergebnisse = new List<string> { value };
                    }
                }
            }
        }

        [Required]
        [StringLength(4000)]
        public string InhaltsgliederungDb
        {
            get => JsonConvert.SerializeObject(Inhaltsgliederung);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Inhaltsgliederung = new List<string>();
                }
                else
                {
                    try
                    {
                        Inhaltsgliederung = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Inhaltsgliederung = new List<string> { value };
                    }
                }
            }
        }

        [StringLength(4000)]
        public string LiteraturDb
        {
            get => JsonConvert.SerializeObject(Literatur);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Literatur = new List<string>();
                }
                else
                {
                    try
                    {
                        Literatur = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Literatur = new List<string> { value };
                    }
                }
            }
        }

        public ModulVersion()
        {
            Lernergebnisse = new List<string>();
            Inhaltsgliederung = new List<string>();
            Literatur = new List<string>();
        }
        public static void setStatus(int versionID, int modulID, Status neuerStatus)
        {
            try
            {
                // ✅ FIX: Berechtigungsprüfung ENTFERNT!
                // Die aufrufende Methode (z.B. WorkflowController.starteGenehmigung) 
                // prüft bereits die Berechtigung (Ersteller, Koordination, Gremium, Admin)
                // 
                // ALTE (FALSCHE) LOGIK:
                // if (Benutzer.CurrentUser.AktuelleRolle.DarfStatusAendern == false && Benutzer.CurrentUser.RollenName != "Admin")
                // ❌ Problem: Dozenten haben DarfStatusAendern = false, können also nicht einreichen!
                //
                // NEUE LOGIK: Keine Berechtigungsprüfung hier, nur Status setzen

                System.Diagnostics.Debug.WriteLine($"🔄 setStatus: versionID={versionID}, modulID={modulID}, neuerStatus={neuerStatus}");

                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.Versionsnummer == versionID && mv.ModulId == modulID);
                    if (modulVersion != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"   Alter Status: {modulVersion.ModulStatus} → Neuer Status: {neuerStatus}");

                        modulVersion.ModulStatus = neuerStatus;
                        modulVersion.LetzteAenderung = DateTime.Now;
                        db.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"✅ Status erfolgreich geändert!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ ModulVersion nicht gefunden!");
                        MessageBox.Show("ModulVersion nicht gefunden");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPTION in setStatus: {ex.Message}");
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return;
            }
        }
        public static bool setDaten(ModulVersion version) //Setzt die Daten der Modulversion in der DB
        {
            string fehlermeldung = PlausibilitaetsService.pruefeForm(version);
            if (fehlermeldung != "Keine Fehler gefunden.")
            {
                MessageBox.Show(fehlermeldung, "Moduldaten wurden nicht in die Datenbank übernommen.");
                return false;
            }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.Versionsnummer == version.Versionsnummer && mv.ModulId == version.ModulId);
                    if (modulVersion == null)
                    {
                        // NEU: Alle Parameter direkt übernehmen!
                        if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == true)
                        {
                            modulVersion = new ModulVersion
                            {
                                ModulId = version.ModulId,
                                Versionsnummer = version.Versionsnummer,
                                GueltigAbSemester = version.GueltigAbSemester,
                                WorkloadPraesenz = version.WorkloadPraesenz,
                                WorkloadSelbststudium = version.WorkloadSelbststudium,
                                EctsPunkte = version.EctsPunkte,
                                Pruefungsform = version.Pruefungsform,
                                Literatur = version.Literatur,
                                Lernergebnisse = version.Lernergebnisse,
                                Inhaltsgliederung = version.Inhaltsgliederung,
                                LetzteAenderung = DateTime.Now,
                                ModulStatus = version.ModulStatus,
                                Ersteller = version.Ersteller,
                                hatKommentar = version.hatKommentar
                            };
                            db.ModulVersion.Add(modulVersion);
                            db.SaveChanges();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um diese Aktion durchzuführen.");
                            return false;
                        }
                    }
                    if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == true)
                    {
                        if (modulVersion.ModulStatus != Status.Entwurf && modulVersion.ModulStatus != Status.Aenderungsbedarf)
                        {
                            int i = ModulController.create(modulVersion.ModulId, modulVersion);
                            if (i == -1)
                            {
                                MessageBox.Show("Fehler beim Erstellen der neuen Modulversion.");
                                return false;
                            }
                        }
                        modulVersion.GueltigAbSemester = version.GueltigAbSemester;
                        modulVersion.WorkloadPraesenz = version.WorkloadPraesenz;
                        modulVersion.WorkloadSelbststudium = version.WorkloadSelbststudium;
                        modulVersion.EctsPunkte = version.EctsPunkte;
                        modulVersion.Pruefungsform = version.Pruefungsform;
                        modulVersion.Literatur = version.Literatur;
                        modulVersion.Lernergebnisse = version.Lernergebnisse;
                        modulVersion.Inhaltsgliederung = version.Inhaltsgliederung;
                        modulVersion.LetzteAenderung = DateTime.Now;
                    }
                    else
                    {
                        MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um diese Aktion durchzuführen.");
                        return false;
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return false;
            }
        }
    }
}
