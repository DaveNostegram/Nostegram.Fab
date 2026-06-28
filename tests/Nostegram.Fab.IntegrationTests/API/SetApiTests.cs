using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Sets;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Nostegram.Fab.Api;

public class SetApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public SetApiTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.SetServiceMock.Reset();

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSet_ReturnsBadRequest_WhenNameAndSetCodeAreEmpty()
    {
        var request = new SetWriteDto("", "", DateOnly.FromDateTime(DateTime.UtcNow));

        var response = await _client.PostAsJsonAsync("/api/sets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));

        validationProblem!.Errors.Should().ContainKey("SetCode");

        validationProblem.Errors["SetCode"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));
    }

    [Fact]
    public async Task CreateSet_ReturnsBadRequest_WhenNameAndSetCodeExceedsMaxLength()
    {
        var request = new SetWriteDto(new string('A', 151), new string('A', 6), DateOnly.FromDateTime(DateTime.UtcNow));

        var response = await _client.PostAsJsonAsync("/api/sets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must be 150 characters or fewer"));

        validationProblem!.Errors.Should().ContainKey("SetCode");

        validationProblem.Errors["SetCode"]
            .Should()
            .Contain(x => x.Contains("must be 3 characters or fewer"));
    }

    [Fact]
    public async Task UpdateSet_ReturnsBadRequest_WhenNameAndSetCodeAreEmpty()
    {
        var publicId = Guid.NewGuid();
        var request = new SetWriteDto("", "", DateOnly.FromDateTime(DateTime.UtcNow));

        var response = await _client.PutAsJsonAsync($"/api/sets/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("Name");

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));

        validationProblem!.Errors.Should().ContainKey("SetCode");

        validationProblem.Errors["SetCode"]
            .Should()
            .Contain(x => x.Contains("must not be empty"));
    }

    [Fact]
    public async Task UpdateSet_ReturnsBadRequest_WhenNameAndSetCodeExceedsMaxLength()
    {
        var publicId = Guid.NewGuid();
        var request = new SetWriteDto(new string('A', 151), new string('A', 6), DateOnly.FromDateTime(DateTime.UtcNow));

        var response = await _client.PutAsJsonAsync($"/api/sets/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem.Errors["Name"]
            .Should()
            .Contain(x => x.Contains("must be 150 characters or fewer"));

        validationProblem!.Errors.Should().ContainKey("SetCode");

        validationProblem.Errors["SetCode"]
            .Should()
            .Contain(x => x.Contains("must be 3 characters or fewer"));
    }
}