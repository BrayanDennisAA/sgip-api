using Sgip.Infrastructure.Data;
using Sgip.WebApi;
using Sgip.WebApi.Common;
using Sgip.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebServices(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Composition root: le damos al factory estático acceso al accessor ya resuelto
ApiProblemDetailsFactory.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// --- Migraciones automáticas + seed data al arrancar ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData.ApplyAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGIP API v1");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();