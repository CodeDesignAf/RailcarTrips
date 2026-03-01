using Microsoft.EntityFrameworkCore;
using RailcarTrips.Application.UseCases;
using RailcarTrips.Infrastructure;
using RailcarTrips.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ImportEquipmentEventsUseCase>();
builder.Services.AddScoped<IGetTripsQuery, GetTripsQuery>();
builder.Services.AddScoped<IGetTripEventsQuery, GetTripEventsQuery>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedCitiesIfEmptyAsync(dbContext, app.Environment.ContentRootPath);

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ClientCors");
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok("ok"))
    .WithName("Health");

app.Run();

public partial class Program;
