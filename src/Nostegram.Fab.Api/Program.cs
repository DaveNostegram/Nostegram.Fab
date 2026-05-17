using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Api.Filters;
using Nostegram.Fab.Api.Middleware;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Validators;
using Nostegram.Fab.Application.ReferenceData.Artists;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Infrastructure.Persistence;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FabDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICommit>(sp => sp.GetRequiredService<FabDbContext>());
builder.Services.AddScoped<IArtistService, ArtistService>();

builder.Services.AddScoped<IArtistRepository, ArtistRepository>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
});

builder.Services.AddValidatorsFromAssemblyContaining<LookupItemWriteDtoValidator>();
builder.Services.AddScoped<FluentValidationFilter>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Needed for WebApplicationFactory<Program>
public partial class Program { }
