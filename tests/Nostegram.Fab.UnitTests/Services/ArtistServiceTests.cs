using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Artists;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;
using Xunit;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class ArtistServiceTests
{
    [Fact]
    public async Task CreateArtist_ValidName_ReturnsPublicId()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        Artist? createdArtist = null;

        repo.Setup(r => r.Create(It.IsAny<Artist>())).Callback<Artist>(artist => createdArtist = artist);

        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateArtist(dto, CancellationToken.None);

        // Assert
        createdArtist.Should().NotBeNull();
        createdArtist!.Name.Should().Be(dto.Name);

        result.PublicId.Should().Be(createdArtist.PublicId);
        result.Name.Should().Be(createdArtist.Name);

        repo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Artist>(a => a.Name == dto.Name)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateArtist_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington";
        var dto = new LookupItemWriteDto(text);

        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(trueText, CancellationToken.None))
            .ReturnsAsync(false);

        Artist? createdArtist = null;

        repo.Setup(r => r.Create(It.IsAny<Artist>())).Callback<Artist>(artist => createdArtist = artist);

        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateArtist(dto, CancellationToken.None);

        // Assert
        createdArtist.Should().NotBeNull();
        createdArtist!.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdArtist.PublicId);
        result.Name.Should().Be(createdArtist.Name);

        repo.Verify(r => r.ExistsByName(trueText, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Artist>(a => a.Name == trueText)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateArtist_AlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateArtist(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        repo.Verify(e => e.Create(It.Is<Artist>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateArtist_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("           ");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateArtist(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(Artist.Name)}' is required.");

        repo.Verify(e => e.Create(It.Is<Artist>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetArtist_ValidPublicId_ReturnsDto()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");

        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var result = await service.GetArtist(dto.PublicId, CancellationToken.None);
        // Assert
        result.PublicId.Should().Be(dto.PublicId);
        result.Name.Should().Be(dto.Name);
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task GetArtist_InvalidPublicId_ReturnsNotFoundException()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((LookupItemDto?)null);
        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetArtist(dto.PublicId, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{dto.PublicId}' not found.");
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllArtists_ReturnsArtists()
    {
        // Arrange
        var dtos = new List<LookupItemDto>
        {
            new(Guid.NewGuid(),"Dave Davington 1"),
            new(Guid.NewGuid(),"Dave Davington 2")
        };

        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllArtists(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllArtists_NoArtists_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<LookupItemDto>();

        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new ArtistService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllArtists(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteArtist_ValidPublicId_Deletes()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        await service.DeleteArtist(artist.PublicId, CancellationToken.None);
        // Assert
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteArtist_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Artist?)null);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.DeleteArtist(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.PublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Never());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DeleteArtist_Conflict_ReturnsConflictException()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(
                    () => service.DeleteArtist(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.Name}' is used by a 'Card'.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task UpdateArtist_ValidPublicIdAndName_Updates()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var updatedArtist = await service.UpdateArtist(searchPublicId, dto, CancellationToken.None);
        // Assert
        updatedArtist.Should().NotBeNull();
        updatedArtist.Name.Should().Be(dto.Name);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateArtist_InvalidPublicId_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var searchPublicId = Guid.Empty;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Artist?)null);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.UpdateArtist(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{searchPublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateArtist_InvalidName_ThrowsAlreadyExistsException()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateArtist(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateArtist_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington 2";

        var artist = new Artist { Name = text };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(" Dave   Davington    2   ");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var updatedArtist = await service.UpdateArtist(searchPublicId, dto, CancellationToken.None);

        // Assert
        updatedArtist.Should().NotBeNull();
        updatedArtist!.Name.Should().Be(trueText);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, trueText, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateArtist_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var artist = new Artist { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto("      ");
        var repo = new Mock<IArtistRepository>();
        var commit = new Mock<ICommit>();
        var service = new ArtistService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateArtist(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(Artist.Name)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
