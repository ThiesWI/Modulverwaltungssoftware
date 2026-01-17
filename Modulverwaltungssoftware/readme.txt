################################################################################
MODULVERWALTUNG - HOCHSCHULE EXAMPLE

Software-Engineering Semesterprojekt
################################################################################

Vielen Dank, dass Sie unser Projekt testen. Diese Datei enthält alle notwendigen Informationen zum Start der Anwendung, zur Anmeldung und zu bekannten Einschränkungen.

1. INSTALLATION & START
Voraussetzungen:
- .NET 6.0 (oder höher) Runtime / SDK
- Visual Studio 2022 (empfohlen)


Starten der Anwendung:
Öffnen Sie die Solution (.sln) in Visual Studio.

Stellen Sie sicher, dass das Projekt "Modulverwaltung" als Startprojektfestgelegt ist.

Starten Sie die Anwendung über "Debug starten" (F5).

HINWEIS ZUR DATENBANK: Die Anwendung nutzt eine lokale SQLite-Datenbank/Mock-Datenbank, die beim Start automatisch initialisiert wird. 
Es ist keine manuelle Einrichtung nötig.

2. BENUTZERKONTEN & ROLLEN (TESTDATEN)
Für den Test der verschiedenen Workflows wurden folgende Benutzerkonten vorbereitet. Bitte nutzen Sie diese, um die rollenspezifischen Funktionen(Dozent, Koordination, Gremium) zu prüfen.

Rolle: DOZENT (Fokus: Module erstellen, bearbeiten)
Name:     Dr. Max Mustermann
E-Mail:   max.mustermann@hs-example.de
Passwort: dozent123

Rolle: KOORDINATION (Fokus: Dashboard, Modulhandbuch, Fristen)
Name:     Sabine Beispiel
E-Mail:   sabine.beispiel@hs-example.de
Passwort: koordination123

Rolle: GREMIUM (Fokus: Genehmigung, Kommentare, Ablehnung)
Name:     Prof. Erika Musterfrau
E-Mail:   erika.musterfrau@hs-example.de
Passwort: gremium123

Rolle: ADMIN (Fokus: Nutzerverwaltung, System-Log)
Name:     Philipp Admin
E-Mail:   admin@hs-example.de
Passwort: admin123

Rolle: GAST (Fokus: Nur Lesen / Suche)
Name:     Gast
E-Mail:   gast@hochschule.de
Passwort: gast123

3. WICHTIGE HINWEISE ZUR BEDIENUNG
- Auto-Login (Entwickler-Modus): Falls die Anwendung direkt ohne Login startet, ist das "Auto-Login-Flag" in der LoginWindow.xaml.cs noch aktiv. Dies dient der Entwicklungsbeschleunigung. Für den regulären Test bitte das Flag autoLoginFlag auf false setzen.

- PDF-Export: Der Export funktioniert aktuell für einzelne Module. Der Batch-Export für ganze Studiengänge ist in dieser Version noch nicht implementiert (siehe Testbericht Kap. 7 "Known Limitations").

4. ENTWICKLER-TEAM

Dieses Projekt wurde erstellt im Rahmen des Moduls Software Engineering. Abgabedatum: 20.01.2025