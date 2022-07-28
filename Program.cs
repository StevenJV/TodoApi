var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
int min = 1;
int max = 5;


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var forecast =  Enumerable.Range(min, max+1).Select(index =>
    new WeatherForecast
    (
        DateTime.Now.AddDays(index),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
    ))
    .ToArray();

app.MapGet("/", () => "Hello World!");

app.MapGet("/weatherforecast", () =>
{
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/weatherforecast/{id}", (int id) =>
{
    if (id < min || id > max) { return null; }
    return forecast[id];
})
.WithName("GetSpecificWeatherForecast");

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}