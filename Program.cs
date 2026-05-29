using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dataFile = "inventory.json"; // Zapisze prosto w katalogu roboczym (bezpieczniejsze w kontenerach)

// Ujednolicamy format JSON na camelCase, żeby idealnie współpracował z Voiceflow
var jsonOptions = new JsonSerializerOptions 
{ 
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

List<Product> LoadInventory()
{
    if (!File.Exists(dataFile)) return new List<Product>();
    try
    {
        var json = File.ReadAllText(dataFile);
        return JsonSerializer.Deserialize<List<Product>>(json, jsonOptions) ?? new List<Product>();
    }
    catch
    {
        return new List<Product>(); // Zabezpieczenie przed uszkodzonym plikiem
    }
}

void SaveInventory(List<Product> inventory)
{
    var json = JsonSerializer.Serialize(inventory, jsonOptions);
    File.WriteAllText(dataFile, json);
}

app.MapGet("/api/inventory", () =>
{
    return Results.Ok(LoadInventory());
});

app.MapPost("/api/inventory", (Product incoming, ILogger<Program> logger) =>
{
    // Logujemy przychodzące dane! Zajrzyj w zakładkę "Logs" na Renderze po wpisaniu czegoś w bocie.
    logger.LogInformation($"Otrzymano z Voiceflow -> Name: '{incoming.Name}', Quantity: {incoming.Quantity}");

    if (string.IsNullOrWhiteSpace(incoming.Name))
    {
        return Results.BadRequest(new { error = "Pole 'name' jest puste." });
    }

    var inventory = LoadInventory();

    // Używamy OrdinalIgnoreCase - jest wydajniejsze pamięciowo niż ToLower() i chroni przed nullami
    var existing = inventory.FirstOrDefault(p => 
        string.Equals(p.Name, incoming.Name, StringComparison.OrdinalIgnoreCase));

    if (existing != null)
    {
        existing.Quantity += incoming.Quantity;
        if (existing.Quantity < 0) existing.Quantity = 0;
    }
    else if (incoming.Quantity > 0)
    {
        inventory.Add(incoming);
    }

    SaveInventory(inventory);
    return Results.Ok(inventory);
});

app.Run();

public class Product
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
