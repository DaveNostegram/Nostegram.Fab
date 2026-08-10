using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Cards;
using Xunit;
using Nostegram.Fab.Contracts.Sets;

namespace Nostegram.Fab.IntegrationTests.Nostegram.Fab.Api;

public class CardApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public CardApiTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.CardServiceMock.Reset();

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCard_ReturnsCreated_WhenServiceReturnsCard()
    {
        var fullCardDto = new CardDto
        {
            PublicId = Guid.NewGuid(),
            Name = "Card 1",
            CardVariants = [
                new CardVariantDto {
                    PublicId = Guid.NewGuid(),
                    Pitch = Contracts.PitchEnumAPI.Blue,
                    SetDetails = [
                        new SetDetailDto {
                            PublicId = Guid.NewGuid(),
                            Rarity = Contracts.RarityEnumAPI.Common,
                            CollectorNumber = "001",
                            Set = new SetDto(Guid.NewGuid(), "set", "SetDetail", DateOnly.FromDateTime(DateTime.Now)),
                            Artist = new LookupItemDto(Guid.NewGuid(), "Name")
                        }
                    ]
                },
                new CardVariantDto {
                    PublicId = Guid.NewGuid(),
                    Pitch = Contracts.PitchEnumAPI.Red,
                    SetDetails = [
                        new SetDetailDto {
                            PublicId = Guid.NewGuid(),
                            Rarity = Contracts.RarityEnumAPI.Common,
                            CollectorNumber = "001",
                            Set = new SetDto(Guid.NewGuid(), "set", "SetDetail", DateOnly.FromDateTime(DateTime.Now)),
                            Artist = new LookupItemDto(Guid.NewGuid(), "Name")
                        }
                    ]
                },
            ]
        };


        var request = new CardWriteDto
        {
            Name = "Card 1",
            CardVariants = [
                new CardVariantWriteDto {
                    Pitch = Contracts.PitchEnumAPI.Blue,
                    SetDetails = [
                        new SetDetailWriteDto {
                            Rarity = Contracts.RarityEnumAPI.Common,
                            CollectorNumber = "001",
                            SetId = Guid.NewGuid(),
                            ArtistId = Guid.NewGuid()
                        }
                    ]
                },
                new CardVariantWriteDto {
                    Pitch = Contracts.PitchEnumAPI.Red,
                    SetDetails = [
                        new SetDetailWriteDto {
                            Rarity = Contracts.RarityEnumAPI.Common,
                            CollectorNumber = "001",
                            SetId = Guid.NewGuid(),
                            ArtistId = Guid.NewGuid()
                        }
                    ]
                },
            ]
        };

        _factory.CardServiceMock
            .Setup(x => x.CreateCard(
                It.Is<CardWriteDto>(dto => dto.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fullCardDto);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var card = await response.Content.ReadFromJsonAsync<CardDto>();

        card.Should().NotBeNull();
        card!.PublicId.Should().Be(fullCardDto.PublicId);
        card.Name.Should().Be(request.Name);

    }

    [Fact]
    public async Task CreateCard_ReturnsBadRequest_WhenNoVariants()
    {
        var request = new CardWriteDto
        {
            Name = "Card 1"
        };

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("CardVariants");

        validationProblem.Errors["CardVariants"]
            .Should()
            .Contain(x => x.Contains("At least one card variant is required."));
    }
    [Fact]
    public async Task CreateCard_ReturnsBadRequest_WhenDuplicatePitches()
    {
        var request = new CardWriteDto
        {
            Name = "Card 1",
            CardVariants = [
                new CardVariantWriteDto {
                    Pitch = Contracts.PitchEnumAPI.NoPitch
                },
                new CardVariantWriteDto {
                    Pitch = Contracts.PitchEnumAPI.NoPitch
                },
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        validationProblem.Should().NotBeNull();

        validationProblem!.Errors.Should().ContainKey("CardVariants");

        validationProblem.Errors["CardVariants"]
            .Should()
            .Contain(x => x.Contains("Card variants cannot contain duplicate pitches."));

        for (var i = 0; i < request.CardVariants.Count; i++)
        {

            validationProblem!.Errors.Should().ContainKey($"CardVariants[{i}].SetDetails");

            validationProblem.Errors[$"CardVariants[{i}].SetDetails"]
                .Should()
                .Contain(x => x.Contains("At least one set detail is required."));
        }
    }
}