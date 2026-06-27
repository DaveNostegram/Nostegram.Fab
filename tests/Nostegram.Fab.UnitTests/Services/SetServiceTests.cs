using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Sets;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Sets.Results;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;
using Xunit;
using Xunit.Sdk;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class SetServiceTests
{
    [Fact]
    public async Task CreateSet_ValidName_ReturnsPublicId()
    {
        // Arrange
        var dto = new SetWriteDto("MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));

        Set? createdSet = null;

        repo.Setup(r => r.Create(It.IsAny<Set>())).Callback<Set>(set => createdSet = set);

        var service = new SetService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateSet(dto, CancellationToken.None);

        // Assert
        createdSet.Should().NotBeNull();
        createdSet!.Name.Should().Be(dto.Name);

        result.PublicId.Should().Be(createdSet.PublicId);
        result.Name.Should().Be(createdSet.Name);

        repo.Verify(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateSet_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var text = " Dave   Davington  ";
        var setCode = " MST ";
        var trueText = "Dave Davington";
        var trueSetCode = "MST";
        var dto = new SetWriteDto(text, setCode, DateOnly.FromDateTime(DateTime.Now));

        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.CheckUniqueness(trueText, trueSetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));

        Set? createdSet = null;

        repo.Setup(r => r.Create(It.IsAny<Set>())).Callback<Set>(set => createdSet = set);

        var service = new SetService(commit.Object, repo.Object);

        // Act
        var result = await service.CreateSet(dto, CancellationToken.None);

        // Assert
        createdSet.Should().NotBeNull();
        createdSet!.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdSet.PublicId);
        result.Name.Should().Be(createdSet.Name);

        repo.Verify(r => r.CheckUniqueness(trueText, trueSetCode, null, CancellationToken.None), Times.Once);
        repo.Verify(r => r.Create(It.Is<Set>(a => a.Name == trueText)), Times.Once);

        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateSet_NameAlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new SetWriteDto("MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(true, false));

        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSet_CodeAlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new SetWriteDto("MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, true));

        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"SetCode '{dto.SetCode}' already exists.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.SetCode == dto.SetCode)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSet_NameAndCodeAlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var dto = new SetWriteDto("MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(true, true));

        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' and SetCode '{dto.SetCode}' already exist.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateSet_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new SetWriteDto("            ", "SET", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(Set.Name)}' is required.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CreateSet_InvalidSetCodeAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new SetWriteDto("Mistveil", "   ", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(Set.SetCode)}' is required.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSet_InvalidNameAndSetCodeAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var dto = new SetWriteDto("         ", "   ", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
            () => service.CreateSet(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{nameof(Set.Name)}' and '{nameof(Set.SetCode)}' are required.");

        repo.Verify(e => e.Create(It.Is<Set>(a => a.Name == dto.Name)), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSet_ValidPublicId_ReturnsDto()
    {
        // Arrange
        var dto = new SetDto(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));

        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var service = new SetService(commit.Object, repo.Object);

        // Act
        var result = await service.GetSet(dto.PublicId, CancellationToken.None);
        // Assert
        result.PublicId.Should().Be(dto.PublicId);
        result.Name.Should().Be(dto.Name);
        result.SetCode.Should().Be(dto.SetCode);
        result.ReleaseDate.Should().Be(dto.ReleaseDate);
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task GetSet_InvalidPublicId_ReturnsNotFoundException()
    {
        // Arrange
        var dto = new SetDto(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, null, CancellationToken.None))
            .ReturnsAsync(new SetUniquenessResult(false, false));

        repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((SetDto?)null);
        var service = new SetService(commit.Object, repo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetSet(dto.PublicId, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"'{dto.PublicId}' not found.");
        repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllSets_ReturnsSets()
    {
        // Arrange
        var dtos = new List<SetDto>
        {
            new(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now)),
            new(Guid.NewGuid(), "Super Slam", "SLM", DateOnly.FromDateTime(DateTime.Now))
        };

        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new SetService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllSets(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetAllSets_NoSets_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<SetDto>();

        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();

        repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
        var service = new SetService(commit.Object, repo.Object);

        // Act
        var result = await service.GetAllSets(CancellationToken.None);
        // Assert
        result.Count.Should().Be(dtos.Count);
        repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteSet_ValidPublicId_Deletes()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.IsUsed(set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(set));
        // Act
        await service.DeleteSet(set.PublicId, CancellationToken.None);
        // Assert
        repo.Verify(e => e.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(set.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(set), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task DeleteSet_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Set?)null);
        repo.Setup(r => r.IsUsed(set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.Delete(set));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.DeleteSet(set.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{set.PublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(set.Id, It.IsAny<CancellationToken>()), Times.Never());
        repo.Verify(e => e.Delete(set), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DeleteSet_Conflict_ReturnsConflictException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.IsUsed(set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.Delete(set));
        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(
                    () => service.DeleteSet(set.PublicId, CancellationToken.None));

        // Assert
        ex.Message.Should().Be($"'{set.Name}' is used by a 'Card'.");
        repo.Verify(e => e.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.IsUsed(set.Id, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.Delete(set), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task UpdateSet_ValidPublicIdAndName_Updates()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));

        // Act
        var updatedSet = await service.UpdateSet(searchPublicId, dto, CancellationToken.None);
        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Name.Should().Be(dto.Name);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateSet_InvalidPublicId_ThrowsNotFoundException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = Guid.Empty;
        var dto = new SetWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Set?)null);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{searchPublicId}' not found.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_InvalidName_ThrowsAlreadyExistsException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(true, false));
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_InvalidSet_ThrowsAlreadyExistsException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, true));
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"SetCode '{dto.SetCode}' already exists.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_InvalidNameAndSetCode_ThrowsAlreadyExistsException()
    {
        // Arrange
        var set = new Set { Name = "Mistveil", SetCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(true, true));
        // Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"Name '{dto.Name}' and SetCode '{dto.SetCode}' already exist.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_WithSpaces_TrimsCorrectly()
    {
        // Arrange
        var name = " Dave   Davington 2 ";
        var trueName = "Dave Davington 2";
        var setCode = " MST ";
        var trueSetCode = "MST";
        var set = new Set { Name = "SetName", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto(name, setCode, DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(trueName, trueSetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var updatedSet = await service.UpdateSet(searchPublicId, dto, CancellationToken.None);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.Name.Should().Be(trueName);
        updatedSet!.SetCode.Should().Be(trueSetCode);

        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(trueName, trueSetCode, set.Id, It.IsAny<CancellationToken>()), Times.Once());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
    [Fact]
    public async Task UpdateSet_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var set = new Set { Name = "SetName", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("            ", "SET", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(Set.Name)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_InvalidSetCodeAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var set = new Set { Name = "SetName", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("SetName", "   ", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(Set.SetCode)}' is required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UpdateSet_InvalidNameAndSetCodeAfterNormalise_ThrowsRequiredFieldException()
    {
        // Arrange
        var set = new Set { Name = "SetName", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
        var searchPublicId = set.PublicId;
        var dto = new SetWriteDto("      ", "   ", DateOnly.FromDateTime(DateTime.Now));
        var repo = new Mock<ISetRepository>();
        var commit = new Mock<ICommit>();
        var service = new SetService(commit.Object, repo.Object);

        repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);
        repo.Setup(r => r.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new SetUniquenessResult(false, false));
        // Act
        var ex = await Assert.ThrowsAsync<RequiredFieldException>(
                    () => service.UpdateSet(searchPublicId, dto, CancellationToken.None));

        // Assert     
        ex.Message.Should().Be($"'{nameof(Set.Name)}' and '{nameof(Set.SetCode)}' are required.");
        repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
        repo.Verify(e => e.CheckUniqueness(dto.Name, dto.SetCode, set.Id, It.IsAny<CancellationToken>()), Times.Never());
        commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
