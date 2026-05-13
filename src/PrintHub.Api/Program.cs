var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "printhub-api" }));
app.MapGet("/", () => Results.Ok(new { service = "PrintHub API", version = "1.0.0" }));

app.Run();
