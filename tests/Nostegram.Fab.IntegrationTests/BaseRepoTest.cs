using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests;

[Collection("Database")]
public abstract class BaseRepoTest : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;

    protected BaseRepoTest(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    protected FabDbContext CreateContext()
        => Fixture.CreateContext();

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task<Artist> SeedSetDetailWithArtist(string name = "")
    {
        var artist = await SeedArtist(name);
        var card = await SeedCard();
        var set = await SeedSet();
        var cardVariant = await SeedCardVariant(card.Id);
        await SeedSetDetail(set.Id, cardVariant.Id, artist.Id);
        return artist;
    }

    public async Task<Artist> SeedArtist(string name = "")
    {
        using var context = CreateContext();
        var newArtist = new Artist
        {
            Name = name == "" ? "Dave Davington" : name
        };
        context.Artists.Add(newArtist);
        await context.SaveChangesAsync();
        return newArtist;
    }

    public async Task<Card> SeedCard(string name = "")
    {
        using var context = CreateContext();
        var card = new Card
        {
            Name = name == "" ? "Test Card" : name
        };

        context.Cards.Add(card);
        await context.SaveChangesAsync();
        return card;
    }
    public async Task<Set> SeedSet(string name = "", string setCode = "", DateOnly? releaseDate = null)
    {
        using var context = CreateContext();
        var set = new Set
        {
            Name = name == "" ? "Test Set" : name,
            SetCode = setCode == "" ? "TEST" : setCode,
            ReleaseDate = releaseDate == null ? DateOnly.FromDateTime(DateTime.Now) : releaseDate.Value,
        };

        context.Sets.Add(set);
        await context.SaveChangesAsync();
        return set;
    }
    public async Task<CardVariant> SeedCardVariant(int cardId)
    {
        using var context = CreateContext();
        var cardVariant = new CardVariant
        {
            CardId = cardId
        };

        context.CardVariants.Add(cardVariant);
        await context.SaveChangesAsync();
        return cardVariant;
    }

    public async Task<SetDetail> SeedSetDetail(int setId, int cardVariantId, int artistId, string collectorNumber = "", RarityEnum rarity = RarityEnum.Common)
    {
        using var context = CreateContext();
        var setDetail = new SetDetail
        {
            CollectorNumber = collectorNumber == "" ? "FAB001" : collectorNumber,
            Rarity = rarity == RarityEnum.Common ? RarityEnum.Common : rarity,
            SetId = setId,
            CardVariantId = cardVariantId,
            ArtistId = artistId
        };

        context.SetDetails.Add(setDetail);
        await context.SaveChangesAsync();
        return setDetail;
    }
}