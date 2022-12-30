using Microsoft.EntityFrameworkCore;
using ODataFull.Data;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Domain;

var builder = WebApplication.CreateBuilder(args);

// Entity Framework
builder.Services.AddDbContext<DynamicGridDBContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DynamicGrid")));

// Odata
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

builder.Services.AddCors();
builder.Services.AddScoped<TransactionsGenerator>();

var app = builder.Build();

app.MapControllers();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) 
    .AllowCredentials());


// Data initializing
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DynamicGridDBContext>()
        .Database.Migrate();

    scope.ServiceProvider.GetRequiredService<TransactionsGenerator>()
        .GenerateTransactions(transactionsCount: 1000);
}

app.Run();
