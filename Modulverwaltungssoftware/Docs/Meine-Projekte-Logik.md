# ?? "MEINE PROJEKTE" - FILTERLOGIK

## Übersicht: Welche Module erscheinen wo?

---

## ?? **ZIEL:**

- **"Meine Projekte"** zeigt nur Module, die für den Benutzer **relevant** sind
- **StartPage** zeigt alle Module, die der Benutzer **sehen darf** (rollenbasiert)
- **Leere "Meine Projekte"** zeigen Meldung statt leer

---

## ?? **"MEINE PROJEKTE" FILTERREGELN**

### **1. GAST**

```
Anzeige: "Keine eigenen Projekte vorhanden"

Begründung:
- Gäste können KEINE Module erstellen
- Gäste bekommen KEINE Module zugewiesen
- Daher: Immer leer
```

### **2. DOZENT**

```
Anzeige: NUR selbst erstellte Module

Filter-Logik:
1. Finde alle ModulVersionen, wo Ersteller = CurrentUser
2. Extrahiere eindeutige ModulIDs
3. Zeige diese Module

Beispiel:
- Dozent "Hans" hat Modul "Mathematik I" erstellt
- "Mathematik I" erscheint in "Meine Projekte"
- Module von anderen Dozenten NICHT
```

### **3. KOORDINATION**

```
Anzeige: Module mit Status "InPruefungKoordination"

Filter-Logik:
1. Finde alle ModulVersionen mit Status = InPruefungKoordination
2. Extrahiere eindeutige ModulIDs
3. Zeige diese Module (= Arbeitsliste)

Beispiel:
- Modul "Thermodynamik I" wurde eingereicht
- Status = InPruefungKoordination
- ? Erscheint in "Meine Projekte" der Koordination
- ? Andere Module NICHT
```

### **4. GREMIUM**

```
Anzeige: Module mit Status "InPruefungGremium"

Filter-Logik:
1. Finde alle ModulVersionen mit Status = InPruefungGremium
2. Extrahiere eindeutige ModulIDs
3. Zeige diese Module (= Arbeitsliste)

Beispiel:
- Koordination leitet Modul weiter
- Status = InPruefungGremium
- ? Erscheint in "Meine Projekte" des Gremiums
- ? Andere Module NICHT
```

### **5. ADMIN**

```
Anzeige: ALLE Module

Filter-Logik:
1. Keine Filterung
2. Zeige alle Module aus der Datenbank

Begründung:
- Admin hat Vollzugriff
- Muss alle Module verwalten können
```

---

## ?? **WORKFLOW-BEISPIEL**

### **Szenario: Modul "Software Engineering I"**

| Schritt | Status | Dozent (Ersteller) | Koordination | Gremium | Admin |
|---------|--------|-------------------|--------------|---------|-------|
| 1. Erstellen | Entwurf | ? In "Meine Projekte" | ? | ? | ? |
| 2. Einreichen | InPruefungKoordination | ? In "Meine Projekte" | ? Neu! | ? | ? |
| 3. Weiterleiten | InPruefungGremium | ? In "Meine Projekte" | ? Weg! | ? Neu! | ? |
| 4. Freigeben | Freigegeben | ? In "Meine Projekte" | ? | ? | ? |

**Erklärung:**
- **Dozent**: Modul bleibt immer in "Meine Projekte" (Ersteller)
- **Koordination**: Nur während Status "InPruefungKoordination"
- **Gremium**: Nur während Status "InPruefungGremium"
- **Admin**: Immer sichtbar

---

## ?? **"NEUES MODUL" BUTTON**

### **Regel:**

```
Aktiviert für: Admin, Dozent
Deaktiviert für: Gast, Koordination, Gremium

Tooltip (deaktiviert):
"Nur Administratoren und Dozenten dürfen neue Module erstellen."
```

### **Begründung:**

- **Gast**: Nur Leserechte
- **Koordination**: Prüft nur, erstellt nicht
- **Gremium**: Genehmigt nur, erstellt nicht
- **Dozent**: Erstellt eigene Module
- **Admin**: Vollzugriff

---

## ?? **MELDUNG BEI LEEREN "MEINE PROJEKTE"**

### **Anzeige:**

Statt leerem Dropdown erscheint:
```
"Keine eigenen Projekte vorhanden"
```

### **Klick-Verhalten:**

```
Klick auf "Keine eigenen Projekte vorhanden"
? MessageBox: "Sie haben momentan keine eigenen Projekte."
? Popup schließt sich
```

### **Wann erscheint die Meldung?**

| Rolle | Wann leer? |
|-------|-----------|
| **Gast** | Immer |
| **Dozent** | Wenn noch kein Modul erstellt |
| **Koordination** | Wenn keine Module in Prüfung |
| **Gremium** | Wenn keine Module zur Genehmigung |
| **Admin** | Nie (hat immer alle Module) |

---

## ?? **IMPLEMENTIERUNGS-DETAILS**

### **Code-Struktur:**

```csharp
// MainWindow.xaml.cs
private void RefreshMyProjects()
{
    string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
    List<string> modulNamen = new List<string>();

    switch (rolle)
    {
        case "Gast":
            modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
            break;

        case "Dozent":
            // Filter: Ersteller == CurrentUser
            var dozentenModulIds = db.ModulVersion
                .Where(v => v.Ersteller == currentUser)
                .Select(v => v.ModulId)
                .Distinct()
                .ToList();
            meineModule = db.Modul.Where(m => dozentenModulIds.Contains(m.ModulID));
            break;

        case "Koordination":
            // Filter: Status == InPruefungKoordination
            var koordinationModulIds = db.ModulVersion
                .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                .Select(v => v.ModulId)
                .Distinct()
                .ToList();
            meineModule = db.Modul.Where(m => koordinationModulIds.Contains(m.ModulID));
            break;

        // ... weitere Rollen
    }

    // Falls leer ? Meldung
    if (modulNamen.Count == 0)
        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };

    UpdateProjects(modulNamen);
}
```

---

## ?? **TEST-SZENARIEN**

### **Test 1: Gast-Login**

```
Login: Gast
"Meine Projekte" öffnen

Erwartung:
? Dropdown zeigt: "Keine eigenen Projekte vorhanden"
? Klick darauf: MessageBox mit Info
```

### **Test 2: Dozent ohne Module**

```
Login: Dozent (neu, ohne Module)
"Meine Projekte" öffnen

Erwartung:
? Dropdown zeigt: "Keine eigenen Projekte vorhanden"
? "Neues Modul" Button: AKTIV
```

### **Test 3: Dozent mit 2 Modulen**

```
Login: Dozent "Hans"
Module: "Mathematik I" (Ersteller: Hans), "Physik I" (Ersteller: Maria)
"Meine Projekte" öffnen

Erwartung:
? Dropdown zeigt: "Mathematik I"
? "Physik I" NICHT sichtbar (nicht Ersteller)
```

### **Test 4: Koordination bei Prüfung**

```
Login: Koordination
Module: 
  - "Thermodynamik I" (Status: InPruefungKoordination)
  - "Software Engineering I" (Status: Freigegeben)
"Meine Projekte" öffnen

Erwartung:
? Dropdown zeigt: "Thermodynamik I"
? "Software Engineering I" NICHT sichtbar (anderer Status)
```

### **Test 5: Admin**

```
Login: Admin
Alle Module in DB: "Mathematik I", "Physik I", "Thermodynamik I"
"Meine Projekte" öffnen

Erwartung:
? Dropdown zeigt: ALLE 3 Module
? Alphabetisch sortiert
```

### **Test 6: "Neues Modul" Button**

```
Test für jede Rolle:

Gast: ? DEAKTIVIERT, Tooltip: "Nur Administratoren und Dozenten..."
Dozent: ? AKTIV
Koordination: ? DEAKTIVIERT
Gremium: ? DEAKTIVIERT
Admin: ? AKTIV
```

---

## ? **ZUSAMMENFASSUNG**

### **Vorteile der Implementierung:**

1. ? **Übersichtlichkeit**: Jede Rolle sieht nur relevante Module
2. ? **Arbeitsliste**: Koordination/Gremium sehen ihre Aufgaben
3. ? **Datenschutz**: Dozenten sehen keine fremden Entwürfe
4. ? **Benutzerfreundlichkeit**: Meldung statt leerem Dropdown
5. ? **Konsistenz**: Gleiche Logik wie Button-Steuerung

---

Erstellt: 2024
Letzte Aktualisierung: 2024
