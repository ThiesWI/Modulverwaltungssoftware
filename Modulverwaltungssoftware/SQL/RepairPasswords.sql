-- SQL-Skript zum Zurücksetzen der Passwörter
-- Führen Sie dieses Skript in SQL Server Management Studio oder Visual Studio aus

USE ModulverwaltungDB;
GO

-- Passwörter zurücksetzen
UPDATE Benutzer SET Passwort = 'gast123' WHERE Name = 'Gast';
UPDATE Benutzer SET Passwort = 'dozent123' WHERE Name = 'Dr. Max Mustermann';
UPDATE Benutzer SET Passwort = 'koordination123' WHERE Name = 'Sabine Beispiel';
UPDATE Benutzer SET Passwort = 'gremium123' WHERE Name = 'Prof. Erika Musterfrau';
UPDATE Benutzer SET Passwort = 'admin123' WHERE Name = 'Philipp Admin';
GO

-- Überprüfung
SELECT Name, Email, Passwort, RollenName FROM Benutzer;
GO
