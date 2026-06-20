using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class FabClassRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateFabClass_ValidName_CreatesFabClass()
    {
        //Arrange
        using var context = CreateContext();
        var fabClass = new FabClass
        {
            Name = "Dave Davington"
        };
        var fabClassRepo = new FabClassRepository(context);
        //Act
        fabClassRepo.Create(fabClass);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var fabClassCreated = assertContext.FabClasses.SingleOrDefault(e => e.Name == fabClass.Name);

        fabClassCreated.Should().NotBeNull();
        fabClassCreated.Name.Should().Be(fabClass.Name);
    }

    [Fact]
    public async Task CreateFabClass_WithSpecialCharacters_CreatesFabClass()
    {
        //Arrange
        using var context = CreateContext();
        var fabClass = new FabClass
        {
            Name = "寿多浩 (Dave Davington)"
        };
        var fabClassRepo = new FabClassRepository(context);
        //Act
        fabClassRepo.Create(fabClass);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var fabClassCreated = assertContext.FabClasses.SingleOrDefault(e => e.Name == fabClass.Name);

        fabClassCreated.Should().NotBeNull();
        fabClassCreated.Name.Should().Be(fabClass.Name);
    }

    [Fact]
    public async Task CreateFabClass_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.FabClasses.Add(new FabClass
        {
            Name = "Dave Davington"
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var fabClassRepo = new FabClassRepository(actContext);

        var duplicateFabClass = new FabClass
        {
            Name = "Dave Davington"
        };

        Func<Task> act = async () =>
        {
            fabClassRepo.Create(duplicateFabClass);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetFabClass_ValidId_ReturnsFabClass()
    {
        //Arrange
        var newFabClass = await SeedFabClass();
        await using var actContext = CreateContext();
        var fabClassRepo = new FabClassRepository(actContext);
        //Act
        var fabClass = await fabClassRepo.GetByPublicId(newFabClass.PublicId, CancellationToken.None);
        //Assert

        fabClass.Should().NotBeNull();
        fabClass.Name.Should().Be(newFabClass.Name);
    }

    [Fact]
    public async Task GetFabClass_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var fabClassId = Guid.Empty;
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var fabClass = await fabClassRepo.GetByPublicId(fabClassId, CancellationToken.None);
        //Assert

        fabClass.Should().BeNull();
    }

    [Fact]
    public async Task GetFabClassDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newFabClass = await SeedFabClass();
        using var context = CreateContext();
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var fabClass = await fabClassRepo.GetDtoByPublicId(newFabClass.PublicId, CancellationToken.None);
        //Assert

        fabClass.Should().NotBeNull();
        fabClass.Name.Should().Be(newFabClass.Name);
    }

    [Fact]
    public async Task GetFabClassDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var fabClassId = Guid.Empty;
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var fabClass = await fabClassRepo.GetDtoByPublicId(fabClassId, CancellationToken.None);
        //Assert

        fabClass.Should().BeNull();
    }

    [Fact]
    public async Task GetAllFabClasses_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var fabClassCount = context.FabClasses.Count();
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var fabClasses = await fabClassRepo.GetAll(CancellationToken.None);
        //Assert

        fabClasses.Should().NotBeNull();
        fabClasses.Count.Should().Be(fabClassCount);
    }

    [Fact]
    public async Task GetAllFabClasses_WithNoFabClasses_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var fabClasses = context.FabClasses.ToList();
        context.FabClasses.RemoveRange(fabClasses);
        await context.SaveChangesAsync();
        var fabClassRepo = new FabClassRepository(context);

        //Act
        var emptyListFabClasses = await fabClassRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListFabClasses.Should().NotBeNull();
        emptyListFabClasses.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteFabClass_ValidId_Successful()
    {
        //Arrange
        var newFabClass = await SeedFabClass();
        using var context = CreateContext();
        var fabClassRepo = new FabClassRepository(context);
        var fabClass = await context.FabClasses.SingleAsync(x => x.PublicId == newFabClass.PublicId);
        //Act
        fabClassRepo.Delete(fabClass);
        context.SaveChanges();
        //Assert
        context.FabClasses.SingleOrDefault(e => e.PublicId == newFabClass.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteFabClass_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        //Act
        using var actContext = CreateContext();
        var fabClass = await actContext.FabClasses.SingleAsync(x => x.PublicId == newCard.FabClasses.First().PublicId);
        var fabClassRepo = new FabClassRepository(actContext);
        Func<Task> act = async () =>
        {
            fabClassRepo.Delete(fabClass);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedFabClass_WhenNameChanged_SavesChange()
    {
        // Arrange
        var fabClass = await SeedFabClass();

        await using var context = CreateContext();
        var trackedFabClass = context.FabClasses.Single(e => e.PublicId == fabClass.PublicId);

        // Act
        trackedFabClass.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedFabClass = assertContext.FabClasses.Single(e => e.PublicId == fabClass.PublicId);

        updatedFabClass.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedFabClass_WhenNameHasSpecialCharacters_SavesChange()
    {
        var fabClass = await SeedFabClass();

        await using var context = CreateContext();
        var trackedFabClass = context.FabClasses.Single(e => e.PublicId == fabClass.PublicId);

        trackedFabClass.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedFabClass = assertContext.FabClasses.Single(e => e.PublicId == fabClass.PublicId);

        updatedFabClass.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedFabClass_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var fabClass1 = await SeedFabClass("Bob");
        await SeedFabClass("Jeff");

        await using var context = CreateContext();
        var trackedFabClass = context.FabClasses.Single(e => e.PublicId == fabClass1.PublicId);

        trackedFabClass.Name = "Jeff";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsByName_WhenFabClassExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var fabClass = await SeedFabClass();
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByName(fabClass.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByName_WhenFabClassDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var fabClassName = "Dave Davington";
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByName(fabClassName, CancellationToken.None);
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
        var fabClass = await SeedFabClass(trueText);
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByName(text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenFabClassExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var fabClass = await SeedFabClass();
        var fabClass2 = await SeedFabClass("Jeff Jeffington");
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByNameExcludingId(fabClass2.Id, fabClass.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenFabClassDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var fabClass = await SeedFabClass("Jeff Jeffington");
        var fabClassName = "Dave Davington";
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByNameExcludingId(fabClass.Id, fabClassName, CancellationToken.None);
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
        var fabClass = await SeedFabClass(trueText);
        var fabClass2 = await SeedFabClass("Jeff Jeffington");
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.ExistsByNameExcludingId(fabClass2.Id, text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsFabClassUsed_WhenFabClassIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var newCard = await SeedCardWithFullDetail();
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.IsUsed(newCard.FabClasses.First().Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsFabClassUsed_WhenFabClassIsNotUsed_ReturnsFalse()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        var fabClass2 = await SeedFabClass("Jeff Jeffington");
        using var context = CreateContext();
        var fabClassRepo = new FabClassRepository(context);
        //Act
        var doesExist = await fabClassRepo.IsUsed(fabClass2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
