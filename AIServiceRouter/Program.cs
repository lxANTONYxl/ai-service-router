using System.Text.Json;
using AIServiceRouter.Services;
using AIServiceRouter.Types;

var builder = WebApplication.CreateBuilder(args);

// Add services to DI
builder.Services.AddSingleton<GroqService>();
builder.Services.AddSingleton<CerebrasService>();

var app = builder.Build();

// Initialize services list
var services = new List<IAIService>
{
    app.Services.GetRequiredService<GroqService>(),
    app.Services.GetRequiredService<CerebrasService>()
};

int currentServiceIndex = 0;

IAIService GetNextService()
{
    var service = services[currentServiceIndex];
    currentServiceIndex = (currentServiceIndex + 1) % services.Count;
    return service;
}

app.MapPost("/chat", async (HttpContext context) =>
{
    ChatRequest? requestBody;
    try
    {
        requestBody = await JsonSerializer.DeserializeAsync<ChatRequest>(context.Request.Body);
    }
    catch
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid JSON");
        return;
    }
    
    if (requestBody?.Messages == null || requestBody.Messages.Count == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Messages are required");
        return;
    }

    var service = GetNextService();
    Console.WriteLine($"Using service: {service.Name}");

    var stream = await service.ChatAsync(requestBody.Messages);

    context.Response.Headers["Content-Type"] = "text/event-stream";
    context.Response.Headers["Cache-Control"] = "no-cache";
    context.Response.Headers["Connection"] = "keep-alive";

    await foreach (var chunk in stream)
    {
        await context.Response.WriteAsync(chunk);
        await context.Response.Body.FlushAsync();
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Run($"http://localhost:{port}");