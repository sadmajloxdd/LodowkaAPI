var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var inventory = new List<Product>();

app.MapGet("/api/inventory", () =>
{
    return Results.Ok(inventory);
});

app.MapPost("/api/inventory", (Product incoming) =>
{
    var existing = inventory.FirstOrDefault(p => p.Name.ToLower() == incoming.Name.ToLower());

    if (existing != null)
    {
        existing.Quantity += incoming.Quantity; 
    }
    else
    {
        inventory.Add(incoming); 
    }

    return Results.Ok(inventory);
});

app.Run();

public class Product
{
    public string Name { get; set; }
    public int Quantity { get; set; }
}