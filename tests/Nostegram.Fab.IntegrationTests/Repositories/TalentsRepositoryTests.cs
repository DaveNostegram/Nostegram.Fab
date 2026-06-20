using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class TalentRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateTalent_ValidName_CreatesTalent()
    {
        //Arrange
        using var context = CreateContext();
        var talent = new Talent
        {
            Name = "Dave Davington"
        };
        var talentRepo = new TalentRepository(context);
        //Act
        talentRepo.Create(talent);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var talentCreated = assertContext.Talents.SingleOrDefault(e => e.Name == talent.Name);

        talentCreated.Should().NotBeNull();
        talentCreated.Name.Should().Be(talent.Name);
    }

    [Fact]
    public async Task CreateTalent_WithSpecialCharacters_CreatesTalent()
    {
        //Arrange
        using var context = CreateContext();
        var talent = new Talent
        {
            Name = "寿多浩 (Dave Davington)"
        };
        var talentRepo = new TalentRepository(context);
        //Act
        talentRepo.Create(talent);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var talentCreated = assertContext.Talents.SingleOrDefault(e => e.Name == talent.Name);

        talentCreated.Should().NotBeNull();
        talentCreated.Name.Should().Be(talent.Name);
    }

    [Fact]
    public async Task CreateTalent_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.Talents.Add(new Talent
        {
            Name = "Dave Davington"
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var talentRepo = new TalentRepository(actContext);

        var duplicateTalent = new Talent
        {
            Name = "Dave Davington"
        };

        Func<Task> act = async () =>
        {
            talentRepo.Create(duplicateTalent);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetTalent_ValidId_ReturnsTalent()
    {
        //Arrange
        var newTalent = await SeedTalent();
        await using var actContext = CreateContext();
        var talentRepo = new TalentRepository(actContext);
        //Act
        var talent = await talentRepo.GetByPublicId(newTalent.PublicId, CancellationToken.None);
        //Assert

        talent.Should().NotBeNull();
        talent.Name.Should().Be(newTalent.Name);
    }

    [Fact]
    public async Task GetTalent_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var talentId = Guid.Empty;
        var talentRepo = new TalentRepository(context);
        //Act
        var talent = await talentRepo.GetByPublicId(talentId, CancellationToken.None);
        //Assert

        talent.Should().BeNull();
    }

    [Fact]
    public async Task GetTalentDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newTalent = await SeedTalent();
        using var context = CreateContext();
        var talentRepo = new TalentRepository(context);
        //Act
        var talent = await talentRepo.GetDtoByPublicId(newTalent.PublicId, CancellationToken.None);
        //Assert

        talent.Should().NotBeNull();
        talent.Name.Should().Be(newTalent.Name);
    }

    [Fact]
    public async Task GetTalentDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var talentId = Guid.Empty;
        var talentRepo = new TalentRepository(context);
        //Act
        var talent = await talentRepo.GetDtoByPublicId(talentId, CancellationToken.None);
        //Assert

        talent.Should().BeNull();
    }

    [Fact]
    public async Task GetAllTalents_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var talentCount = context.Talents.Count();
        var talentRepo = new TalentRepository(context);
        //Act
        var talents = await talentRepo.GetAll(CancellationToken.None);
        //Assert

        talents.Should().NotBeNull();
        talents.Count.Should().Be(talentCount);
    }

    [Fact]
    public async Task GetAllTalents_WithNoTalents_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var talents = context.Talents.ToList();
        context.Talents.RemoveRange(talents);
        await context.SaveChangesAsync();
        var talentRepo = new TalentRepository(context);

        //Act
        var emptyListTalents = await talentRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListTalents.Should().NotBeNull();
        emptyListTalents.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteTalent_ValidId_Successful()
    {
        //Arrange
        var newTalent = await SeedTalent();
        using var context = CreateContext();
        var talentRepo = new TalentRepository(context);
        var talent = await context.Talents.SingleAsync(x => x.PublicId == newTalent.PublicId);
        //Act
        talentRepo.Delete(talent);
        context.SaveChanges();
        //Assert
        context.Talents.SingleOrDefault(e => e.PublicId == newTalent.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteTalent_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        //Act
        using var actContext = CreateContext();
        var talent = await actContext.Talents.SingleAsync(x => x.PublicId == newCard.Talents.First().PublicId);
        var talentRepo = new TalentRepository(actContext);
        Func<Task> act = async () =>
        {
            talentRepo.Delete(talent);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedTalent_WhenNameChanged_SavesChange()
    {
        // Arrange
        var talent = await SeedTalent();

        await using var context = CreateContext();
        var trackedTalent = context.Talents.Single(e => e.PublicId == talent.PublicId);

        // Act
        trackedTalent.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedTalent = assertContext.Talents.Single(e => e.PublicId == talent.PublicId);

        updatedTalent.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedTalent_WhenNameHasSpecialCharacters_SavesChange()
    {
        var talent = await SeedTalent();

        await using var context = CreateContext();
        var trackedTalent = context.Talents.Single(e => e.PublicId == talent.PublicId);

        trackedTalent.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedTalent = assertContext.Talents.Single(e => e.PublicId == talent.PublicId);

        updatedTalent.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedTalent_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var talent1 = await SeedTalent("Bob");
        await SeedTalent("Jeff");

        await using var context = CreateContext();
        var trackedTalent = context.Talents.Single(e => e.PublicId == talent1.PublicId);

        trackedTalent.Name = "Jeff";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsByName_WhenTalentExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var talent = await SeedTalent();
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByName(talent.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByName_WhenTalentDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var talentName = "Dave Davington";
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByName(talentName, CancellationToken.None);
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
        var talent = await SeedTalent(trueText);
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByName(text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenTalentExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var talent = await SeedTalent();
        var talent2 = await SeedTalent("Jeff Jeffington");
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByNameExcludingId(talent2.Id, talent.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenTalentDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var talent = await SeedTalent("Jeff Jeffington");
        var talentName = "Dave Davington";
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByNameExcludingId(talent.Id, talentName, CancellationToken.None);
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
        var talent = await SeedTalent(trueText);
        var talent2 = await SeedTalent("Jeff Jeffington");
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.ExistsByNameExcludingId(talent2.Id, text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsTalentUsed_WhenTalentIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var newCard = await SeedCardWithFullDetail();
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.IsUsed(newCard.Talents.First().Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsTalentUsed_WhenTalentIsNotUsed_ReturnsFalse()
    {
        //Arrange
        var newCard = await SeedCardWithFullDetail();
        var talent2 = await SeedTalent("Jeff Jeffington");
        using var context = CreateContext();
        var talentRepo = new TalentRepository(context);
        //Act
        var doesExist = await talentRepo.IsUsed(talent2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
