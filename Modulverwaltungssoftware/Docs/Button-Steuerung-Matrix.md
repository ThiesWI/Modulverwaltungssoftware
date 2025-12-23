# ?? BUTTON-STEUERUNG MATRIX - MODULVIEW

## Vollständige Übersicht: Wer darf was bei welchem Status?

---

## ?? LEGENDE

| Symbol | Bedeutung |
|--------|-----------|
| ? | Button AKTIV (klickbar) |
| ? | Button DEAKTIVIERT (ausgegraut) |
| ?? | Bedingung: Nur wenn Ersteller |
| ?? | Bedingung: Nur wenn Admin |
| ?? | Bedingung: Nur wenn Koordination |
| ?? | Bedingung: Nur wenn Gremium |

---

## 1?? STATUS: **ENTWURF**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Gast** | ? (unsichtbar) | ? | ? | ? | ? |
| **Ersteller (Dozent)** | ? | ? | ? | ? | ? |
| **Anderer Dozent** | ? | ? | ? | ? | ? |
| **Koordination** | ? | ? | ? | ? | ? |
| **Gremium** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Entwurf = privat** ? Nur Ersteller + Admin dürfen ändern
- **Kommentieren deaktiviert**: Noch nicht eingereicht, keine Reviewphase
- **Einreichen aktiv**: Ersteller kann zur Prüfung einreichen

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.Entwurf:
    bearbeitenButton.IsEnabled = isErsteller || isAdmin;
    loeschenButton.IsEnabled = isErsteller || isAdmin;
    einreichenButton.IsEnabled = isErsteller || isAdmin;
    kommentierenButton.IsEnabled = false; // ? RICHTIG
    break;
```

---

## 2?? STATUS: **IN PRÜFUNG KOORDINATION**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Ersteller (Dozent)** | ? | ? | ? | ? | ? |
| **Koordination** | ? | ? | ? | ? | ? |
| **Gremium** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Modul "eingefroren"**: Ersteller kann nicht mehr bearbeiten
- **NUR Koordination + Admin** dürfen kommentieren (Reviewphase)
- **Gremium darf NICHT kommentieren** (noch nicht in deren Zuständigkeit)
- **Admin darf löschen** (Notfall/Korrektur)

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.InPruefungKoordination:
    bearbeitenButton.IsEnabled = false; // ? RICHTIG: Niemand außer Admin
    loeschenButton.IsEnabled = isAdmin; // ? RICHTIG
    einreichenButton.IsEnabled = false; // ? RICHTIG: Bereits eingereicht
    kommentierenButton.IsEnabled = isKoordination || isAdmin; // ? RICHTIG
    break;
```

---

## 3?? STATUS: **IN PRÜFUNG GREMIUM**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Ersteller (Dozent)** | ? | ? | ? | ? | ? |
| **Koordination** | ? | ? | ? | ? | ? |
| **Gremium** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Finale Prüfphase**: Nur Gremium entscheidet
- **Koordination darf NICHT mehr kommentieren** (Phase abgeschlossen)
- **NUR Gremium + Admin** dürfen kommentieren
- **Ersteller kann nur warten**

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.InPruefungGremium:
    bearbeitenButton.IsEnabled = false; // ? RICHTIG
    loeschenButton.IsEnabled = isAdmin; // ? RICHTIG
    einreichenButton.IsEnabled = false; // ? RICHTIG
    kommentierenButton.IsEnabled = isGremium || isAdmin; // ? RICHTIG
    break;
```

---

## 4?? STATUS: **ÄNDERUNGSBEDARF**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Ersteller (Dozent)** | ? | ? | ? | ? | ? |
| **Koordination** | ? | ? | ? | ? | ? |
| **Gremium** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Zurück an Ersteller**: Muss überarbeiten
- **Kommentare bereits vorhanden**: In EditWithCommentsView sichtbar
- **Kommentieren deaktiviert**: Feedback schon gegeben
- **Nach Überarbeitung**: Kann erneut eingereicht werden

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.Aenderungsbedarf:
    bearbeitenButton.IsEnabled = isErsteller || isAdmin; // ? RICHTIG
    loeschenButton.IsEnabled = isErsteller || isAdmin; // ? RICHTIG
    einreichenButton.IsEnabled = isErsteller || isAdmin; // ? RICHTIG
    kommentierenButton.IsEnabled = false; // ? RICHTIG: Kommentare vorhanden
    break;
```

---

## 5?? STATUS: **FREIGEGEBEN** ? **WICHTIG!**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Ersteller (Dozent)** | ? | ? | ? | ? | ? |
| **Koordination** | ? | ? | ? | ? | ? |
| **Gremium** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Offiziell veröffentlicht**: Schreibschutz für alle außer Admin
- **Ersteller darf NICHT mehr ändern**: Könnte Inkonsistenzen erzeugen
- **Koordination/Gremium dürfen NICHT kommentieren**: Phase abgeschlossen
- **Nur Admin**: Notfallkorrekturen möglich
- **Einreichen unmöglich**: Bereits freigegeben

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.Freigegeben:
    bearbeitenButton.IsEnabled = isAdmin; // ? RICHTIG
    loeschenButton.IsEnabled = isAdmin; // ? RICHTIG
    einreichenButton.IsEnabled = false; // ? RICHTIG
    kommentierenButton.IsEnabled = isAdmin; // ? RICHTIG
    break;
```

---

## 6?? STATUS: **ARCHIVIERT**

### Berechtigungen:

| Rolle | Exportieren | Bearbeiten | Löschen | Kommentieren | Einreichen |
|-------|-------------|------------|---------|--------------|------------|
| **Alle außer Admin** | ? | ? | ? | ? | ? |
| **Admin** | ? | ? | ? | ? | ? |

### Begründung:
- **Historisches Archiv**: Nur zur Dokumentation
- **Nur Admin darf reaktivieren/löschen**: Datenverwaltung
- **Keine Kommentare**: Nicht mehr im aktiven Workflow

### Code (bereits implementiert):
```csharp
case ModulVersion.Status.Archiviert:
    bearbeitenButton.IsEnabled = isAdmin; // ? RICHTIG
    loeschenButton.IsEnabled = isAdmin; // ? RICHTIG
    einreichenButton.IsEnabled = false; // ? RICHTIG
    kommentierenButton.IsEnabled = false; // ? RICHTIG: Auch Admin nicht
    break;
```

---

## ? ZUSAMMENFASSUNG: ALLE ANFORDERUNGEN ERFÜLLT

### ? **Koordination/Gremium bei nicht-freigegebenen Modulen:**
- ? Können NUR exportieren
- ? Können NICHT bearbeiten (außer in ihrer Prüfphase)
- ? Können NICHT löschen (nur Admin)
- ? Können NUR in ihrer Prüfphase kommentieren:
  - Koordination ? NUR bei "InPruefungKoordination"
  - Gremium ? NUR bei "InPruefungGremium"

### ? **Admin:**
- ? Kann IMMER bearbeiten
- ? Kann IMMER löschen
- ? Kann IMMER kommentieren (außer Archiviert)
- ? Kann bei Entwurf/Änderungsbedarf einreichen

### ? **Ersteller:**
- ? Kann NICHT kommentieren (nie!)
- ? Kann NUR eigene Entwürfe/Änderungsbedarf bearbeiten
- ? Kann NUR eigene Entwürfe/Änderungsbedarf löschen
- ? Kann bei Entwurf/Änderungsbedarf einreichen

---

## ?? KEINE WEITEREN ÄNDERUNGEN NOTWENDIG!

Die aktuelle Implementierung in `UpdateButtonStates()` ist **VOLLSTÄNDIG und KORREKT**.

Alle deine Anforderungen sind bereits erfüllt:
1. ? Freigegeben: Nur Admin darf Einreichen/Kommentieren/Löschen
2. ? Koordination/Gremium: Nur in ihrer Prüfphase kommentieren
3. ? Ersteller: Kann nie kommentieren
4. ? Admin: Hat immer Vollzugriff (außer Einreichen bei Freigegeben/Archiviert)

---

Erstellt: 2024
Letzte Aktualisierung: 2024
