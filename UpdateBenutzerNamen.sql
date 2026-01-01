-- SQL-Skript zum Aktualisieren der Benutzernamen in der Datenbank
-- Damit sie mit den Auto-Login-Namen übereinstimmen

-- Benutzer-Tabelle aktualisieren
UPDATE Benutzer SET Name = 'Dr. Max Mustermann', Email = 'max.mustermann@hs-example.de' WHERE RollenName = 'Dozent';
UPDATE Benutzer SET Name = 'Sabine Beispiel', Email = 'sabine.beispiel@hs-example.de' WHERE RollenName = 'Koordination';
UPDATE Benutzer SET Name = 'Prof. Erika Musterfrau', Email = 'erika.musterfrau@hs-example.de' WHERE RollenName = 'Gremium';
UPDATE Benutzer SET Name = 'Philipp Admin', Email = 'admin@hs-example.de' WHERE RollenName = 'Admin';

-- Alle ModulVersionen aktualisieren, die vom Dozent erstellt wurden
UPDATE ModulVersion SET Ersteller = 'Dr. Max Mustermann' WHERE Ersteller = 'Dozent';

-- Alle Kommentare aktualisieren
UPDATE Kommentar SET Ersteller = 'Sabine Beispiel' WHERE Ersteller = 'Prof. Dr. Koordinator';
UPDATE Kommentar SET Ersteller = 'Prof. Erika Musterfrau' WHERE Ersteller = 'Gremium';

-- Alle Benachrichtigungen aktualisieren
UPDATE Benachrichtigung SET Empfaenger = 'Dr. Max Mustermann' WHERE Empfaenger = 'Dozent';
UPDATE Benachrichtigung SET Sender = 'Dr. Max Mustermann' WHERE Sender = 'Dozent';
UPDATE Benachrichtigung SET Empfaenger = 'Sabine Beispiel' WHERE Empfaenger = 'Koordination';
UPDATE Benachrichtigung SET Sender = 'Sabine Beispiel' WHERE Sender = 'Koordination';
UPDATE Benachrichtigung SET Empfaenger = 'Prof. Erika Musterfrau' WHERE Empfaenger = 'Gremium';
UPDATE Benachrichtigung SET Sender = 'Prof. Erika Musterfrau' WHERE Sender = 'Gremium';
UPDATE Benachrichtigung SET Empfaenger = 'Philipp Admin' WHERE Empfaenger = 'Admin';
UPDATE Benachrichtigung SET Sender = 'Philipp Admin' WHERE Sender = 'Admin';

-- Studiengang Verantwortliche aktualisieren
UPDATE Studiengang SET Verantwortlicher = 'Dr. Max Mustermann' WHERE Verantwortlicher = 'Dozent';
UPDATE Studiengang SET Verantwortlicher = 'Philipp Admin' WHERE Verantwortlicher = 'Admin';

SELECT 'Benutzer-Namen erfolgreich aktualisiert!' AS Status;
