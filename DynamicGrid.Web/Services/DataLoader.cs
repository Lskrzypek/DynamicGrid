using Domain;
using Radzen;

namespace DynamicGrid.Web.Services;

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
