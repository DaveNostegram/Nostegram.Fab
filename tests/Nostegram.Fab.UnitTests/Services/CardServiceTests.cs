// using FluentAssertions;
// using Moq;
// using Nostegram.Fab.Application.Common.Interfaces;
// using Nostegram.Fab.Application.Exceptions;
// using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
// using Nostegram.Fab.Application.ReferenceData.Cards;
// using Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;
// using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
// using Nostegram.Fab.Contracts.Cards;
// using Nostegram.Fab.Contracts.Common;
// using Nostegram.Fab.Domain;
// using Nostegram.Fab.Infrastructure.Persistence.Repositories;
// using Xunit;

// namespace Nostegram.Fab.UnitTests.Services;

// public sealed class CardServiceTests()
// {
//     [Fact]
//     public async Task CreateCard_Valid_ReturnsPublicId()
//     {
//         // Arrange
//         var text = "Card 1";
//         var artist = new Artist { Name = "Artist 1" };
//         var set = new Set { Name = "Set 1", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };

//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = set.PublicId,
//                             CollectorNumber = "001",
//                             ArtistId = artist.PublicId
//                         }
//                     ]

//                 }
//             ]
//         };
//         var cardRepo = new Mock<ICardRepository>();
//         var artistRepo = new Mock<IArtistRepository>();
//         var setRepo = new Mock<ISetRepository>();
//         var commit = new Mock<ICommit>();
//         //Card repo setup
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         Card? createdCard = null;
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

//         //artist Repo
//         artistRepo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);

//         //set Repo
//         setRepo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);

//         var service = new CardService(commit.Object, cardRepo.Object);

//         // Act
//         var result = await service.CreateCard(dto, CancellationToken.None);

//         // Assert
//         createdCard.Should().NotBeNull();
//         createdCard!.Name.Should().Be(dto.Name);
//         createdCard.CardVariants.Count.Should().Be(1);
//         createdCard.CardVariants.First().SetDetails.First().ArtistId.Should().Be(artist.Id);
//         createdCard.CardVariants.First().SetDetails.First().SetId.Should().Be(set.Id);
//         result.PublicId.Should().Be(createdCard.PublicId);
//         result.Name.Should().Be(createdCard.Name);

//         cardRepo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
//         cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Once);
//         artistRepo.Verify(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once);
//         setRepo.Verify(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>()), Times.Once);

//         commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
//     }

//     [Fact]
//     public async Task CreateCard_WithSpaces_TrimsCorrectly()
//     {
//         // Arrange
//         var text = " Card   1  ";
//         var trueText = "Card 1";
//         var artist = new Artist { Name = "Artist 1" };
//         var set = new Set { Name = "Set 1", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };

//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = set.PublicId,
//                             CollectorNumber = "001",
//                             ArtistId = artist.PublicId
//                         }
//                     ]

//                 }
//             ]
//         };
//         var cardRepo = new Mock<ICardRepository>();
//         var artistRepo = new Mock<IArtistRepository>();
//         var setRepo = new Mock<ISetRepository>();
//         var commit = new Mock<ICommit>();
//         //Card repo setup
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         Card? createdCard = null;
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

//         //artist Repo
//         artistRepo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);

//         //set Repo
//         setRepo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(set);

//         var service = new CardService(commit.Object, cardRepo.Object);

//         // Act
//         var result = await service.CreateCard(dto, CancellationToken.None);

//         // Assert
//         createdCard.Should().NotBeNull();
//         createdCard!.Name.Should().Be(dto.Name);
//         createdCard.CardVariants.Count.Should().Be(1);
//         createdCard.CardVariants.First().SetDetails.First().ArtistId.Should().Be(artist.Id);
//         createdCard.CardVariants.First().SetDetails.First().SetId.Should().Be(set.Id);
//         result.PublicId.Should().Be(createdCard.PublicId);
//         result.Name.Should().Be(createdCard.Name);

//         cardRepo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
//         cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Once);
//         artistRepo.Verify(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>()), Times.Once);
//         setRepo.Verify(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>()), Times.Once);

//         commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
//     }

//     [Fact]
//     public async Task CreateCard_AlreadyExists_ThrowsAlreadyExistsException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ]

//                 }
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(true);
//         // Act
//         var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_InvalidNameAfterNormalise_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "           ";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ]

//                 }
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }

//     [Fact]
//     public async Task CreateCard_RequiresAtLeastOneCardVariant_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be("Finish this assert");
//         ex.Message.Should().Be($"Card is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_AllCardVariantRequireAtLeastOneSetDetail_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red

//                 },
//                 new CardVariantWriteDto
//                 {
//                     Pitch = Contracts.PitchEnumAPI.Blue
//                 },
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be("Finish this assert");
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_CannotHaveDuplicatePitch_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red

//                 },
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red
//                 },
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be("Finish this assert");
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_SetDetailMustHaveRarity_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red

//                 },
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be("Finish this assert");
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_SetDetailMustHaveSetId_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red

//                 },
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_SetDetailMustHaveCollectorNumber_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ],
//                     Pitch = Contracts.PitchEnumAPI.Red

//                 },
//             ]
//         };
//         var repo = new Mock<ICardRepository>();
//         var commit = new Mock<ICommit>();
//         var service = new CardService(commit.Object, repo.Object);

//         repo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<RequiredFieldException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be("Finish this assert");
//         ex.Message.Should().Be($"'{nameof(Card.Name)}' is required.");

//         repo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_SetDetailSetMustExist_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var artist = new Artist { Name = "Artist 1" };

//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = Guid.NewGuid(),
//                             CollectorNumber = "001",
//                             ArtistId = artist.PublicId
//                         }
//                     ]

//                 }
//             ]
//         };
//         var cardRepo = new Mock<ICardRepository>();
//         var artistRepo = new Mock<IArtistRepository>();
//         var setRepo = new Mock<ISetRepository>();
//         var commit = new Mock<ICommit>();
//         //Card repo setup
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         Card? createdCard = null;
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

//         //artist Repo
//         artistRepo.Setup(r => r.GetByPublicId(artist.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(artist);

//         //set Repo
//         setRepo.Setup(r => r.GetByPublicId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Set?)null);

//         var service = new CardService(commit.Object, cardRepo.Object);
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<NotFoundException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be($"Set not found");
//         cardRepo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
//         setRepo.Verify(r => r.GetByPublicId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
//         cardRepo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
//     [Fact]
//     public async Task CreateCard_SetDetailArtistMustExist_ThrowsRequiredFieldException()
//     {
//         // Arrange
//         var text = "Card 1";
//         var artist = new Artist { Name = "Artist 1" };
//         var set = new Set { Name = "Set 1", SetCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };

//         var dto = new CardWriteDto
//         {
//             Name = text,
//             CardVariants =
//             [
//                 new CardVariantWriteDto
//                 {
//                     SetDetails = [
//                         new() {
//                             Rarity = Contracts.RarityEnumAPI.Common,
//                             SetId = set.PublicId,
//                             CollectorNumber = "001",
//                             ArtistId = Guid.NewGuid()
//                         }
//                     ]

//                 }
//             ]
//         };
//         var cardRepo = new Mock<ICardRepository>();
//         var artistRepo = new Mock<IArtistRepository>();
//         var setRepo = new Mock<ISetRepository>();
//         var commit = new Mock<ICommit>();
//         //Card repo setup
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         Card? createdCard = null;
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);
//         cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

//         //artist Repo
//         artistRepo.Setup(r => r.GetByPublicId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Artist?)null);

//         //set Repo
//         setRepo.Setup(r => r.GetByPublicId(set.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((set));

//         var service = new CardService(commit.Object, cardRepo.Object);
//         cardRepo.Setup(r => r.ExistsByName(dto.Name, CancellationToken.None))
//             .ReturnsAsync(false);
//         // Act
//         var ex = await Assert.ThrowsAsync<NotFoundException>(
//             () => service.CreateCard(dto, CancellationToken.None));
//         // Assert
//         ex.Message.Should().Be($"Artist not found");
//         cardRepo.Verify(r => r.ExistsByName(dto.Name, CancellationToken.None), Times.Once);
//         artistRepo.Verify(r => r.GetByPublicId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
//         cardRepo.Verify(e => e.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never());
//         commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//     }
// }