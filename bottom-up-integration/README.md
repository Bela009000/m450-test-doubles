# Lernaufgabe: Bottom-up-Integrationstests

> Dieser Branch enthält eine [Musterlösung mit Lösungsweg](LOESUNGEN.md). Die Arbeitsanweisungen beschreiben die Aufgabe für Lernende.

## Ausgangslage

Ein Versanddienst berechnet aus den Artikeln eines Pakets dessen Gesamtgewicht und anschliessend die Versandkosten. Die fertigen Klassen stehen in [`Shipping.cs`](Shipping.cs); Tests erstellt ihr selbst. Verändert für die Grundaufgaben den Produktionscode nicht.

```text
ShippingQuoteService
├── ParcelWeightCalculator
│   └── ItemWeightCalculator
└── ShippingRateCalculator
```

Ein `ParcelItem` hat ein Einzelgewicht in kg und eine Stückzahl. Die Tarife betragen CHF 5.– bis und mit 1 kg, CHF 9.– über 1 bis und mit 5 kg und CHF 15.– über 5 kg. Gewichte und Tarife sind `decimal`-Werte. Leere Pakete, Artikel mit nicht positivem Gewicht oder nicht positiver Stückzahl und ein nicht positives Gesamtgewicht sind ungültig.

## Lernziele

- Die untersten Komponenten zuerst prüfen und danach schrittweise mit echten Komponenten zusammensetzen.
- An jeder Verbindung zwischen zwei Klassen fachlich aussagekräftige Ergebnisse prüfen.
- Fehler anhand der Teststufe einer Komponente oder ihrer Zusammenarbeit zuordnen.
- Testtreiber und einen vorübergehenden Platzhalter gezielt beim schrittweisen Integrieren einsetzen.

## Vorbereitung

Erstellt ein xUnit- oder MSTest-Projekt unter `tests/BottomUpIntegrationExercise.Tests` und fügt eine Projektreferenz auf `bottom-up-integration/BottomUpIntegrationExercise.csproj` hinzu. Ihr könnt dazu die IDE verwenden oder vom Repository-Stamm aus folgende Befehle ausführen (wählt **eine** der ersten beiden Zeilen):

```bash
dotnet new xunit --name BottomUpIntegrationExercise.Tests --output tests/BottomUpIntegrationExercise.Tests --framework net10.0
# oder: dotnet new mstest --name BottomUpIntegrationExercise.Tests --output tests/BottomUpIntegrationExercise.Tests --framework net10.0
dotnet add tests/BottomUpIntegrationExercise.Tests/BottomUpIntegrationExercise.Tests.csproj reference bottom-up-integration/BottomUpIntegrationExercise.csproj
```

Löscht die erzeugte Beispieldatei `UnitTest1.cs`. Legt für jede Stufe eine eigene Testklasse an. Führt nach jeder Stufe `dotnet test tests/BottomUpIntegrationExercise.Tests/BottomUpIntegrationExercise.Tests.csproj` aus. Nutzt das [AAA-Muster](../AAA_PATTERN.md) und sprechende Testnamen.

## Testtreiber und Platzhalter

Eure Testmethoden sind die **Testtreiber**: Sie bauen die benötigten Komponenten auf, rufen die jeweils höchste integrierte Komponente auf und vergleichen das Ergebnis mit der Erwartung. In Stufe 2 treibt ein Test beispielsweise `ParcelWeightCalculator` mit einem echten `ItemWeightCalculator` an, obwohl `ShippingQuoteService` noch nicht beteiligt ist.

In Stufe 3a ersetzt ein selbst geschriebener **Platzhalter** vorübergehend den Tarifrechner. Er implementiert `IShippingRateCalculator`, gibt einen festgelegten Betrag zurück und merkt sich das übergebene Gewicht. So könnt ihr den bereits integrierten Gewichtspfad bis zum `ShippingQuoteService` prüfen. In Stufe 3b ersetzt ihr den Platzhalter durch den echten `ShippingRateCalculator`. Legt den Platzhalter nur im Testprojekt an; die endgültigen Integrationstests verwenden echte Komponenten.

## Kleine Aufgaben

### 1. Unterste Komponenten prüfen

Testet `ItemWeightCalculator` und `ShippingRateCalculator` zunächst einzeln:

- Zwei Artikel zu je 0,4 kg ergeben 0,8 kg.
- Einzelgewicht 0 kg und Stückzahl 0 führen jeweils zu einer passenden Exception.
- Prüft die Tarife für 1 kg, 1,01 kg, 5 kg und 5,01 kg. Erwartet CHF 5.–, 9.–, 9.– und 15.–.
- Ein Gesamtgewicht von 0 kg ist ungültig.

### 2. Artikelgewichte integrieren

Erstellt einen **echten** `ItemWeightCalculator` und übergebt ihn an `ParcelWeightCalculator`. Testet die Zusammenarbeit der beiden Klassen mit einem Paket aus zwei Artikeln: `2 × 0,4 kg` und `1 × 0,2 kg` ergeben zusammen 1 kg. Prüft zusätzlich ein leeres Paket und einen ungültigen Artikel in einer Liste. Verwendet in dieser Stufe keinen Mock oder Stub.

### 3a. Angebot mit einem Tarif-Platzhalter antreiben

Schreibt im Testprojekt `RecordingRatePlaceholder : IShippingRateCalculator`. `Calculate` soll das erhaltene Gewicht speichern und immer `42m` zurückgeben. Baut einen Testtreiber, der `ShippingQuoteService` mit dem **echten** `ItemWeightCalculator`, dem **echten** `ParcelWeightCalculator` und diesem Platzhalter erstellt. Ruft `CreateQuote` für `2 × 0,4 kg` und `1 × 0,2 kg` auf. Prüft das Angebotsgewicht von 1 kg, den Platzhalterpreis von CHF 42.– und das vom Platzhalter aufgezeichnete Gewicht von 1 kg. Damit prüft ihr die Übergabe an die Tarifberechnung.

### 3b. Den Platzhalter durch den echten Tarifrechner ersetzen

Erstellt `ShippingQuoteService` mit einem echten `ParcelWeightCalculator`, einem echten `ItemWeightCalculator` und einem echten `ShippingRateCalculator`. Prüft **Gesamtgewicht und Preis** des zurückgegebenen `ShippingQuote`:

| Paketinhalt | Gesamtgewicht | Versandkosten |
| --- | ---: | ---: |
| `2 × 0,4 kg` und `1 × 0,2 kg` | 1 kg | CHF 5.– |
| `2 × 0,4 kg` und `1 × 0,3 kg` | 1,1 kg | CHF 9.– |
| `2 × 2 kg` und `1 × 1 kg` | 5 kg | CHF 9.– |
| `2 × 2 kg` und `2 × 1 kg` | 6 kg | CHF 15.– |

Prüft auch, dass ein leeres Paket beim Erstellen eines Angebots abgewiesen wird. Jeder Test baut seine Komponenten und Testdaten neu auf; es werden keine externen Dienste benötigt.

## Grössere Aufgabe für Fortgeschrittene

Erweitert den Dienst um einen optionalen Expresszuschlag von CHF 7.–. Schreibt zuerst einen Integrationstest, der für dasselbe Paket Standard- und Expresspreis prüft. Ergänzt danach die Funktion im Produktionscode. Die Gewichtsberechnung und die Tarifgrenzen sollen unverändert bleiben; testet deshalb auch ein Paket an der 1-kg-Grenze.

## Abgabe und Selbstkontrolle

- Testprojekt mit getrennten Testtreibern für die untersten Komponenten, die Gewichtssumme, das Angebot mit Platzhalter und das vollständige Versandangebot.
- Ein selbst geschriebener Tarif-Platzhalter im Testprojekt, der den erhaltenen Gewichtswert aufzeichnet.
- Mindestens die oben genannten Normal-, Grenz- und Fehlerfälle.
- Alle Tests bestehen mit `dotnet test tests/BottomUpIntegrationExercise.Tests/BottomUpIntegrationExercise.Tests.csproj`.
