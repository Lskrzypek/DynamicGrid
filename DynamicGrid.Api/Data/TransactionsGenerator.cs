using Domain;
using Microsoft.EntityFrameworkCore;

namespace ODataFull.Data;

public class TransactionsGenerator
{
    private readonly DynamicGridDBContext _dbContext;

    public TransactionsGenerator(DynamicGridDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void GenerateTransactions(int transactionsCount)
    {
        _dbContext.Transactions.ExecuteDelete();

        _dbContext.AddRange(CreateTransactions(transactionsCount));
        _dbContext.SaveChanges();
    }

    private IEnumerable<Transaction> CreateTransactions(int transactionsCount)
    {
        var random = new Random();

        var account = new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000 };
        
        return Enumerable
            .Range(0, transactionsCount)
            .Select(_ => new Transaction()
            {
                Date = DateTime.Now.AddDays(-random.Next(1000)),
                Amount = Math.Round(1000000M * (decimal)random.NextDouble(), 2),
                Description = random.Next(10) == 1 ? "Cancelled" : "",
                SourceAccount = account.Skip(random.Next(account.Length)).First(),
                DestinationAccount = account.Skip(random.Next(account.Length)).First()
            });
    }
}
