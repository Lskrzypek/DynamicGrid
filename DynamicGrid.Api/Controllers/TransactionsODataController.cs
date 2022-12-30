using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataFull.Data;

namespace ODataFull.Controllers;

public class TransactionsODataController : ODataController
{
    [HttpGet]
    [EnableQuery]
    public IQueryable<Transaction> Get([FromServices] DynamicGridDBContext dbContext)
    {
        return dbContext.Transactions;
    }
}