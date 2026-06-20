using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class CardSubTypeRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateCardSubType_ValidName_CreatesCardSubType()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubType = new CardSubType
        {
            Name = "Dave Davington"
        };
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        cardSubTypeRepo.Create(cardSubType);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var cardSubTypeCreated = assertContext.CardSubTypes.SingleOrDefault(e => e.Name == cardSubType.Name);

        cardSubTypeCreated.Should().NotBeNull();
        cardSubTypeCreated.Name.Should().Be(cardSubType.Name);
    }

    [Fact]
    public async Task CreateCardSubType_WithSpecialCharacters_CreatesCardSubType()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubType = new CardSubType
        {
            Name = "寿多浩 (Dave Davington)"
        };
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        cardSubTypeRepo.Create(cardSubType);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var cardSubTypeCreated = assertContext.CardSubTypes.SingleOrDefault(e => e.Name == cardSubType.Name);

        cardSubTypeCreated.Should().NotBeNull();
        cardSubTypeCreated.Name.Should().Be(cardSubType.Name);
    }

    [Fact]
    public async Task CreateCardSubType_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.CardSubTypes.Add(new CardSubType
        {
            Name = "Dave Davington"
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var cardSubTypeRepo = new CardSubTypeRepository(actContext);

        var duplicateCardSubType = new CardSubType
        {
            Name = "Dave Davington"
        };

        Func<Task> act = async () =>
        {
            cardSubTypeRepo.Create(duplicateCardSubType);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetCardSubType_ValidId_ReturnsCardSubType()
    {
        //Arrange
        var newCardSubType = await SeedCardSubType();
        await using var actContext = CreateContext();
        var cardSubTypeRepo = new CardSubTypeRepository(actContext);
        //Act
        var cardSubType = await cardSubTypeRepo.GetByPublicId(newCardSubType.PublicId, CancellationToken.None);
        //Assert

        cardSubType.Should().NotBeNull();
        cardSubType.Name.Should().Be(newCardSubType.Name);
    }

    [Fact]
    public async Task GetCardSubType_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubTypeId = Guid.Empty;
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var cardSubType = await cardSubTypeRepo.GetByPublicId(cardSubTypeId, CancellationToken.None);
        //Assert

        cardSubType.Should().BeNull();
    }

    [Fact]
    public async Task GetCardSubTypeDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newCardSubType = await SeedCardSubType();
        using var context = CreateContext();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var cardSubType = await cardSubTypeRepo.GetDtoByPublicId(newCardSubType.PublicId, CancellationToken.None);
        //Assert

        cardSubType.Should().NotBeNull();
        cardSubType.Name.Should().Be(newCardSubType.Name);
    }

    [Fact]
    public async Task GetCardSubTypeDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubTypeId = Guid.Empty;
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var cardSubType = await cardSubTypeRepo.GetDtoByPublicId(cardSubTypeId, CancellationToken.None);
        //Assert

        cardSubType.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCardSubTypes_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubTypeCount = context.CardSubTypes.Count();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var cardSubTypes = await cardSubTypeRepo.GetAll(CancellationToken.None);
        //Assert

        cardSubTypes.Should().NotBeNull();
        cardSubTypes.Count.Should().Be(cardSubTypeCount);
    }

    [Fact]
    public async Task GetAllCardSubTypes_WithNoCardSubTypes_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubTypes = context.CardSubTypes.ToList();
        context.CardSubTypes.RemoveRange(cardSubTypes);
        await context.SaveChangesAsync();
        var cardSubTypeRepo = new CardSubTypeRepository(context);

        //Act
        var emptyListCardSubTypes = await cardSubTypeRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListCardSubTypes.Should().NotBeNull();
        emptyListCardSubTypes.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteCardSubType_ValidId_Successful()
    {
        //Arrange
        var newCardSubType = await SeedCardSubType();
        using var context = CreateContext();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        var cardSubType = await context.CardSubTypes.SingleAsync(x => x.PublicId == newCardSubType.PublicId);
        //Act
        cardSubTypeRepo.Delete(cardSubType);
        context.SaveChanges();
        //Assert
        context.CardSubTypes.SingleOrDefault(e => e.PublicId == newCardSubType.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteCardSubType_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        //Act
        using var actContext = CreateContext();
        var cardSubType = await actContext.CardSubTypes.SingleAsync(x => x.PublicId == newCard.CardSubTypes.First().PublicId);
        var cardSubTypeRepo = new CardSubTypeRepository(actContext);
        Func<Task> act = async () =>
        {
            cardSubTypeRepo.Delete(cardSubType);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedCardSubType_WhenNameChanged_SavesChange()
    {
        // Arrange
        var cardSubType = await SeedCardSubType();

        await using var context = CreateContext();
        var trackedCardSubType = context.CardSubTypes.Single(e => e.PublicId == cardSubType.PublicId);

        // Act
        trackedCardSubType.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedCardSubType = assertContext.CardSubTypes.Single(e => e.PublicId == cardSubType.PublicId);

        updatedCardSubType.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedCardSubType_WhenNameHasSpecialCharacters_SavesChange()
    {
        var cardSubType = await SeedCardSubType();

        await using var context = CreateContext();
        var trackedCardSubType = context.CardSubTypes.Single(e => e.PublicId == cardSubType.PublicId);

        trackedCardSubType.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedCardSubType = assertContext.CardSubTypes.Single(e => e.PublicId == cardSubType.PublicId);

        updatedCardSubType.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedCardSubType_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var cardSubType1 = await SeedCardSubType("Bob");
        await SeedCardSubType("Jeff");

        await using var context = CreateContext();
        var trackedCardSubType = context.CardSubTypes.Single(e => e.PublicId == cardSubType1.PublicId);

        trackedCardSubType.Name = "Jeff";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsByName_WhenCardSubTypeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubType = await SeedCardSubType();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByName(cardSubType.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByName_WhenCardSubTypeDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubTypeName = "Dave Davington";
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByName(cardSubTypeName, CancellationToken.None);
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
        var cardSubType = await SeedCardSubType(trueText);
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByName(text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenCardSubTypeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubType = await SeedCardSubType();
        var cardSubType2 = await SeedCardSubType("Jeff Jeffington");
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByNameExcludingId(cardSubType2.Id, cardSubType.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenCardSubTypeDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var cardSubType = await SeedCardSubType("Jeff Jeffington");
        var cardSubTypeName = "Dave Davington";
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByNameExcludingId(cardSubType.Id, cardSubTypeName, CancellationToken.None);
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
        var cardSubType = await SeedCardSubType(trueText);
        var cardSubType2 = await SeedCardSubType("Jeff Jeffington");
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.ExistsByNameExcludingId(cardSubType2.Id, text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsCardSubTypeUsed_WhenCardSubTypeIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var newCard = await SeedCardWithFullDetail();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.IsUsed(newCard.CardSubTypes.First().Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsCardSubTypeUsed_WhenCardSubTypeIsNotUsed_ReturnsFalse()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        var cardSubType2 = await SeedCardSubType("Jeff Jeffington");
        using var context = CreateContext();
        var cardSubTypeRepo = new CardSubTypeRepository(context);
        //Act
        var doesExist = await cardSubTypeRepo.IsUsed(cardSubType2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
