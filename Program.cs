using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connectionString = "Host=aws-1-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.aiamnjqleyejujwrglxt;Password=janPawel2137!;SSL Mode=Require;Trust Server Certificate=true";

app.MapGet("/api/inventory", async () =>
{
    var products = new List<Product>();
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand("SELECT name, quantity FROM inventory", conn);
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        products.Add(new Product { Name = reader.GetString(0), Quantity = reader.GetInt32(1) });
    return Results.Ok(products);
});

app.MapPost("/api/inventory", async (Product incoming) =>
{
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand(@"
        INSERT INTO inventory (name, quantity) 
        VALUES (@name, @qty)
        ON CONFLICT (name) 
        DO UPDATE SET quantity = GREATEST(0, inventory.quantity + @qty)
    ", conn);
    cmd.Parameters.AddWithValue("name", incoming.Name);
    cmd.Parameters.AddWithValue("qty", incoming.Quantity);
    await cmd.ExecuteNonQueryAsync();

    var products = new List<Product>();
    await using var cmd2 = new NpgsqlCommand("SELECT name, quantity FROM inventory", conn);
    await using var reader = await cmd2.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        products.Add(new Product { Name = reader.GetString(0), Quantity = reader.GetInt32(1) });
    return Results.Ok(products);
});

app.Run();

public class Product
{
    public string Name { get; set; }
    public int Quantity { get; set; }
}
