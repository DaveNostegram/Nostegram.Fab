using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Talents;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;
using Xunit;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class TalentServiceTests
{
    [Fact]
    public async Task CreateTalent_ValidName_ReturnsPublicId()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        Talent? createdTalent = null;

        repo.Setup(r => r.Create(It.IsAny<Talent>())).Callback<Talent>(artist => createdTalent = artist);

        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateTalent(dto, CancellationToken.None);

        // Assert
        createdTalent.Should().NotBeNull();
        createdTalent!.Name.Should().Be(dto.Name);

        result.PublicId.Should().Be(createdTalent.PublicId);
        result.Name.Should().Be(createdTalent.Name);

        repo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Talent>(a => a.Name == dto.Name)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateTalent_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington";
        var dto = new LookupItemWriteDto(text);

        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(trueText, CancellationToken.None))
            .ReturnsAsync(false);

        Talent? createdTalent = null;

        repo.Setup(r => r.Create(It.IsAny<Talent>())).Callback<Talent>(artist => createdTalent = artist);

        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateTalent(dto, CancellationToken.None);

        // Assert
        createdTalent.Should().NotBeNull();
        createdTalent!.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdTalent.PublicId);
        result.Name.Should().Be(createdTalent.Name);

        repo.Verify(r => r.ExistsByName(trueText, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Talent>(a => a.Name == trueText)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateTalent_AlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateTalent(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        repo.Verify(e => e.Create(It.Is<Talent>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateTalent_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("           ");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateTalent(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(Talent.Name)}' is required.");

        repo.Verify(e => e.Create(It.Is<Talent>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTalent_ValidPublicId_ReturnsDto()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");

        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var result = await service.GetTalent(dto.PublicId, CancellationToken.None);
        // Assert
        result.PublicId.Should().Be(dto.PublicId);
        result.Name.Should().Be(dto.Name);
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task GetTalent_InvalidPublicId_ReturnsNotFoundException()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((LookupItemDto?)null);
        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetTalent(dto.PublicId, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{dto.PublicId}' not found.");
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllTalents_ReturnsTalents()
    {
        // Arrange
        var dtos = new List<LookupItemDto>
        {
            new(Guid.NewGuid(),"Dave Davington 1"),
            new(Guid.NewGuid(),"Dave Davington 2")
        };

        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllTalents(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllTalents_NoTalents_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<LookupItemDto>();

        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new TalentService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllTalents(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteTalent_ValidPublicId_Deletes()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        await service.DeleteTalent(artist.PublicId, CancellationToken.None);
        // Assert
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteTalent_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Talent?)null);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.DeleteTalent(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.PublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Never());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DeleteTalent_Conflict_ReturnsConflictException()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(
                    () => service.DeleteTalent(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.Name}' is used by a 'Card'.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task UpdateTalent_ValidPublicIdAndName_Updates()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var updatedTalent = await service.UpdateTalent(searchPublicId, dto, CancellationToken.None);
        // Assert
        updatedTalent.Should().NotBeNull();
        updatedTalent.Name.Should().Be(dto.Name);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateTalent_InvalidPublicId_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var searchPublicId = Guid.Empty;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Talent?)null);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.UpdateTalent(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{searchPublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateTalent_InvalidName_ThrowsAlreadyExistsException()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateTalent(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateTalent_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington 2";

        var artist = new Talent { Name = text };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(" Dave   Davington    2   ");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var updatedTalent = await service.UpdateTalent(searchPublicId, dto, CancellationToken.None);

        // Assert
        updatedTalent.Should().NotBeNull();
        updatedTalent!.Name.Should().Be(trueText);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, trueText, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateTalent_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var artist = new Talent { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto("      ");
        var repo = new Mock<ITalentRepository>();
        var commit = new Mock<ICommit>();
        var service = new TalentService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateTalent(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(Talent.Name)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
