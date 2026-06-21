using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;
using Xunit;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class CardSubTypeServiceTests
{
    [Fact]
    public async Task CreateCardSubType_ValidName_ReturnsPublicId()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        CardSubType? createdCardSubType = null;

        repo.Setup(r => r.Create(It.IsAny<CardSubType>())).Callback<CardSubType>(artist => createdCardSubType = artist);

        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateCardSubType(dto, CancellationToken.None);

        // Assert
        createdCardSubType.Should().NotBeNull();
        createdCardSubType!.Name.Should().Be(dto.Name);

        result.PublicId.Should().Be(createdCardSubType.PublicId);
        result.Name.Should().Be(createdCardSubType.Name);

        repo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<CardSubType>(a => a.Name == dto.Name)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateCardSubType_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington";
        var dto = new LookupItemWriteDto(text);

        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(trueText, CancellationToken.None))
            .ReturnsAsync(false);

        CardSubType? createdCardSubType = null;

        repo.Setup(r => r.Create(It.IsAny<CardSubType>())).Callback<CardSubType>(artist => createdCardSubType = artist);

        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateCardSubType(dto, CancellationToken.None);

        // Assert
        createdCardSubType.Should().NotBeNull();
        createdCardSubType!.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdCardSubType.PublicId);
        result.Name.Should().Be(createdCardSubType.Name);

        repo.Verify(r => r.ExistsByName(trueText, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<CardSubType>(a => a.Name == trueText)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateCardSubType_AlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateCardSubType(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        repo.Verify(e => e.Create(It.Is<CardSubType>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateCardSubType_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("           ");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateCardSubType(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(CardSubType.Name)}' is required.");

        repo.Verify(e => e.Create(It.Is<CardSubType>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCardSubType_ValidPublicId_ReturnsDto()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");

        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var result = await service.GetCardSubType(dto.PublicId, CancellationToken.None);
        // Assert
        result.PublicId.Should().Be(dto.PublicId);
        result.Name.Should().Be(dto.Name);
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task GetCardSubType_InvalidPublicId_ReturnsNotFoundException()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((LookupItemDto?)null);
        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetCardSubType(dto.PublicId, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{dto.PublicId}' not found.");
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllCardSubTypes_ReturnsCardSubTypes()
    {
        // Arrange
        var dtos = new List<LookupItemDto>
        {
            new(Guid.NewGuid(),"Dave Davington 1"),
            new(Guid.NewGuid(),"Dave Davington 2")
        };

        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllCardSubTypes(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllCardSubTypes_NoCardSubTypes_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<LookupItemDto>();

        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new CardSubTypeService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllCardSubTypes(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteCardSubType_ValidPublicId_Deletes()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        await service.DeleteCardSubType(artist.PublicId, CancellationToken.None);
        // Assert
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteCardSubType_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((CardSubType?)null);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.DeleteCardSubType(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.PublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Never());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DeleteCardSubType_Conflict_ReturnsConflictException()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(
                    () => service.DeleteCardSubType(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.Name}' is used by a 'Card'.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task UpdateCardSubType_ValidPublicIdAndName_Updates()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var updatedCardSubType = await service.UpdateCardSubType(searchPublicId, dto, CancellationToken.None);
        // Assert
        updatedCardSubType.Should().NotBeNull();
        updatedCardSubType.Name.Should().Be(dto.Name);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateCardSubType_InvalidPublicId_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var searchPublicId = Guid.Empty;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((CardSubType?)null);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.UpdateCardSubType(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{searchPublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateCardSubType_InvalidName_ThrowsAlreadyExistsException()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateCardSubType(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateCardSubType_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington 2";

        var artist = new CardSubType { Name = text };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(" Dave   Davington    2   ");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var updatedCardSubType = await service.UpdateCardSubType(searchPublicId, dto, CancellationToken.None);

        // Assert
        updatedCardSubType.Should().NotBeNull();
        updatedCardSubType!.Name.Should().Be(trueText);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, trueText, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateCardSubType_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var artist = new CardSubType { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto("      ");
        var repo = new Mock<ICardSubTypeRepository>();
        var commit = new Mock<ICommit>();
        var service = new CardSubTypeService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateCardSubType(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(CardSubType.Name)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
