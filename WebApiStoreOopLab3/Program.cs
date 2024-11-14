using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebApiStoreOopLab3.BLL;
using WebApiStoreOopLab3.DAL;
using WebApiStoreOopLab3.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var configuration = builder.Configuration;

if (configuration["DataAccess:Type"] == "Database")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IStoreRepository, DbStoreRepository>();
}
else
{
    builder.Services.AddScoped<IStoreRepository>(sp =>
        new CsvStoreRepository("CSV/stores.csv", "CSV/inventory.csv"));
}

builder.Services.AddScoped<StoreService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}