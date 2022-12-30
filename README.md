# Dynamiczny Grid - Blazor/Radzen/EF/OData
## Problem
Załóżmy, że w aplikacji webowej musisz utworzyć grida wyświetlającego rekordy z tabeli w bazie danych. Grid musi obsługiwać stronicowanie, a także mieć możliwość sortowania i filtrowania po każdej z kolumn. Jest jeszcze jeden warunek: z bazy danych mają być odczytywane tylko te rekordy, które w danym momencie potrzebujemy. Informacje o sortowaniu i zakresie danych musimy więc w jakiś sposób przekazać przez API, a na końcu przetworzyć na zapytanie do bazy danych.
## Rozwiązanie
Poniższe rozwiązanie używa takich technologii jak:
- Blazor – jako frontend
- Radzen – darmowe kontrolki, w tym Grid
- Entity Framework – odczyt z bazy danych
- OData – Microsoftowy protokół sieciowy

Na szczególną uwagę zasługuje tutaj OData. Jest to rozwijany przez Microsoft protokół, który umożliwia łatwe przesyłanie danych przez API. Wprowadza on swego rodzaju język zapytań, które można kierować do API.

Na przykład wysłanie zapytania:
```http://host/service/Products?$filter=Name eq 'Milk'```
informuje serwer API, że chcemy produkty z nazwą równą ‘Milk’

Natomiast zapytanie:
```http://host/service/Products?$filter=Name eq 'Milk'?$orderby=ReleaseDate```
informuje serwer API, że chcemy produkty z nazwą równą ‘Milk’ ale jednocześnie posortowane po ReleaseDate.

Język tych zapytań jest opisany tutaj:
https://learn.microsoft.com/en-us/odata/concepts/queryoptions-overview
