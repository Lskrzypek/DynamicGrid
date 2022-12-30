# Dynamiczny Grid - Blazor/Radzen/EF/OData
## Problem
Załóżmy, że w aplikacji webowej musisz utworzyć grida wyświetlającego rekordy z tabeli w bazie danych. Grid musi obsługiwać stronicowanie, a także mieć możliwość sortowania i filtrowania po każdej z kolumn. Jest jeszcze jeden warunek: z bazy danych mają być odczytywane tylko te rekordy, które w danym momencie potrzebujemy. Informacje o sortowaniu i zakresie danych musimy więc w jakiś sposób przekazać przez API, a na końcu przetworzyć na zapytanie do bazy danych.
### Rozwiązanie
Poniższe rozwiązanie używa takich technologii jak:
- Blazor – jako frontend
- Radzen – darmowe kontrolki, w tym Grid
- Entity Framework – odczyt z bazy danych
- OData – Microsoftowy protokół sieciowy
