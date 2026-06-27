using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class SetRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateSet_ValidName_CreatesSet()
    {
        //Arrange
        using var context = CreateContext();
        var set = new Set
        {
            Name = "Part The Mistveil",
            SetCode = "MST",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        var setRepo = new SetRepository(context);
        //Act
        setRepo.Create(set);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var setCreated = assertContext.Sets.SingleOrDefault(e => e.Name == set.Name);

        setCreated.Should().NotBeNull();
        setCreated.Name.Should().Be(set.Name);
        setCreated.SetCode.Should().Be(set.SetCode);
        setCreated.ReleaseDate.Should().Be(set.ReleaseDate);
    }

    [Fact]
    public async Task CreateSet_WithSpecialCharacters_CreatesSet()
    {
        //Arrange
        using var context = CreateContext();
        var set = new Set
        {
            Name = "寿多浩 (Part The Mistveil)",
            SetCode = "MST",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        var setRepo = new SetRepository(context);
        //Act
        setRepo.Create(set);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var setCreated = assertContext.Sets.SingleOrDefault(e => e.Name == set.Name);

        setCreated.Should().NotBeNull();
        setCreated.Name.Should().Be(set.Name);
    }

    [Fact]
    public async Task CreateSet_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.Sets.Add(new Set
        {
            Name = "Part The Mistveil",
            SetCode = "MST",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var setRepo = new SetRepository(actContext);

        var duplicateSet = new Set
        {
            Name = "Part The Mistveil",
            SetCode = "MSO",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        Func<Task> act = async () =>
        {
            setRepo.Create(duplicateSet);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CreateSet_WithSetCodeAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.Sets.Add(new Set
        {
            Name = "Part The Mistveil",
            SetCode = "MST",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var setRepo = new SetRepository(actContext);

        var duplicateSet = new Set
        {
            Name = "Mistveil",
            SetCode = "MST",
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        Func<Task> act = async () =>
        {
            setRepo.Create(duplicateSet);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetSet_ValidId_ReturnsSet()
    {
        //Arrange
        var newSet = await SeedSet();
        await using var actContext = CreateContext();
        var setRepo = new SetRepository(actContext);
        //Act
        var set = await setRepo.GetByPublicId(newSet.PublicId, CancellationToken.None);
        //Assert

        set.Should().NotBeNull();
        set.Name.Should().Be(newSet.Name);
        set.SetCode.Should().Be(newSet.SetCode);
        set.ReleaseDate.Should().Be(newSet.ReleaseDate);
    }

    [Fact]
    public async Task GetSet_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var setId = Guid.Empty;
        var setRepo = new SetRepository(context);
        //Act
        var set = await setRepo.GetByPublicId(setId, CancellationToken.None);
        //Assert

        set.Should().BeNull();
    }

    [Fact]
    public async Task GetSetDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newSet = await SeedSet();
        using var context = CreateContext();
        var setRepo = new SetRepository(context);
        //Act
        var set = await setRepo.GetDtoByPublicId(newSet.PublicId, CancellationToken.None);
        //Assert

        set.Should().NotBeNull();
        set.Name.Should().Be(newSet.Name);
        set.SetCode.Should().Be(newSet.SetCode);
        set.ReleaseDate.Should().Be(newSet.ReleaseDate);
    }

    [Fact]
    public async Task GetSetDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var setId = Guid.Empty;
        var setRepo = new SetRepository(context);
        //Act
        var set = await setRepo.GetDtoByPublicId(setId, CancellationToken.None);
        //Assert

        set.Should().BeNull();
    }

    [Fact]
    public async Task GetAllSets_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var setCount = context.Sets.Count();
        var setRepo = new SetRepository(context);
        //Act
        var sets = await setRepo.GetAll(CancellationToken.None);
        //Assert

        sets.Should().NotBeNull();
        sets.Count.Should().Be(setCount);
    }

    [Fact]
    public async Task GetAllSets_WithNoSets_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var sets = context.Sets.ToList();
        context.Sets.RemoveRange(sets);
        await context.SaveChangesAsync();
        var setRepo = new SetRepository(context);

        //Act
        var emptyListSets = await setRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListSets.Should().NotBeNull();
        emptyListSets.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteSet_ValidId_Successful()
    {
        //Arrange
        var newSet = await SeedSet();
        using var context = CreateContext();
        var setRepo = new SetRepository(context);
        var set = await context.Sets.SingleAsync(x => x.PublicId == newSet.PublicId);
        //Act
        setRepo.Delete(set);
        context.SaveChanges();
        //Assert
        context.Sets.SingleOrDefault(e => e.PublicId == newSet.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteSet_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newSet = (await SeedSetDetail()).Set;
        //Act
        using var actContext = CreateContext();
        var setRepo = new SetRepository(actContext);
        Func<Task> act = async () =>
        {
            setRepo.Delete(newSet);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedSet_WhenNameChanged_SavesChange()
    {
        // Arrange
        var set = await SeedSet();

        await using var context = CreateContext();
        var trackedSet = context.Sets.Single(e => e.PublicId == set.PublicId);

        // Act
        trackedSet.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedSet = assertContext.Sets.Single(e => e.PublicId == set.PublicId);

        updatedSet.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedSet_WhenNameHasSpecialCharacters_SavesChange()
    {
        var set = await SeedSet();

        await using var context = CreateContext();
        var trackedSet = context.Sets.Single(e => e.PublicId == set.PublicId);

        trackedSet.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedSet = assertContext.Sets.Single(e => e.PublicId == set.PublicId);

        updatedSet.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedSet_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var set1 = await SeedSet("SetName1", "CO1");
        await SeedSet("SetName2", "CO2");

        await using var context = CreateContext();
        var trackedSet = context.Sets.Single(e => e.PublicId == set1.PublicId);

        trackedSet.Name = "SetName2";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task TrackedSet_WhenSetCodeAlreadyExists_ThrowsDbUpdateException()
    {
        var set1 = await SeedSet("SetName1", "CO1");
        await SeedSet("SetName2", "CO2");

        await using var context = CreateContext();
        var trackedSet = context.Sets.Single(e => e.PublicId == set1.PublicId);

        trackedSet.SetCode = "CO2";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CheckUniqueness_WhenSetNameAndCodeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var set = await SeedSet();
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(set.Name, set.SetCode, null, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(true);
        doesExist.SetCodeExists.Should().Be(true);
    }
    [Fact]
    public async Task CheckUniqueness_WhenSetNameExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var set = await SeedSet();
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(set.Name, "UnusedCode", null, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(true);
        doesExist.SetCodeExists.Should().Be(false);
    }
    [Fact]
    public async Task CheckUniqueness_WhenSetCodeExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var set = await SeedSet();
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness("UnusedName", set.SetCode, null, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(false);
        doesExist.SetCodeExists.Should().Be(true);
    }
    [Fact]
    public async Task CheckUniqueness_WhenSetNameAndCodeDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var setName = "Mistveil";
        var setCode = "MST";
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(setName, setCode, null, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(false);
        doesExist.SetCodeExists.Should().Be(false);
    }
    [Fact]
    public async Task CheckUniqueness_IsCaseInsensitive_ReturnsTrue()
    {
        //Arrange
        var setName = "mistveil";
        var trueSetName = "Mistveil";
        var setCode = "mst";
        var trueSetCode = "MST";
        using var context = CreateContext();
        var set = await SeedSet(trueSetName, trueSetCode);
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(setName, setCode, null, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(true);
        doesExist.SetCodeExists.Should().Be(true);
    }
    [Fact]
    public async Task CheckUniqueness_WhenSetExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var set = await SeedSet();
        var set2 = await SeedSet("Set2", "CO2");
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(set2.Name, set2.SetCode, set.Id, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(true);
        doesExist.SetCodeExists.Should().Be(true);
    }
    [Fact]
    public async Task CheckUniqueness_WhenSetDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var set = await SeedSet();
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.CheckUniqueness(set.Name, set.SetCode, set.Id, CancellationToken.None);
        //Assert
        doesExist.NameExists.Should().Be(false);
        doesExist.SetCodeExists.Should().Be(false);
    }
    [Fact]
    public async Task IsSetUsed_WhenSetIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var set = (await SeedSetDetail()).Set;
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.IsUsed(set.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsSetUsed_WhenSetIsNotUsed_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var set = (await SeedSetDetail()).Set;
        var set2 = await SeedSet("Mistveil", "MST");
        var setRepo = new SetRepository(context);
        //Act
        var doesExist = await setRepo.IsUsed(set2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
