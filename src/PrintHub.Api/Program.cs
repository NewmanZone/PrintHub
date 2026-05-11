var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "printhub-api" }));
app.MapGet("/", () => Results.Ok(new { service = "PrintHub API", version = "1.0.0" }));

app.Run();
