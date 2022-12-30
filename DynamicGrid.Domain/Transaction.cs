namespace Domain;

public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int SourceAccount { get; set; }
    public int DestinationAccount { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
}
    
