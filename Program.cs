var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Ścieżka do pliku z danymi
var dataFile = "/app/data/inventory.json";
Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!);

// Funkcje do odczytu i zapisu
List<Product> LoadInventory()
{
    if (!File.Exists(dataFile)) return new List<Product>();
    var json = File.ReadAllText(dataFile);
    return System.Text.Json.JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
}

void SaveInventory(List<Product> inventory)
{
    var json = System.Text.Json.JsonSerializer.Serialize(inventory);
    File.WriteAllText(dataFile, json);
}

// GET
app.MapGet("/api/inventory", () =>
{
    return Results.Ok(LoadInventory());
});

// POST
app.MapPost("/api/inventory", (Product incoming) =>
{
    var inventory = LoadInventory();
    var existing = inventory.FirstOrDefault(p => p.Name.ToLower() == incoming.Name.ToLower());
    if (existing != null)
    {
        existing.Quantity += incoming.Quantity;
        if (existing.Quantity < 0) existing.Quantity = 0;
    }
    else
    {
        if (incoming.Quantity > 0)
            inventory.Add(incoming);
    }
    SaveInventory(inventory);
    return Results.Ok(inventory);
});

app.Run();

public class Product
{
    public string Name { get; set; }
    public int Quantity { get; set; }
}
