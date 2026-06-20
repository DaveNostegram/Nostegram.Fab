using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class CardTypeRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateCardType_ValidName_CreatesCardType()
    {
        //Arrange
        using var context = CreateContext();
        var cardType = new CardType
        {
            Name = "Dave Davington"
        };
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        cardTypeRepo.Create(cardType);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var cardTypeCreated = assertContext.CardTypes.SingleOrDefault(e => e.Name == cardType.Name);

        cardTypeCreated.Should().NotBeNull();
        cardTypeCreated.Name.Should().Be(cardType.Name);
    }

    [Fact]
    public async Task CreateCardType_WithSpecialCharacters_CreatesCardType()
    {
        //Arrange
        using var context = CreateContext();
        var cardType = new CardType
        {
            Name = "寿多浩 (Dave Davington)"
        };
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        cardTypeRepo.Create(cardType);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var cardTypeCreated = assertContext.CardTypes.SingleOrDefault(e => e.Name == cardType.Name);

        cardTypeCreated.Should().NotBeNull();
        cardTypeCreated.Name.Should().Be(cardType.Name);
    }

    [Fact]
    public async Task CreateCardType_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.CardTypes.Add(new CardType
        {
            Name = "Dave Davington"
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var cardTypeRepo = new CardTypeRepository(actContext);

        var duplicateCardType = new CardType
        {
            Name = "Dave Davington"
        };

        Func<Task> act = async () =>
        {
            cardTypeRepo.Create(duplicateCardType);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetCardType_ValidId_ReturnsCardType()
    {
        //Arrange
        var newCardType = await SeedCardType();
        await using var actContext = CreateContext();
        var cardTypeRepo = new CardTypeRepository(actContext);
        //Act
        var cardType = await cardTypeRepo.GetByPublicId(newCardType.PublicId, CancellationToken.None);
        //Assert

        cardType.Should().NotBeNull();
        cardType.Name.Should().Be(newCardType.Name);
    }

    [Fact]
    public async Task GetCardType_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var cardTypeId = Guid.Empty;
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var cardType = await cardTypeRepo.GetByPublicId(cardTypeId, CancellationToken.None);
        //Assert

        cardType.Should().BeNull();
    }

    [Fact]
    public async Task GetCardTypeDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newCardType = await SeedCardType();
        using var context = CreateContext();
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var cardType = await cardTypeRepo.GetDtoByPublicId(newCardType.PublicId, CancellationToken.None);
        //Assert

        cardType.Should().NotBeNull();
        cardType.Name.Should().Be(newCardType.Name);
    }

    [Fact]
    public async Task GetCardTypeDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var cardTypeId = Guid.Empty;
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var cardType = await cardTypeRepo.GetDtoByPublicId(cardTypeId, CancellationToken.None);
        //Assert

        cardType.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCardTypes_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var cardTypeCount = context.CardTypes.Count();
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var cardTypes = await cardTypeRepo.GetAll(CancellationToken.None);
        //Assert

        cardTypes.Should().NotBeNull();
        cardTypes.Count.Should().Be(cardTypeCount);
    }

    [Fact]
    public async Task GetAllCardTypes_WithNoCardTypes_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var cardTypes = context.CardTypes.ToList();
        context.CardTypes.RemoveRange(cardTypes);
        await context.SaveChangesAsync();
        var cardTypeRepo = new CardTypeRepository(context);

        //Act
        var emptyListCardTypes = await cardTypeRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListCardTypes.Should().NotBeNull();
        emptyListCardTypes.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteCardType_ValidId_Successful()
    {
        //Arrange
        var newCardType = await SeedCardType();
        using var context = CreateContext();
        var cardTypeRepo = new CardTypeRepository(context);
        var cardType = await context.CardTypes.SingleAsync(x => x.PublicId == newCardType.PublicId);
        //Act
        cardTypeRepo.Delete(cardType);
        context.SaveChanges();
        //Assert
        context.CardTypes.SingleOrDefault(e => e.PublicId == newCardType.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteCardType_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        //Act
        using var actContext = CreateContext();
        var cardType = await actContext.CardTypes.SingleAsync(x => x.PublicId == newCard.CardTypes.First().PublicId);
        var cardTypeRepo = new CardTypeRepository(actContext);
        Func<Task> act = async () =>
        {
            cardTypeRepo.Delete(cardType);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedCardType_WhenNameChanged_SavesChange()
    {
        // Arrange
        var cardType = await SeedCardType();

        await using var context = CreateContext();
        var trackedCardType = context.CardTypes.Single(e => e.PublicId == cardType.PublicId);

        // Act
        trackedCardType.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedCardType = assertContext.CardTypes.Single(e => e.PublicId == cardType.PublicId);

        updatedCardType.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedCardType_WhenNameHasSpecialCharacters_SavesChange()
    {
        var cardType = await SeedCardType();

        await using var context = CreateContext();
        var trackedCardType = context.CardTypes.Single(e => e.PublicId == cardType.PublicId);

        trackedCardType.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedCardType = assertContext.CardTypes.Single(e => e.PublicId == cardType.PublicId);

        updatedCardType.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedCardType_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var cardType1 = await SeedCardType("Bob");
        await SeedCardType("Jeff");

        await using var context = CreateContext();
        var trackedCardType = context.CardTypes.Single(e => e.PublicId == cardType1.PublicId);

        trackedCardType.Name = "Jeff";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsByName_WhenCardTypeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var cardType = await SeedCardType();
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByName(cardType.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByName_WhenCardTypeDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var cardTypeName = "Dave Davington";
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByName(cardTypeName, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
    [Fact]
    public async Task ExistsByName_IsCaseInsensitive_ReturnsTrue()
    {
        //Arrange
        var text = "dave davington";
        var trueText = "Dave Davington";
        using var context = CreateContext();
        var cardType = await SeedCardType(trueText);
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByName(text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenCardTypeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var cardType = await SeedCardType();
        var cardType2 = await SeedCardType("Jeff Jeffington");
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByNameExcludingId(cardType2.Id, cardType.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenCardTypeDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var cardType = await SeedCardType("Jeff Jeffington");
        var cardTypeName = "Dave Davington";
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByNameExcludingId(cardType.Id, cardTypeName, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_IsCaseInsensitive_ReturnsTrue()
    {
        //Arrange
        var text = "dave davington";
        var trueText = "Dave Davington";
        using var context = CreateContext();
        var cardType = await SeedCardType(trueText);
        var cardType2 = await SeedCardType("Jeff Jeffington");
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.ExistsByNameExcludingId(cardType2.Id, text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsCardTypeUsed_WhenCardTypeIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var newCard = await SeedCardWithFullDetail();
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.IsUsed(newCard.CardTypes.First().Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsCardTypeUsed_WhenCardTypeIsNotUsed_ReturnsFalse()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        var cardType2 = await SeedCardType("Jeff Jeffington");
        using var context = CreateContext();
        var cardTypeRepo = new CardTypeRepository(context);
        //Act
        var doesExist = await cardTypeRepo.IsUsed(cardType2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
