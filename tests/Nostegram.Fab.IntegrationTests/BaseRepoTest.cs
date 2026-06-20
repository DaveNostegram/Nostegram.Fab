using Microsoft.EntityFrameworkCore;
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
    public async Task<Card> SeedCardWithFullDetail(string name = "")
    {
        var card = await SeedCard(name);
        var cardSubType = await SeedCardSubType();
        var cardType = await SeedCardType();
        var fabClass = await SeedFabClass();
        var talent = await SeedTalent();

        using var context = CreateContext();

        var trackedCard = await context.Cards
            .Include(x => x.CardSubTypes)
            .Include(x => x.CardTypes)
            .Include(x => x.FabClasses)
            .Include(x => x.Talents)
            .SingleAsync(x => x.PublicId == card.PublicId);

        var trackedCardSubType = await context.CardSubTypes
            .SingleAsync(x => x.PublicId == cardSubType.PublicId);

        var trackedCardType = await context.CardTypes
            .SingleAsync(x => x.PublicId == cardType.PublicId);

        var trackedFabClass = await context.FabClasses
            .SingleAsync(x => x.PublicId == fabClass.PublicId);

        var trackedTalent = await context.Talents
            .SingleAsync(x => x.PublicId == talent.PublicId);

        trackedCard.CardSubTypes.Add(trackedCardSubType);
        trackedCard.CardTypes.Add(trackedCardType);
        trackedCard.FabClasses.Add(trackedFabClass);
        trackedCard.Talents.Add(trackedTalent);

        await context.SaveChangesAsync();

        return trackedCard;
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
    public async Task<CardSubType> SeedCardSubType(string name = "")
    {
        using var context = CreateContext();
        var newCardSubType = new CardSubType
        {
            Name = name == "" ? "Attack" : name
        };
        context.CardSubTypes.Add(newCardSubType);
        await context.SaveChangesAsync();
        return newCardSubType;
    }
    public async Task<CardType> SeedCardType(string name = "")
    {
        using var context = CreateContext();
        var newCardType = new CardType
        {
            Name = name == "" ? "Action" : name
        };
        context.CardTypes.Add(newCardType);
        await context.SaveChangesAsync();
        return newCardType;
    }
    public async Task<FabClass> SeedFabClass(string name = "")
    {
        using var context = CreateContext();
        var newFabClass = new FabClass
        {
            Name = name == "" ? "Warrior" : name
        };
        context.FabClasses.Add(newFabClass);
        await context.SaveChangesAsync();
        return newFabClass;
    }
    public async Task<Talent> SeedTalent(string name = "")
    {
        using var context = CreateContext();
        var newTalent = new Talent
        {
            Name = name == "" ? "Ice" : name
        };
        context.Talents.Add(newTalent);
        await context.SaveChangesAsync();
        return newTalent;
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