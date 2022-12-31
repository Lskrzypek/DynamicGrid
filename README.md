# Dynamiczny Grid - Blazor/Radzen/EF/OData
## Problem
Załóżmy, że w aplikacji webowej musisz utworzyć grida wyświetlającego rekordy z tabeli w bazie danych. Chcemy, żeby grid spełniał następujace warunki:
- grid musi obsługiwać stronicowanie
- grid musi mieć możliwość sortowania i filtrowania po każdej z kolumn
- z bazy danych mają być odczytywane tylko te rekordy, które w danym momencie potrzebujemy. Informacje o sortowaniu i zakresie danych musimy więc w jakiś sposób przekazać przez API, a na końcu przetworzyć na zapytanie do bazy danych.
## Rozwiązanie
Poniżej prezentuję przykładowe rozwiązanie. Używam takich technologii jak:
- Blazor – jako frontend
- Radzen – darmowe kontrolki, w tym Grid
- Entity Framework – odczyt z bazy danych
- OData – Microsoftowy protokół sieciowy

Na szczególną uwagę zasługuje tutaj OData i na nim się skupię. Jest to rozwijany przez Microsoft protokół, który umożliwia łatwe przesyłanie danych przez API. Wprowadza on swego rodzaju język zapytań, które można kierować do API.

Na przykład wysłanie zapytania:
```http://host/service/Products?$filter=Name eq 'Milk'```
informuje serwer API, że chcemy produkty z nazwą równą ‘Milk’

Natomiast zapytanie:
```http://host/service/Products?$filter=Name eq 'Milk'?$orderby=ReleaseDate```
informuje serwer API, że chcemy produkty z nazwą równą ‘Milk’ ale jednocześnie posortowane po ReleaseDate.

Język tych zapytań jest opisany tutaj:
https://learn.microsoft.com/en-us/odata/concepts/queryoptions-overview
### Domena
W przykładowym programie będziemy chcieli wyświetlić Grida z transakcjami. Nasze transakcje będą zawierać informacje o dacie, kwocie, opisie i dwóch kontach - jedno z którego przelewamy pieniądze i drugie na które robimy przelew. Domena jest więc bardzo prosta. Stanowi ją jedna klasa Transaction:
```
public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int SourceAccount { get; set; }
    public int DestinationAccount { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
}
```
### API
Żeby skorzystać z dobrodziejstw OData potrzebujemy zainstalować paczkę nugetową:
```
Microsoft.AspNetCore.OData
```
Druga rzecz, to utworzenie kontrolera:
```
public class TransactionsODataController : ODataController
{
    [HttpGet]
    [EnableQuery]
    public IQueryable<Transaction> Get([FromServices] DynamicGridDBContext dbContext)
    {
        return dbContext.Transactions;
    }
}
```
Kontroler ten różni się od zwykłego kontrolera kilkoma rzeczami. Przede wszystkim dziedziczy po klasie ODataController zamiast ControllerBase i w ten sposób informuje, że będzie używał protokółu OData. Druga rzecz, to atrybut [EnableQuery] - wskazujemy w ten sposób, że endpoint może być odpytywany przez składnię OData. Jeśli chodzi o wnętrze metody, to jest ono bardzo proste. Po prostu zwracamy konkretny DataSet z naszego DBContextu Entity Framework. Co ważne jako wynik możemy zwrócić interfejs IQueryable.

Aby kontroler mógł działać, musimy zarejestrować go w Program.cs:
```
builder.Services
    .AddControllers()
    .AddOData(options => options
        .EnableQueryFeatures()
        .AddRouteComponents("/", GetEdmModel()));

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Transaction>("TransactionsOData");
    return builder.GetEdmModel();
}
```

Po uruchomieniu, w naszym API możemy budować zapytania OData.
### Web Client
Utworzyłem aplikację Blazor z szablonu. Następnie usunąłem wszystko, co jest związane z przykładową implementacją Microsoftu, żeby maksymalnie uprościć nasz program. Zostawiłem jedynie stronę Index.razor.

Zainstalowałem następującą paczkę nuget:

```Radzen.Blazor```

W pliku index.razor umieściłem kontrolkę RadzenDataGrid. Więcej można o niej przeczytać w dokumentacji Radzen: https://blazor.radzen.com/datagrid

```
<RadzenDataGrid Data="@transactions" AllowSorting="true" AllowFiltering="true" AllowPaging="true" PageSize="5" 
    LoadData="@LoadData" Count="@allTransactionsCount" FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive" 
    FilterMode="FilterMode.Advanced" >
    <Columns>
        <RadzenDataGridColumn TItem="Transaction" Property="Date" Title="Date" FormatString="{0:yyyy-MM-dd}" />
        <RadzenDataGridColumn TItem="Transaction" Property="SourceAccount" Title="Source Account" />
        <RadzenDataGridColumn TItem="Transaction" Property="DestinationAccount" Title="Destination Account" />
        <RadzenDataGridColumn TItem="Transaction" Property="Amount" Title="Amount"/>
        <RadzenDataGridColumn TItem="Transaction" Property="Description" Title="Description" />
    </Columns>
</RadzenDataGrid>


@code {
    IEnumerable<Transaction>? transactions;
    int allTransactionsCount;

    async Task LoadData(LoadDataArgs args)
    {
        var transactionsResult = await dataLoader.GetTransactionsWithOData(args.Filter, args.Top, args.Skip, args.OrderBy);
        
        transactions = transactionsResult.Value.AsODataEnumerable();
        allTransactionsCount = transactionsResult.Count;
    }
}
```
Kilka rzeczy zasługuje tutaj na uwagę:
- transactions - zmienna, która wskazuje na transakcje, wyświetlane na Gridzie
- allTransactionsCount - ilość wszystkich transakcji w bazie danych. Ta liczba różni się od transactions.Count(). Tak jak pisałem na początku, grida chcemy zasilać tylko tymi danymi, które w tym momęcie potrzebujemy. Będzie to wynikało ze stronicowania, filtrowania i sortowania. Natomiast allTransactionsCount to liczba wszystkich transakcji z bazy danych i ją trzeba ustawić oddzielnie.
- LoadData - ta metoda będzie wywoływana za każdym razem, kiedy grid będzie potrzebował dane. Czyli przy przejściu na inną stronę, albo przy zmianie sortowania czy filtrowania.

Sam odczyt danych z API znajduje się w pliku DataLoader.cs:
```
public class DataLoader
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DataLoader(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ODataServiceResult<Transaction>> GetTransactionsWithOData(string? filter, int? top, int? skip, string? orderby)
    {
        var uri = new Uri("https://localhost:7234/TransactionsOData")
            .GetODataUri(filter, top, skip, orderby, expand: null, select: null, count: true);

        var response = await _httpClientFactory.CreateClient().GetAsync(uri);

        return await response.ReadAsync<ODataServiceResult<Transaction>>();
    }
}
```
Kontrolki Radzen świetnie współpracują z OData. Biblioteka Radzen zawiera między innymi rozszerzenie klasy Uri na metode GetODataUri. Konwertuje ona sortowania i filtrowanie Radzenowe na składnię OData. Dodatkowo wprowadza typ ODataServiceResult, który przechowuje zarówno odczytane z API wiersze, jak i całkowitą ilość wierszy.

