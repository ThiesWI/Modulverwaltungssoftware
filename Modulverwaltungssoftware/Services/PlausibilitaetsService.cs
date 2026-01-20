// <copyright file="PlausibilitaetsService.cs" company="Modulverwaltungssoftware">
// Copyright (c) Modulverwaltungssoftware. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Modulverwaltungssoftware
{
    using System.Collections.Generic;
    using Modulverwaltungssoftware.Helpers;

    /// <summary>
    /// Service for validating module data plausibility and adherence to academic standards.
    /// Provides workload validation based on ECTS credit points and comprehensive form validation.
    /// </summary>
    public static class PlausibilitaetsService
    {
        private const double EctsMin = 2.5;
        private const double EctsMax = 30.0;
        private const int WorkloadMax = 900;
        private const double HoursPerEctsStandardMin = 28.0;
        private const double HoursPerEctsStandardMax = 32.0;

        /// <summary>
        /// Validates workload hours against ECTS standard requirements.
        /// The standard defines 28-32 hours per ECTS credit point.
        /// </summary>
        /// <param name="stunden">Total workload hours (presence + self-study).</param>
        /// <param name="ects">ECTS credit points assigned to the module.</param>
        /// <returns>A validation message indicating if the workload is within acceptable ranges.</returns>
        public static string PruefeWorkloadStandard(int stunden, double ects)
        {
            if (ects <= 0)
            {
                return "ECTS-Punkte müssen größer als 0 sein.";
            }

            double stundenProEcts = stunden / ects;

            if (stundenProEcts >= HoursPerEctsStandardMin && stundenProEcts <= HoursPerEctsStandardMax)
            {
                return "Der Workload entspricht dem Standard.";
            }

            if (stunden >= 60 && stunden <= 450)
            {
                return "Der Workload liegt im akzeptablen Bereich.";
            }

            if (stunden >= 450 && stunden <= WorkloadMax)
            {
                return "Ungewöhnlich hoher Workload. Bitte stellen Sie sicher, dass dies beabsichtigt ist.";
            }

            return "Der Workload liegt außerhalb des üblichen Bereichs. Bitte prüfen Sie, ob ein Eingabefehler vorliegt.";
        }

        /// <summary>
        /// Internal validation method to check if workload meets strict standard requirements.
        /// Used for binary validation (pass/fail) without detailed messages.
        /// </summary>
        /// <param name="stunden">Total workload hours.</param>
        /// <param name="ects">ECTS credit points.</param>
        /// <returns><c>true</c> if workload is within standard range; otherwise, <c>false</c>.</returns>
        internal static bool PruefeWorkloadStandardIntern(int stunden, double ects)
        {
            if (ects <= 0 || ects < EctsMin || ects > EctsMax)
            {
                return false;
            }

            if (stunden <= 0 || stunden > WorkloadMax)
            {
                return false;
            }

            double stundenProEcts = stunden / ects;
            return stundenProEcts >= HoursPerEctsStandardMin &&
                   stundenProEcts <= HoursPerEctsStandardMax;
        }

        /// <summary>
        /// Validates a complete module version form for all required fields and plausibility.
        /// </summary>
        /// <param name="v">The module version to validate.</param>
        /// <returns>
        /// A string containing error messages for each invalid field,
        /// or "Keine Fehler gefunden." if all validations pass.
        /// </returns>
        /// <remarks>
        /// Validates: module type, semester, exam form, schedule, ECTS, workload, responsible person, learning outcomes, and content.
        /// </remarks>
        public static string PruefeForm(ModulVersion v)
        {
            string modultyp = v.Modul.Modultyp.ToString();
            int semester = v.Modul.EmpfohlenesSemester;
            string pruefungsform = v.Pruefungsform;
            string turnus = v.Modul.Turnus.ToString();
            double ects = v.EctsPunkte;
            int workloadGesamt = v.WorkloadPraesenz + v.WorkloadSelbststudium;

            bool workloadTest = PruefeWorkloadStandardIntern(workloadGesamt, ects);
            bool typTest = modultyp == "Grundlagen" || modultyp == "Wahlpflicht";
            bool semesterTest = semester >= 1 && semester <= 8;

            var gueltigePruefungsformen = new List<string>
            {
                "PL",
                "SP",
                "SL",
                "Klausur",
                "Mündliche Prüfung",
                "Projektarbeit",
                "Hausarbeit",
                "Präsentation",
                "Portfolio",
                "Referat",
            };

            bool pruefungsformTest = !string.IsNullOrWhiteSpace(pruefungsform) &&
                                     gueltigePruefungsformen.Contains(pruefungsform);

            bool turnusTest = turnus == "JedesSemester" ||
                             turnus == "NurWintersemester" ||
                             turnus == "NurSommersemester";

            bool verantwortlicherTest = !string.IsNullOrEmpty(v.Ersteller);
            bool lernzieleTest = v.Lernergebnisse?.Count > 0;
            bool lehrinhaltTest = v.Inhaltsgliederung?.Count > 0;

            return FehlerListe(
                workloadTest,
                typTest,
                semesterTest,
                pruefungsformTest,
                turnusTest,
                verantwortlicherTest,
                lehrinhaltTest,
                lernzieleTest);
        }

        /// <summary>
        /// Generates a formatted error message listing all validation failures.
        /// </summary>
        /// <param name="workload">Indicates if workload validation passed.</param>
        /// <param name="typ">Indicates if module type validation passed.</param>
        /// <param name="semester">Indicates if semester validation passed.</param>
        /// <param name="pruefung">Indicates if exam form validation passed.</param>
        /// <param name="turnus">Indicates if schedule validation passed.</param>
        /// <param name="verantwortlicher">Indicates if responsible person validation passed.</param>
        /// <param name="lehrinhalte">Indicates if content validation passed.</param>
        /// <param name="lernziele">Indicates if learning outcomes validation passed.</param>
        /// <returns>
        /// "Keine Fehler gefunden." if all validations pass;
        /// otherwise, a multi-line string listing all validation errors.
        /// </returns>
        private static string FehlerListe(
            bool workload,
            bool typ,
            bool semester,
            bool pruefung,
            bool turnus,
            bool verantwortlicher,
            bool lehrinhalte,
            bool lernziele)
        {
            if (workload && typ && semester && pruefung && turnus && verantwortlicher && lehrinhalte && lernziele)
            {
                return "Keine Fehler gefunden.";
            }

            var fehlerMeldungen = "Fehler gefunden in folgenden Bereichen:\n";

            if (!workload)
            {
                fehlerMeldungen += "Workload entspricht nicht dem Standard.\n";
            }

            if (!typ)
            {
                fehlerMeldungen += "Ungültiger Modultyp.\n";
            }

            if (!semester)
            {
                fehlerMeldungen += "Empfohlenes Semester außerhalb des gültigen Bereichs.\n";
            }

            if (!pruefung)
            {
                fehlerMeldungen += "Ungültige Prüfungsform.\n";
            }

            if (!turnus)
            {
                fehlerMeldungen += "Ungültiger Turnus.\n";
            }

            if (!verantwortlicher)
            {
                fehlerMeldungen += "Kein Verantwortlicher angegeben.\n";
            }

            if (!lehrinhalte)
            {
                fehlerMeldungen += "Keine Lehrinhalte angegeben.\n";
            }

            if (!lernziele)
            {
                fehlerMeldungen += "Keine Lernziele angegeben.\n";
            }

            return fehlerMeldungen;
        }

        /// <summary>
        /// Validates a module version and returns detailed field-specific errors.
        /// Provides more granular validation results than <see cref="PruefeForm"/>.
        /// </summary>
        /// <param name="v">The module version to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> object containing:
        /// <list type="bullet">
        /// <item><description>Overall validation status (IsValid)</description></item>
        /// <item><description>Field-specific error messages</description></item>
        /// <item><description>Global error message if applicable</description></item>
        /// </list>
        /// </returns>
        public static ValidationResult ValidateModulVersion(ModulVersion v)
        {
            var result = new ValidationResult();

            if (v?.Modul == null)
            {
                result.IsValid = false;
                result.GlobalMessage = "Modulversion oder Modul-Daten fehlen.";
                return result;
            }

            // Modultyp prüfen
            string modultyp = v.Modul.Modultyp.ToString();
            if (modultyp != "Grundlagen" && modultyp != "Wahlpflicht")
            {
                result.AddError("Modultyp", "Ungültiger Modultyp. Nur 'Grundlagen' oder 'Wahlpflicht' erlaubt.");
            }

            // Semester prüfen (1-8)
            int semester = v.Modul.EmpfohlenesSemester;
            if (semester < 1 || semester > 8)
            {
                result.AddError("Semester", "Empfohlenes Semester muss zwischen 1 und 8 liegen.");
            }

            // Prüfungsform prüfen
            var gueltigePruefungsformen = new List<string>
            {
                "PL",
                "SP",
                "SL",
                "Klausur",
                "Mündliche Prüfung",
                "Projektarbeit",
                "Hausarbeit",
                "Präsentation",
                "Portfolio",
                "Referat",
            };

            if (string.IsNullOrWhiteSpace(v.Pruefungsform) || !gueltigePruefungsformen.Contains(v.Pruefungsform))
            {
                result.AddError("Pruefungsform", "Ungültige Prüfungsform.");
            }

            // Turnus prüfen
            string turnus = v.Modul.Turnus.ToString();
            if (turnus != "JedesSemester" && turnus != "NurWintersemester" && turnus != "NurSommersemester")
            {
                result.AddError("Turnus", "Ungültiger Turnus.");
            }

            // Verantwortlicher prüfen
            if (string.IsNullOrEmpty(v.Ersteller))
            {
                result.AddError("Verantwortlicher", "Kein Verantwortlicher angegeben.");
            }

            // Lernziele prüfen
            if (v.Lernergebnisse?.Count == 0 || v.Lernergebnisse == null)
            {
                result.AddError("Lernziele", "Keine Lernziele angegeben.");
            }

            // Lehrinhalte prüfen
            if (v.Inhaltsgliederung?.Count == 0 || v.Inhaltsgliederung == null)
            {
                result.AddError("Lehrinhalte", "Keine Lehrinhalte angegeben.");
            }

            // Workload prüfen
            int workloadGesamt = v.WorkloadPraesenz + v.WorkloadSelbststudium;
            double ects = v.EctsPunkte;

            if (ects <= 0)
            {
                result.AddError("ECTS", "ECTS-Punkte müssen größer als 0 sein.");
            }
            else if (!PruefeWorkloadStandardIntern(workloadGesamt, ects))
            {
                result.AddError("Workload", "Workload entspricht nicht dem Standard (28-32 Stunden/ECTS).");
            }

            return result;
        }
    }
}