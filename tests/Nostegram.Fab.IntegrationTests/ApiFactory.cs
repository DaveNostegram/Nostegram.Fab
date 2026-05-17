using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;

namespace Nostegram.Fab.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public Mock<IArtistService> ArtistServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IArtistService>();

            services.AddSingleton(ArtistServiceMock.Object);
        });
    }
}