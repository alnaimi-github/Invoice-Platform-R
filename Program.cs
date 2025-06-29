using InvoiceProcessing.API.Extensions;
using InvoiceProcessing.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddApplicationServices()
    .AddDatabaseServices()
    .AddAuthenticationServices();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Processing API v1");
        c.RoutePrefix = string.Empty;
    });

    await app.Services.MigrateAndSeedDatabaseAsync();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();