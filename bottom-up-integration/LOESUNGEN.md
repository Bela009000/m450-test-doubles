# Musterlösung: Bottom-up-Integrationstests

Die ausführbare xUnit-Lösung liegt in [`tests/BottomUpIntegrationExercise.Tests`](../tests/BottomUpIntegrationExercise.Tests). Sie enthält getrennte Testklassen für jede Integrationsstufe. Die Express-Erweiterung ist ebenfalls umgesetzt.

```bash
dotnet test tests/BottomUpIntegrationExercise.Tests/BottomUpIntegrationExercise.Tests.csproj
```

## Lösungsweg

### 1. Die untersten Bausteine absichern

Zuerst werden `ItemWeightCalculator` und `ShippingRateCalculator` unabhängig getestet. Der erste Test prüft `0,4 kg × 2 = 0,8 kg`; zwei weitere Tests prüfen ungültige Eingaben. Für den Tarif sind die Werte direkt an beiden Grenzen besonders wichtig: 1 kg und 5 kg gehören noch zur günstigeren Tarifstufe, 1,01 kg und 5,01 kg bereits zur nächsten. Diese Tests helfen später zu unterscheiden, ob ein Fehler in der Einzelberechnung oder in der Verbindung der Komponenten liegt.

### 2. Die Gewichtskette zusammensetzen

`ParcelWeightCalculatorTests` erstellt einen echten `ItemWeightCalculator` und übergibt ihn an `ParcelWeightCalculator`. Der Test ist ein **Testtreiber** für diese zwei Klassen: Er baut sie auf, übergibt zwei Artikel und prüft die Summe von 1 kg. Ein leeres Paket wird vom Paket-Rechner abgewiesen. Bei einem ungültigen zweiten Artikel gelangt die Eingabe bis zum Artikel-Rechner, dessen Exception weitergegeben wird. Diese Stufe testet die Zusammenarbeit der beiden Klassen ohne Ersatzobjekte.

### 3a. Den oberen Dienst mit Platzhalter prüfen

`ShippingQuoteServiceTests` baut den echten Gewichtspfad auf. Für die Tarifberechnung verwendet ein Test `RecordingRatePlaceholder`, der `IShippingRateCalculator` implementiert. Er merkt sich das erhaltene Gewicht, zählt seine Aufrufe und liefert fest CHF 42.–. Der Test prüft drei Dinge: Das Angebot enthält 1 kg, es enthält CHF 42.– und der Platzhalter erhielt genau einmal 1 kg. So ist der Übergabepunkt zum Tarifrechner geprüft, bevor dieser in den vollständigen Ablauf eingebunden wird. Der Platzhalter liegt ausschliesslich im Testprojekt.

### 3b. Den Platzhalter ersetzen

Danach setzt `CreateService` den echten `ShippingRateCalculator` ein. Vier Testfälle prüfen Gesamtgewicht **und** Preis mit allen drei Tarifstufen und beiden Grenzwerten. Damit werden Gewichtsberechnung, Übergabe und Tarifauswahl gemeinsam geprüft. Ein separater Test prüft, dass ein leeres Paket auch über `ShippingQuoteService` abgewiesen wird. Ein Platzhalter könnte die echte Tariftabelle nicht prüfen; deshalb verwenden diese Tests ausschliesslich echte Komponenten.

### 4. Express als Erweiterung

Der optionale Parameter `express` in `CreateQuote` addiert nach der normalen Tarifberechnung CHF 7.–. Ein Test verwendet dasselbe 1-kg-Paket für Standard und Express und erwartet CHF 5.– beziehungsweise CHF 12.– bei unverändertem Gewicht. Dadurch bleiben die vorhandenen Tarifgrenzen und Standardfälle prüfbar.

## Antworten zur Selbstkontrolle

- Wird die Stückzahl ignoriert, scheitert bereits `ItemWeightCalculatorTests.Calculate_WithTwoItems_ReturnsCombinedWeight`. Danach würden auch Integrationsfälle mit mehreren Stücken fehlschlagen.
- Ist die Tarifgrenze bei 1 kg falsch, scheitert `ShippingRateCalculatorTests.Calculate_AtTariffBoundaries_ReturnsExpectedCost` für 1 kg oder 1,01 kg. Auch das vollständige Angebot an dieser Grenze kann scheitern.
- Ein Testtreiber ruft die jeweils höchste bereits integrierte Komponente auf. In xUnit ist dafür keine separate Konsolenanwendung nötig: Die Testmethode übernimmt diese Rolle.
- Der Tarif-Platzhalter isoliert in Stufe 3a den Übergabepunkt. Er wird in Stufe 3b durch den echten Tarifrechner ersetzt; erst diese Stufe prüft den vollständigen Ablauf.
