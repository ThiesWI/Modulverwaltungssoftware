using System;
using System.Collections.Generic;
using System.IO;
using Modulverwaltungssoftware;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PDF_Test
{
    public class PDFService
    {
        public static void ErstellePDF(ModulVersion v)
        {
            string titel = v.Modul.ModulnameDE;
            string modultyp = v.Modul.Modultyp.ToString();
            int semester = v.Modul.EmpfohlenesSemester;
            string pruefungsform = v.Pruefungsform.ToString();
            string turnus = v.Modul.Turnus.ToString();
            double ects = v.EctsPunkte;
            int workloadPraesenz = v.WorkloadPraesenz;
            int workloadSelbststudium = v.WorkloadSelbststudium;
            string verantwortlicher = v.Ersteller;
            List<string> voraussetzungen = v.Modul.Voraussetzungen;
            string voraussetzungenPDF = (voraussetzungen != null && voraussetzungen.Count > 0)
                ? string.Join("\n", voraussetzungen)
                : "";
            List<string> lernziele = v.Lernergebnisse;
            List<string> lehrinhalte = v.Inhaltsgliederung;
            int version = v.Versionsnummer;
            List<string> literatur = v.Literatur;
            QuestPDF.Settings.License = LicenseType.Community;
            string ueberschrift = "Modulhandbuch \t" + modultyp + "\t" + titel;
            DateTime jetzt = DateTime.Now;
            string erstellungsDatum = (jetzt.Year + "_" + jetzt.Month + "_" + jetzt.Day).ToString();
            string dateipfad = GeneriereDateinamen(titel, version, erstellungsDatum);

            var doc = Document.Create(container => // PDF-Dokument erstellen
            {
                container.Page(page =>
                {
                    page.Margin(2, Unit.Centimetre);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));
                    page.Header() // Überschrift
                        .Text(ueberschrift)
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Black);
                    page.Content() // Content-Layer
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Erste Tabelle: Titel
                            column.Item().DefaultTextStyle(x => x.FontColor(Colors.Blue.Darken1)).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                });
                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cell)
                                {
                                    return cell.Border(1).BorderColor(Colors.Black).Padding(5).AlignLeft();
                                }
                                table.Cell().Element(CellStyle).Text(titel).Bold();
                            });

                            // Zweite Tabelle: Semester, Turnus, Dauer, ECTS, Arbeitsbelastung
                            column.Item().DefaultTextStyle(x => x.FontColor(Colors.Black).FontSize(12)).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(5, Unit.Centimetre);
                                });
                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cell)
                                {
                                    return cell.Border(1).BorderColor(Colors.Black).Padding(5).AlignLeft();
                                }
                                table.Cell().Element(CellStyle).Text("Semester").Bold();
                                table.Cell().Element(CellStyle).Text("Turnus").Bold();
                                table.Cell().Element(CellStyle).Text("Dauer").Bold();
                                table.Cell().Element(CellStyle).Text("ECTS").Bold();
                                table.Cell().Element(CellStyle).Text("Studentische Arbeitsbelastung").Bold();
                                table.Cell().Element(CellStyle).Text(semester.ToString() ?? "");
                                table.Cell().Element(CellStyle).Text(turnus ?? "");
                                table.Cell().Element(CellStyle).Text("1 Semester");
                                table.Cell().Element(CellStyle).Text(ects.ToString());
                                table.Cell().Element(CellStyle).Text($"{workloadPraesenz} Stunden Präsenz\n{workloadSelbststudium} Stunden Selbststudium");
                            });
                            // Dritte Tabelle: Voraussetzungen, Prüfungsform, Verantwortlicher
                            column.Item().PaddingTop(10).DefaultTextStyle(x => x.FontColor(Colors.Black).FontSize(12)).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cell)
                                {
                                    return cell.Border(1).BorderColor(Colors.Black).Padding(5).AlignLeft();
                                }
                                table.Cell().Element(CellStyle).Text("Voraussetzungen").Bold();
                                table.Cell().Element(CellStyle).Text("Prüfungsform").Bold();
                                table.Cell().Element(CellStyle).Text("Modulverantwortliche/r").Bold();
                                table.Cell().Element(CellStyle).Text(voraussetzungenPDF ?? "");
                                table.Cell().Element(CellStyle).Text(pruefungsform ?? "");
                                table.Cell().Element(CellStyle).Text(verantwortlicher ?? "");
                            });

                            // Vierte Tabelle: Qualifikationsziele
                            column.Item().PaddingTop(10).DefaultTextStyle(x => x.FontColor(Colors.Black).FontSize(12)).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                });
                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cell)
                                {
                                    return cell.Border(1).BorderColor(Colors.Black).Padding(5).AlignLeft();
                                }
                                table.Cell().Element(CellStyle).Text("Qualifikationsziele").Bold();
                                string lernzieleText = (lernziele != null && lernziele.Count > 0)
                                    ? string.Join("\n\n", lernziele)
                                    : "";
                                table.Cell().Element(CellStyle).Text(lernzieleText);
                            });

                            // Fünfte Tabelle: Lehrinhalte und Literatur
                            column.Item().PaddingTop(10).DefaultTextStyle(x => x.FontColor(Colors.Black).FontSize(12)).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                });
                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cell)
                                {
                                    return cell.Border(1).BorderColor(Colors.Black).Padding(5).AlignLeft();
                                }
                                table.Cell().Element(CellStyle).Text("Lehrinhalte").Bold();

                                string lehrinhalteText = (lehrinhalte != null && lehrinhalte.Count > 0)
                                    ? string.Join("\n\n", lehrinhalte)
                                    : "";

                                string literaturBlock = "";
                                if (!string.IsNullOrWhiteSpace(lehrinhalteText))
                                    literaturBlock += lehrinhalteText;

                                if (literatur != null && literatur.Count > 0)
                                {
                                    literaturBlock += "\n\n";

                                    literaturBlock += "<b>Literatur</b>";

                                    literaturBlock += string.Join("\n", literatur);
                                }

                                table.Cell().Element(CellStyle).Text(t =>
                                {
                                    if (!string.IsNullOrWhiteSpace(lehrinhalteText))
                                        t.Span(lehrinhalteText);
                                    if (literatur != null && literatur.Count > 0)
                                    {
                                        t.Line("\n");
                                        t.Span("Literatur").Bold();
                                        t.Line("\n");
                                    }


                                    if (literatur != null && literatur.Count > 0)
                                    {
                                        for (int i = 0; i < literatur.Count; i++)
                                        {
                                            t.Span(literatur[i]);
                                            if (i < literatur.Count - 1)
                                                t.Line("\n\n");
                                        }
                                    }
                                });
                            });
                        });
                    page.Footer() // Footer mit Datum und Version
                        .AlignCenter()
                        .Text($"Letze Änderung: {erstellungsDatum} | Version: {version}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
            doc.GeneratePdf(dateipfad); // PDF speichern
        }
        public static string GeneriereDateinamen(string titel, int version, string datum) // Dateinamen mit Datum und Version generieren
        {
            string pfad = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string bereinigterTitel = titel;
            foreach (char c in Path.GetInvalidFileNameChars()) // Ungültige Zeichen ersetzen
            {
                bereinigterTitel = bereinigterTitel.Replace(c, '_');
            }
            if (bereinigterTitel.Length > 30) // Titel auf 30 Zeichen kürzen
            {
                string tempTitel = bereinigterTitel;
                int letzterTrenner = 0;
                letzterTrenner = tempTitel.LastIndexOf('_', ' ');
                tempTitel = bereinigterTitel.Substring(0, 30);
                if (letzterTrenner > 0 && letzterTrenner <= 30)
                {
                    bereinigterTitel = tempTitel.Substring(0, letzterTrenner);
                }
                else
                {
                    bereinigterTitel = tempTitel;
                }
            }
            bereinigterTitel = bereinigterTitel.TrimEnd('_', ' ', '&'); // Entferne abschließende unerwünschte Zeichen
            string dokumentName = $"Modulhandbuch {bereinigterTitel} Version {version} {datum}.pdf";
            string vollerPfad = Path.Combine(pfad, "Downloads", dokumentName);
            return vollerPfad;
        }
    }
}