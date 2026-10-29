# Antworten Test Doubles

Ich habe die Aufgabe mit xUnit gemacht (`tests/TestDoublesExercise.Tests`). Meine eigenen Doubles sind im Ordner `TestDoubles`.

## Aufgabe 1

Der ReservationService bekommt `ISeatAvailability` im Konstruktor und arbeitet nur mit dem Interface. Darum kann ich im Test einfach eine eigene Klasse übergeben, die das Interface implementiert, auch wenn es noch keine echte Implementierung gibt.

Aus dem Interface weiss man nur, dass man einen Veranstaltungscode übergibt und eine Zahl zurückbekommt. Nicht klar ist zum Beispiel, was bei einem unbekannten Code passiert oder ob die Zahl auch negativ sein kann.

Meine Annahme: `GetAvailableSeats` gibt zurück, wie viele Plätze für diese Veranstaltung gerade noch frei sind. Die Zahl ist nie negativ und bei einem unbekannten Code ist sie 0.

## Aufgabe 2

Bei diesen Tests übergebe ich für alle drei Abhängigkeiten einen Spy. Die Spies speichern jeden Aufruf in einer Liste. Nach der Exception prüfe ich, ob alle Listen leer sind. Dann weiss ich, dass nichts aufgerufen wurde, und am Produktionscode musste ich nichts ändern.

## Aufgabe 3

Der Stub gibt dem Service nur Werte zurück, die ich vorher festlege, der Spy speichert dagegen, womit er aufgerufen wurde, damit ich das nachher im Test prüfen kann.

## Aufgabe 5

Für die Verfügbarkeit und den Sender habe ich meine eigenen Spies genommen, weil ich die schon hatte und man mit `Assert.Empty` gut sieht, dass nichts aufgerufen wurde. Beim Repository habe ich Moq genommen. `TrySave` darf hier gar nicht aufgerufen werden, deshalb muss ich keinen Rückgabewert festlegen und kann direkt mit `Times.Never()` prüfen.

## Zusatzaufgabe: Send wirft eine Exception

Wenn `Send` nach dem Speichern eine Exception wirft, ist die Reservierung schon gespeichert, aber der Kunde bekommt keine Bestätigung. Bei `ReserveBatch` kann es sein, dass ein Teil der Leute eine Bestätigung bekommt und der Rest nicht. Der Aufrufer bekommt eine Exception und denkt vielleicht, dass die Reservierung nicht geklappt hat. Wenn er es dann nochmal probiert, sind die Plätze doppelt reserviert.

Eine Lösung wäre, dass die Reservierung schon als erfolgreich gilt, sobald sie gespeichert ist. Wenn die Bestätigung nicht verschickt werden kann, merkt man sich das und schickt sie später nochmal. Wenn es gar nicht klappt, meldet sich jemand vom Support beim Kunden.
