using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connectionString = "Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.aiamnjqleyejujwrglxt;Password=janPawel2137!;SSL Mode=Require;Trust Server Certificate=true";

app.MapGet("/api/inventory", async () =>
{
    var products = new System.Collections.Generic.List<Product>();
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand("SELECT name, quantity FROM inventory", conn);
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        products.Add(new Product { Name = reader.GetString(0), Quantity = reader.GetInt32(1) });
    return Results.Ok(products);
});

app.MapPost("/api/inventory", async (System.Collections.Generic.List<Product> incoming) =>
{
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    foreach (var item in incoming)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO inventory (name, quantity) 
            VALUES (@name, @qty)
            ON CONFLICT (name) 
            DO UPDATE SET quantity = GREATEST(0, inventory.quantity + @qty)
        ", conn);
        cmd.Parameters.AddWithValue("name", item.Name);
        cmd.Parameters.AddWithValue("qty", item.Quantity);
        await cmd.ExecuteNonQueryAsync();
    }
    var products = new System.Collections.Generic.List<Product>();
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
