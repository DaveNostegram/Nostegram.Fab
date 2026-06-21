using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.FabClasses;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;
using Xunit;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class FabClassServiceTests
{
    [Fact]
    public async Task CreateFabClass_ValidName_ReturnsPublicId()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        FabClass? createdFabClass = null;

        repo.Setup(r => r.Create(It.IsAny<FabClass>())).Callback<FabClass>(artist => createdFabClass = artist);

        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateFabClass(dto, CancellationToken.None);

        // Assert
        createdFabClass.Should().NotBeNull();
        createdFabClass!.Name.Should().Be(dto.Name);

        result.PublicId.Should().Be(createdFabClass.PublicId);
        result.Name.Should().Be(createdFabClass.Name);

        repo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<FabClass>(a => a.Name == dto.Name)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateFabClass_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington";
        var dto = new LookupItemWriteDto(text);

        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(trueText, CancellationToken.None))
            .ReturnsAsync(false);

        FabClass? createdFabClass = null;

        repo.Setup(r => r.Create(It.IsAny<FabClass>())).Callback<FabClass>(artist => createdFabClass = artist);

        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateFabClass(dto, CancellationToken.None);

        // Assert
        createdFabClass.Should().NotBeNull();
        createdFabClass!.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdFabClass.PublicId);
        result.Name.Should().Be(createdFabClass.Name);

        repo.Verify(r => r.ExistsByName(trueText, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<FabClass>(a => a.Name == trueText)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateFabClass_AlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("Dave Davington");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateFabClass(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        repo.Verify(e => e.Create(It.Is<FabClass>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateFabClass_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new LookupItemWriteDto("           ");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateFabClass(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(FabClass.Name)}' is required.");

        repo.Verify(e => e.Create(It.Is<FabClass>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetFabClass_ValidPublicId_ReturnsDto()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");

        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var result = await service.GetFabClass(dto.PublicId, CancellationToken.None);
        // Assert
        result.PublicId.Should().Be(dto.PublicId);
        result.Name.Should().Be(dto.Name);
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task GetFabClass_InvalidPublicId_ReturnsNotFoundException()
    {
        // Arrange
        var dto = new LookupItemDto(Guid.NewGuid(), "Dave Davington");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((LookupItemDto?)null);
        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetFabClass(dto.PublicId, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{dto.PublicId}' not found.");
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllFabClasses_ReturnsFabClasses()
    {
        // Arrange
        var dtos = new List<LookupItemDto>
        {
            new(Guid.NewGuid(),"Dave Davington 1"),
            new(Guid.NewGuid(),"Dave Davington 2")
        };

        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllFabClasses(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllFabClasses_NoFabClasses_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<LookupItemDto>();

        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new FabClassService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllFabClasses(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteFabClass_ValidPublicId_Deletes()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        await service.DeleteFabClass(artist.PublicId, CancellationToken.None);
        // Assert
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteFabClass_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((FabClass?)null);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.DeleteFabClass(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.PublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Never());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DeleteFabClass_Conflict_ReturnsConflictException()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.IsUsed(artist.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.Delete(artist));
        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(
                    () => service.DeleteFabClass(artist.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{artist.Name}' is used by a 'Card'.");
        repo.Verify(e => e.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(artist.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(artist), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task UpdateFabClass_ValidPublicIdAndName_Updates()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var updatedFabClass = await service.UpdateFabClass(searchPublicId, dto, CancellationToken.None);
        // Assert
        updatedFabClass.Should().NotBeNull();
        updatedFabClass.Name.Should().Be(dto.Name);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateFabClass_InvalidPublicId_ThrowsNotFoundException()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var searchPublicId = Guid.Empty;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((FabClass?)null);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.UpdateFabClass(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{searchPublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateFabClass_InvalidName_ThrowsAlreadyExistsException()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(artist.Name + 2);
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateFabClass(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateFabClass_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var trueText = "Dave Davington 2";

        var artist = new FabClass { Name = text };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto(" Dave   Davington    2   ");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var updatedFabClass = await service.UpdateFabClass(searchPublicId, dto, CancellationToken.None);

        // Assert
        updatedFabClass.Should().NotBeNull();
        updatedFabClass!.Name.Should().Be(trueText);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, trueText, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateFabClass_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var artist = new FabClass { Name = "Dave Davington" };
        var searchPublicId = artist.PublicId;
        var dto = new LookupItemWriteDto("      ");
        var repo = new Mock<IFabClassRepository>();
        var commit = new Mock<ICommit>();
        var service = new FabClassService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);
        repo.Setup(r => r.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateFabClass(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(FabClass.Name)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.ExistsByNameExcludingId(artist.Id, dto.Name, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
