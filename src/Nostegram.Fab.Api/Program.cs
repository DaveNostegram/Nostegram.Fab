using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Api.Filters;
using Nostegram.Fab.Api.Middleware;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Validators;
using Nostegram.Fab.Application.ReferenceData.Artists;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardTypes;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.FabClasses;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Sets;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Talents;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Infrastructure.Persistence;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FabDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICommit>(sp => sp.GetRequiredService<FabDbContext>());
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ICardSubTypeService, CardSubTypeService>();
builder.Services.AddScoped<ICardTypeService, CardTypeService>();
builder.Services.AddScoped<IFabClassService, FabClassService>();
builder.Services.AddScoped<ITalentService, TalentService>();
builder.Services.AddScoped<ISetService, SetService>();

builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<ICardSubTypeRepository, CardSubTypeRepository>();
builder.Services.AddScoped<ICardTypeRepository, CardTypeRepository>();
builder.Services.AddScoped<IFabClassRepository, FabClassRepository>();
builder.Services.AddScoped<ITalentRepository, TalentRepository>();
builder.Services.AddScoped<ISetRepository, SetRepository>();

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
