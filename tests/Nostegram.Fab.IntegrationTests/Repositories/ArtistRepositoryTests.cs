using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Domain;
using Nostegram.Fab.Infrastructure.Persistence.Repositories;
using Nostegram.Fab.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.Repositories;

public sealed class ArtistRepositoryTests(DatabaseFixture fixture) : BaseRepoTest(fixture)
{
    [Fact]
    public async Task CreateArtist_ValidName_CreatesArtist()
    {
        //Arrange
        using var context = CreateContext();
        var artist = new Artist
        {
            Name = "Dave Davington"
        };
        var artistRepo = new ArtistRepository(context);
        //Act
        artistRepo.Create(artist);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var artistCreated = assertContext.Artists.SingleOrDefault(e => e.Name == artist.Name);

        artistCreated.Should().NotBeNull();
        artistCreated.Name.Should().Be(artist.Name);
    }

    [Fact]
    public async Task CreateArtist_WithSpecialCharacters_CreatesArtist()
    {
        //Arrange
        using var context = CreateContext();
        var artist = new Artist
        {
            Name = "寿多浩 (Dave Davington)"
        };
        var artistRepo = new ArtistRepository(context);
        //Act
        artistRepo.Create(artist);
        await context.SaveChangesAsync(CancellationToken.None);
        //Assert
        using var assertContext = CreateContext();
        var artistCreated = assertContext.Artists.SingleOrDefault(e => e.Name == artist.Name);

        artistCreated.Should().NotBeNull();
        artistCreated.Name.Should().Be(artist.Name);
    }

    [Fact]
    public async Task CreateArtist_WithNameAlreadyExists_ThrowsException()
    {
        // Arrange
        await using var arrangeContext = CreateContext();

        arrangeContext.Artists.Add(new Artist
        {
            Name = "Dave Davington"
        });

        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var artistRepo = new ArtistRepository(actContext);

        var duplicateArtist = new Artist
        {
            Name = "Dave Davington"
        };

        Func<Task> act = async () =>
        {
            artistRepo.Create(duplicateArtist);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetArtist_ValidId_ReturnsArtist()
    {
        //Arrange
        var newArtist = await SeedArtist();
        await using var actContext = CreateContext();
        var artistRepo = new ArtistRepository(actContext);
        //Act
        var artist = await artistRepo.GetByPublicId(newArtist.PublicId, CancellationToken.None);
        //Assert

        artist.Should().NotBeNull();
        artist.Name.Should().Be(newArtist.Name);
    }

    [Fact]
    public async Task GetArtist_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var artistId = Guid.Empty;
        var artistRepo = new ArtistRepository(context);
        //Act
        var artist = await artistRepo.GetByPublicId(artistId, CancellationToken.None);
        //Assert

        artist.Should().BeNull();
    }

    [Fact]
    public async Task GetArtistDto_ValidId_ReturnsDto()
    {
        //Arrange
        var newArtist = await SeedArtist();
        using var context = CreateContext();
        var artistRepo = new ArtistRepository(context);
        //Act
        var artist = await artistRepo.GetDtoByPublicId(newArtist.PublicId, CancellationToken.None);
        //Assert

        artist.Should().NotBeNull();
        artist.Name.Should().Be(newArtist.Name);
    }

    [Fact]
    public async Task GetArtistDto_InvalidId_ReturnsNull()
    {
        //Arrange
        using var context = CreateContext();
        var artistId = Guid.Empty;
        var artistRepo = new ArtistRepository(context);
        //Act
        var artist = await artistRepo.GetDtoByPublicId(artistId, CancellationToken.None);
        //Assert

        artist.Should().BeNull();
    }

    [Fact]
    public async Task GetAllArtists_ReturnsList()
    {
        //Arrange
        using var context = CreateContext();
        var artistCount = context.Artists.Count();
        var artistRepo = new ArtistRepository(context);
        //Act
        var artists = await artistRepo.GetAll(CancellationToken.None);
        //Assert

        artists.Should().NotBeNull();
        artists.Count.Should().Be(artistCount);
    }

    [Fact]
    public async Task GetAllArtists_WithNoArtists_ReturnsEmptyList()
    {
        //Arrange
        using var context = CreateContext();
        var artists = context.Artists.ToList();
        context.Artists.RemoveRange(artists);
        await context.SaveChangesAsync();
        var artistRepo = new ArtistRepository(context);

        //Act
        var emptyListArtists = await artistRepo.GetAll(CancellationToken.None);
        //Assert

        emptyListArtists.Should().NotBeNull();
        emptyListArtists.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteArtist_ValidId_Successful()
    {
        //Arrange
        var newArtist = await SeedArtist();
        using var context = CreateContext();
        var artistRepo = new ArtistRepository(context);
        var artist = await context.Artists.SingleAsync(x => x.PublicId == newArtist.PublicId);
        //Act
        artistRepo.Delete(artist);
        context.SaveChanges();
        //Assert
        context.Artists.SingleOrDefault(e => e.PublicId == newArtist.PublicId).Should().BeNull();
    }

    [Fact]
    public async Task DeleteArtist_WhenReferencedBySetDetail_ThrowsDbUpdateException()
    {
        //Arrange
        var newArtist = (await SeedSetDetail()).Artist;
        //Act
        using var actContext = CreateContext();
        var artist = await actContext.Artists.SingleAsync(x => x.PublicId == newArtist.PublicId);
        var artistRepo = new ArtistRepository(actContext);
        Func<Task> act = async () =>
        {
            artistRepo.Delete(artist);
            await actContext.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public async Task TrackedArtist_WhenNameChanged_SavesChange()
    {
        // Arrange
        var artist = await SeedArtist();

        await using var context = CreateContext();
        var trackedArtist = context.Artists.Single(e => e.PublicId == artist.PublicId);

        // Act
        trackedArtist.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        await using var assertContext = CreateContext();
        var updatedArtist = assertContext.Artists.Single(e => e.PublicId == artist.PublicId);

        updatedArtist.Name.Should().Be("New Name");
    }
    [Fact]
    public async Task TrackedArtist_WhenNameHasSpecialCharacters_SavesChange()
    {
        var artist = await SeedArtist();

        await using var context = CreateContext();
        var trackedArtist = context.Artists.Single(e => e.PublicId == artist.PublicId);

        trackedArtist.Name = "寿多浩 (Dave Davington)";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var updatedArtist = assertContext.Artists.Single(e => e.PublicId == artist.PublicId);

        updatedArtist.Name.Should().Be("寿多浩 (Dave Davington)");
    }
    [Fact]
    public async Task TrackedArtist_WhenNameAlreadyExists_ThrowsDbUpdateException()
    {
        var artist1 = await SeedArtist("Bob");
        await SeedArtist("Jeff");

        await using var context = CreateContext();
        var trackedArtist = context.Artists.Single(e => e.PublicId == artist1.PublicId);

        trackedArtist.Name = "Jeff";

        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsByName_WhenArtistExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var artist = await SeedArtist();
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByName(artist.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByName_WhenArtistDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var artistName = "Dave Davington";
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByName(artistName, CancellationToken.None);
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
        var artist = await SeedArtist(trueText);
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByName(text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenArtistExists_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var artist = await SeedArtist();
        var artist2 = await SeedArtist("Jeff Jeffington");
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByNameExcludingId(artist2.Id, artist.Name, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task ExistsByNameExcludingId_WhenArtistDoesNotExist_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var artist = await SeedArtist("Jeff Jeffington");
        var artistName = "Dave Davington";
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByNameExcludingId(artist.Id, artistName, CancellationToken.None);
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
        var artist = await SeedArtist(trueText);
        var artist2 = await SeedArtist("Jeff Jeffington");
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.ExistsByNameExcludingId(artist2.Id, text, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsArtistUsed_WhenArtistIsUsed_ReturnsTrue()
    {
        //Arrange
        using var context = CreateContext();
        var artist = (await SeedSetDetail()).Artist;
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.IsUsed(artist.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(true);
    }
    [Fact]
    public async Task IsArtistUsed_WhenArtistIsNotUsed_ReturnsFalse()
    {
        //Arrange
        using var context = CreateContext();
        var artist = (await SeedSetDetail()).Artist;
        var artist2 = await SeedArtist("Jeff Jeffington");
        var artistRepo = new ArtistRepository(context);
        //Act
        var doesExist = await artistRepo.IsUsed(artist2.Id, CancellationToken.None);
        //Assert
        doesExist.Should().Be(false);
    }
}
