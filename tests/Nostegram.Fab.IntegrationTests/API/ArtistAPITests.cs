using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Contracts.Common;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Nostegram.Fab.Api;

public class ArtistApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ArtistApiTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.ArtistServiceMock.Reset();

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetArtist_ReturnsOk_WhenServiceReturnsArtist()
    {
        var artistId = Guid.NewGuid();

        var expected = new LookupItemDto(artistId, "Test Artist");

        _factory.ArtistServiceMock
            .Setup(x => x.GetArtist(artistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await _client.GetAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var artist = await response.Content.ReadFromJsonAsync<LookupItemDto>();

        artist.Should().NotBeNull();
        artist!.PublicId.Should().Be(artistId);
        artist.Name.Should().Be("Test Artist");
    }

    [Fact]
    public async Task GetArtist_ReturnsNotFound_WhenServiceThrowsNotFoundException()
    {
        var artistId = Guid.NewGuid();

        _factory.ArtistServiceMock
            .Setup(x => x.GetArtist(artistId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(artistId.ToString()));

        var response = await _client.GetAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
        problem.Title.Should().Be("Not found.");
    }

    [Fact]
    public async Task GetArtist_WithBadGuid_ReturnsNotFound_BecauseRouteDoesNotMatch()
    {
        var response = await _client.GetAsync("/api/artists/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _factory.ArtistServiceMock.Verify(
            x => x.GetArtist(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllArtists_ReturnsOk_WhenServiceReturnsArtists()
    {
        var artists = new List<LookupItemDto>
        {
            new(Guid.NewGuid(), "Artist One"),
            new(Guid.NewGuid(), "Artist Two"),
        };

        _factory.ArtistServiceMock
            .Setup(x => x.GetAllArtists(It.IsAny<CancellationToken>()))
            .ReturnsAsync(artists);

        var response = await _client.GetAsync("/api/artists");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<LookupItemDto>>();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result!.Select(x => x.Name).Should().Contain(["Artist One", "Artist Two"]);
    }

    [Fact]
    public async Task CreateArtist_ReturnsCreated_WhenServiceReturnsGuid()
    {
        var artistId = Guid.NewGuid();

        var request = new LookupItemWriteDto("New Artist");

        _factory.ArtistServiceMock
            .Setup(x => x.CreateArtist(
                It.Is<LookupItemWriteDto>(dto => dto.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LookupItemDto(artistId, request.Name));

        var response = await _client.PostAsJsonAsync("/api/artists", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var artist = await response.Content.ReadFromJsonAsync<LookupItemDto>();

        artist.Should().NotBeNull();
        artist!.PublicId.Should().Be(artistId);
        artist.Name.Should().Be(request.Name);

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateArtist_ReturnsConflict_WhenServiceThrowsAlreadyExistsException()
    {
        var request = new LookupItemWriteDto("Existing Artist");

        _factory.ArtistServiceMock
            .Setup(x => x.CreateArtist(It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AlreadyExistsException("Name", request.Name));

        var response = await _client.PostAsJsonAsync("/api/artists", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.Conflict);
        problem.Title.Should().Be("Resource already exists.");
        problem.Detail.Should().Be("Name 'Existing Artist' already exists.");
    }

    [Fact]
    public async Task DeleteArtist_ReturnsNoContent_WhenServiceSucceeds()
    {
        var artistId = Guid.NewGuid();

        _factory.ArtistServiceMock
            .Setup(x => x.DeleteArtist(artistId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteArtist_ReturnsNotFound_WhenServiceThrowsNotFoundException()
    {
        var artistId = Guid.NewGuid();

        _factory.ArtistServiceMock
            .Setup(x => x.DeleteArtist(artistId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Artist: {artistId}"));

        var response = await _client.DeleteAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
        problem.Title.Should().Be("Not found.");
    }

    [Fact]
    public async Task DeleteArtist_ReturnsConflict_WhenServiceThrowsConflictException()
    {
        var artistId = Guid.NewGuid();

        _factory.ArtistServiceMock
            .Setup(x => x.DeleteArtist(artistId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Test Artist", "Card"));

        var response = await _client.DeleteAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.Conflict);
        problem.Title.Should().Be("Conflict prevented action.");
        problem.Detail.Should().Be("'Test Artist' is used by a 'Card'.");
    }

    [Fact]
    public async Task AnyEndpoint_ReturnsInternalServerError_WhenServiceThrowsUnexpectedException()
    {
        var artistId = Guid.NewGuid();

        _factory.ArtistServiceMock
            .Setup(x => x.GetArtist(artistId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong."));

        var response = await _client.GetAsync($"/api/artists/{artistId}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problem.Title.Should().Be("An unexpected error occurred.");
    }

    [Fact]
    public async Task CreateArtist_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var request = new LookupItemWriteDto("");

        var response = await _client.PostAsJsonAsync("/api/artists", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));
    }

    [Fact]
    public async Task CreateArtist_ReturnsBadRequest_WhenNameExceedsMaxLength()
    {
        var request = new LookupItemWriteDto(new string('A', 151));

        var response = await _client.PostAsJsonAsync("/api/artists", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must be 150 characters or fewer"));

    }

    [Fact]
    public async Task UpdateArtist_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var publicId = Guid.NewGuid();
        var request = new LookupItemWriteDto("");

        var response = await _client.PutAsJsonAsync($"/api/artists/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));
    }

    [Fact]
    public async Task UpdateArtist_ReturnsBadRequest_WhenNameExceedsMaxLength()
    {
        var publicId = Guid.NewGuid();
        var request = new LookupItemWriteDto(new string('A', 151));

        var response = await _client.PutAsJsonAsync($"/api/artists/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task UpdateArtist_WithBadGuid_ReturnsNotFound_BecauseRouteDoesNotMatch()
    {
        var publicId = "Bad data";
        var request = new LookupItemWriteDto("Good data");

        var response = await _client.PutAsJsonAsync($"/api/artists/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task UpdateArtist_ReturnsOk_WhenServiceReturnsUpdatedArtist()
    {
        var artistId = Guid.NewGuid();
        var request = new LookupItemWriteDto("Updated Artist");
        var expected = new LookupItemDto(artistId, request.Name);

        _factory.ArtistServiceMock
            .Setup(x => x.UpdateArtist(
                artistId,
                It.Is<LookupItemWriteDto>(dto => dto.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await _client.PutAsJsonAsync($"/api/artists/{artistId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var artist = await response.Content.ReadFromJsonAsync<LookupItemDto>();

        artist.Should().NotBeNull();
        artist!.PublicId.Should().Be(artistId);
        artist.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateArtist_ReturnsConflict_WhenServiceThrowsAlreadyExistsException()
    {
        var artistId = Guid.NewGuid();
        var request = new LookupItemWriteDto("Existing Artist");

        _factory.ArtistServiceMock
            .Setup(x => x.UpdateArtist(artistId, It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AlreadyExistsException("Name", request.Name));

        var response = await _client.PutAsJsonAsync($"/api/artists/{artistId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.Conflict);
        problem.Title.Should().Be("Resource already exists.");
        problem.Detail.Should().Be("Name 'Existing Artist' already exists.");
    }

    [Fact]
    public async Task UpdateArtist_ReturnsNotFound_WhenServiceThrowsNotFoundException()
    {
        var artistId = Guid.NewGuid();
        var request = new LookupItemWriteDto("Missing Artist");

        _factory.ArtistServiceMock
            .Setup(x => x.UpdateArtist(artistId, It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Artist: {artistId}"));

        var response = await _client.PutAsJsonAsync($"/api/artists/{artistId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
        problem.Title.Should().Be("Not found.");
    }

    [Fact]
    public async Task GetAllArtists_ReturnsOkWithEmptyList_WhenServiceReturnsNoArtists()
    {
        _factory.ArtistServiceMock
            .Setup(x => x.GetAllArtists(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/api/artists");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<LookupItemDto>>();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteArtist_WithBadGuid_ReturnsNotFound_BecauseRouteDoesNotMatch()
    {
        var response = await _client.DeleteAsync("/api/artists/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _factory.ArtistServiceMock.Verify(
            x => x.DeleteArtist(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateArtist_ReturnsBadRequest_WhenBodyIsMalformedJson()
    {
        using var content = new StringContent(
            "{ \"name\": ",
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/artists", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _factory.ArtistServiceMock.Verify(
            x => x.CreateArtist(It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateArtist_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        using var content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/artists", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _factory.ArtistServiceMock.Verify(
            x => x.CreateArtist(It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateArtist_ReturnsBadRequest_WhenBodyIsMalformedJson()
    {
        var artistId = Guid.NewGuid();

        using var content = new StringContent(
            "{ \"name\": ",
            Encoding.UTF8,
            "application/json");

        var response = await _client.PutAsync($"/api/artists/{artistId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _factory.ArtistServiceMock.Verify(
            x => x.UpdateArtist(It.IsAny<Guid>(), It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateArtist_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var artistId = Guid.NewGuid();

        using var content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json");

        var response = await _client.PutAsync($"/api/artists/{artistId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _factory.ArtistServiceMock.Verify(
            x => x.UpdateArtist(It.IsAny<Guid>(), It.IsAny<LookupItemWriteDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}