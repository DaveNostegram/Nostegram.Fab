using FluentAssertions;
using Moq;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Cards;
using Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Domain;
using Xunit;
using Xunit.Sdk;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Application.Exceptions;

namespace Nostegram.Fab.UnitTests.Services;

public sealed class CardServiceTests
{
    [Fact]
    public async Task CreateCard_ValidCard_ReturnsDto()
    {
        // Arrange
        var lightningTalent = Guid.NewGuid();
        var actionType = Guid.NewGuid();
        var attackSubType = Guid.NewGuid();
        var auroraSetId = Guid.NewGuid();
        var fryArtist = Guid.NewGuid();
        var dto = new CardWriteDto
        {
            Name = "Fry",
            Talents = [lightningTalent],
            CardTypes = [actionType],
            CardSubTypes = [attackSubType],
            CardVariants = [
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 3,
                Pitch = Contracts.PitchEnumAPI.Red,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR008",
                        ArtistId = fryArtist
                        }
                    ]
                },
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 2,
                Pitch = Contracts.PitchEnumAPI.Yellow,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR016",
                        ArtistId = fryArtist
                        }
                    ]
                }
            ]
        };

        var cardRepo = new Mock<ICardRepository>();
        var talentRepo = new Mock<ITalentRepository>();
        var cardTypeRepo = new Mock<ICardTypeRepository>();
        var cardSubTypeRepo = new Mock<ICardSubTypeRepository>();
        var fabClassRepo = new Mock<IFabClassRepository>();
        var artistRepo = new Mock<IArtistRepository>();
        var setRepo = new Mock<ISetRepository>();

        cardRepo.Setup(cr => cr.Exists(dto.Name, null, CancellationToken.None))
            .ReturnsAsync(false);
        talentRepo.Setup(tr => tr.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, CancellationToken.None))
            .ReturnsAsync([new Talent { Name = "Lightning" }]);
        cardTypeRepo.Setup(ctr => ctr.GetCardTypesByPublicIds(new List<Guid> { actionType }, CancellationToken.None))
            .ReturnsAsync([new CardType { Name = "Action" }]);
        cardSubTypeRepo.Setup(cstr => cstr.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, CancellationToken.None))
            .ReturnsAsync([new CardSubType { Name = "Attack" }]);
        fabClassRepo.Setup(fbr => fbr.GetFabClassesByPublicIds(new List<Guid>(), CancellationToken.None))
            .ReturnsAsync([]);
        artistRepo.Setup(ar => ar.GetByPublicId(fryArtist, CancellationToken.None))
            .ReturnsAsync(new Artist { Name = "Edward Chee" });
        setRepo.Setup(sr => sr.GetByPublicId(auroraSetId, CancellationToken.None))
            .ReturnsAsync(new Set { Name = "1st Strike: Aurora", SetCode = "AUR", ReleaseDate = new DateOnly(2024, 8, 1) });

        var commit = new Mock<ICommit>();

        Card? createdCard = null;

        cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

        var service = new CardService(commit.Object, cardRepo.Object, talentRepo.Object, cardTypeRepo.Object, cardSubTypeRepo.Object, fabClassRepo.Object, artistRepo.Object, setRepo.Object);

        // Act
        var result = await service.CreateCard(dto, CancellationToken.None);

        // Assert
        createdCard.Should().NotBeNull();
        createdCard.Name.Should().Be(dto.Name);
        createdCard.CardSubTypes.Count.Should().Be(1);
        createdCard.CardSubTypes.First().Name.Should().Be("Attack");
        createdCard.CardTypes.Count.Should().Be(1);
        createdCard.CardTypes.First().Name.Should().Be("Action");
        createdCard.Talents.Count.Should().Be(1);
        createdCard.Talents.First().Name.Should().Be("Lightning");
        createdCard.FabClasses.Count.Should().Be(0);
        createdCard.CardVariants.Count.Should().Be(2);
        var cardVariant1 = createdCard.CardVariants.First(cv => cv.Pitch == PitchEnum.Red);
        cardVariant1.CardText.Should().Be("Go again");
        cardVariant1.Cost.Should().Be(0);
        cardVariant1.Block.Should().Be(0);
        cardVariant1.Power.Should().Be(3);
        cardVariant1.Health.Should().BeNull();
        cardVariant1.Intellect.Should().BeNull();
        cardVariant1.SetDetails.Count.Should().Be(1);
        cardVariant1.SetDetails.First().Artist.Name.Should().Be("Edward Chee");
        cardVariant1.SetDetails.First().Set.Name.Should().Be("1st Strike: Aurora");
        cardVariant1.SetDetails.First().Set.SetCode.Should().Be("AUR");
        cardVariant1.SetDetails.First().Rarity.Should().Be(RarityEnum.Common);
        cardVariant1.SetDetails.First().CollectorNumber.Should().Be("AUR008");

        var cardVariant2 = createdCard.CardVariants.First(cv => cv.Pitch == PitchEnum.Yellow);
        cardVariant2.CardText.Should().Be("Go again");
        cardVariant2.Cost.Should().Be(0);
        cardVariant2.Block.Should().Be(0);
        cardVariant2.Power.Should().Be(2);
        cardVariant2.Health.Should().BeNull();
        cardVariant2.Intellect.Should().BeNull();
        cardVariant2.SetDetails.Count.Should().Be(1);
        cardVariant2.SetDetails.First().Artist.Name.Should().Be("Edward Chee");
        cardVariant2.SetDetails.First().Set.Name.Should().Be("1st Strike: Aurora");
        cardVariant2.SetDetails.First().Set.SetCode.Should().Be("AUR");
        cardVariant2.SetDetails.First().Rarity.Should().Be(RarityEnum.Common);
        cardVariant2.SetDetails.First().CollectorNumber.Should().Be("AUR016");

        result.PublicId.Should().Be(createdCard.PublicId);
        result.Name.Should().Be(createdCard.Name);

        cardRepo.Verify(r => r.Exists(dto.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        talentRepo.Verify(r => r.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, It.IsAny<CancellationToken>()), Times.Once);
        cardTypeRepo.Verify(r => r.GetCardTypesByPublicIds(new List<Guid> { actionType }, It.IsAny<CancellationToken>()), Times.Once);
        cardSubTypeRepo.Verify(r => r.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, It.IsAny<CancellationToken>()), Times.Once);
        fabClassRepo.Verify(r => r.GetFabClassesByPublicIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        artistRepo.Verify(r => r.GetByPublicId(fryArtist, It.IsAny<CancellationToken>()), Times.Exactly(2));
        setRepo.Verify(r => r.GetByPublicId(auroraSetId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Once);
        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateCard_ValidCardWithSpaces_TrimsName()
    {
        var trueText = "Fry Fry";
        var text = "   Fry    Fry   ";
        // Arrange
        var lightningTalent = Guid.NewGuid();
        var actionType = Guid.NewGuid();
        var attackSubType = Guid.NewGuid();
        var auroraSetId = Guid.NewGuid();
        var fryArtist = Guid.NewGuid();
        var dto = new CardWriteDto
        {
            Name = text,
            Talents = [lightningTalent],
            CardTypes = [actionType],
            CardSubTypes = [attackSubType],
            CardVariants = [
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 3,
                Pitch = Contracts.PitchEnumAPI.Red,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR008",
                        ArtistId = fryArtist
                        }
                    ]
                },
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 2,
                Pitch = Contracts.PitchEnumAPI.Yellow,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR016",
                        ArtistId = fryArtist
                        }
                    ]
                }
            ]
        };

        var cardRepo = new Mock<ICardRepository>();
        var talentRepo = new Mock<ITalentRepository>();
        var cardTypeRepo = new Mock<ICardTypeRepository>();
        var cardSubTypeRepo = new Mock<ICardSubTypeRepository>();
        var fabClassRepo = new Mock<IFabClassRepository>();
        var artistRepo = new Mock<IArtistRepository>();
        var setRepo = new Mock<ISetRepository>();

        cardRepo.Setup(cr => cr.Exists(dto.Name, null, CancellationToken.None))
            .ReturnsAsync(false);
        talentRepo.Setup(tr => tr.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, CancellationToken.None))
            .ReturnsAsync([new Talent { Name = "Lightning" }]);
        cardTypeRepo.Setup(ctr => ctr.GetCardTypesByPublicIds(new List<Guid> { actionType }, CancellationToken.None))
            .ReturnsAsync([new CardType { Name = "Action" }]);
        cardSubTypeRepo.Setup(cstr => cstr.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, CancellationToken.None))
            .ReturnsAsync([new CardSubType { Name = "Attack" }]);
        fabClassRepo.Setup(fbr => fbr.GetFabClassesByPublicIds(new List<Guid>(), CancellationToken.None))
            .ReturnsAsync([]);
        artistRepo.Setup(ar => ar.GetByPublicId(fryArtist, CancellationToken.None))
            .ReturnsAsync(new Artist { Name = "Edward Chee" });
        setRepo.Setup(sr => sr.GetByPublicId(auroraSetId, CancellationToken.None))
            .ReturnsAsync(new Set { Name = "1st Strike: Aurora", SetCode = "AUR", ReleaseDate = new DateOnly(2024, 8, 1) });

        var commit = new Mock<ICommit>();

        Card? createdCard = null;

        cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

        var service = new CardService(commit.Object, cardRepo.Object, talentRepo.Object, cardTypeRepo.Object, cardSubTypeRepo.Object, fabClassRepo.Object, artistRepo.Object, setRepo.Object);

        // Act
        var result = await service.CreateCard(dto, CancellationToken.None);

        // Assert
        createdCard.Should().NotBeNull();
        createdCard.Name.Should().Be(trueText);

        result.PublicId.Should().Be(createdCard.PublicId);
        result.Name.Should().Be(trueText);

        cardRepo.Verify(r => r.Exists(dto.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        talentRepo.Verify(r => r.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, It.IsAny<CancellationToken>()), Times.Once);
        cardTypeRepo.Verify(r => r.GetCardTypesByPublicIds(new List<Guid> { actionType }, It.IsAny<CancellationToken>()), Times.Once);
        cardSubTypeRepo.Verify(r => r.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, It.IsAny<CancellationToken>()), Times.Once);
        fabClassRepo.Verify(r => r.GetFabClassesByPublicIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        artistRepo.Verify(r => r.GetByPublicId(fryArtist, It.IsAny<CancellationToken>()), Times.Exactly(2));
        setRepo.Verify(r => r.GetByPublicId(auroraSetId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Once);
        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    [Fact]
    public async Task CreateCard_NameAlreadyExists_ThrowsAlreadyExistsException()
    {
        // Arrange
        var lightningTalent = Guid.NewGuid();
        var actionType = Guid.NewGuid();
        var attackSubType = Guid.NewGuid();
        var auroraSetId = Guid.NewGuid();
        var fryArtist = Guid.NewGuid();
        var dto = new CardWriteDto
        {
            Name = "Fry",
            Talents = [lightningTalent],
            CardTypes = [actionType],
            CardSubTypes = [attackSubType],
            CardVariants = [
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 3,
                Pitch = Contracts.PitchEnumAPI.Red,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR008",
                        ArtistId = fryArtist
                        }
                    ]
                },
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 2,
                Pitch = Contracts.PitchEnumAPI.Yellow,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR016",
                        ArtistId = fryArtist
                        }
                    ]
                }
            ]
        };

        var cardRepo = new Mock<ICardRepository>();
        var talentRepo = new Mock<ITalentRepository>();
        var cardTypeRepo = new Mock<ICardTypeRepository>();
        var cardSubTypeRepo = new Mock<ICardSubTypeRepository>();
        var fabClassRepo = new Mock<IFabClassRepository>();
        var artistRepo = new Mock<IArtistRepository>();
        var setRepo = new Mock<ISetRepository>();

        cardRepo.Setup(cr => cr.Exists(dto.Name, null, CancellationToken.None))
            .ReturnsAsync(true);
        talentRepo.Setup(tr => tr.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, CancellationToken.None))
            .ReturnsAsync([new Talent { Name = "Lightning" }]);
        cardTypeRepo.Setup(ctr => ctr.GetCardTypesByPublicIds(new List<Guid> { actionType }, CancellationToken.None))
            .ReturnsAsync([new CardType { Name = "Action" }]);
        cardSubTypeRepo.Setup(cstr => cstr.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, CancellationToken.None))
            .ReturnsAsync([new CardSubType { Name = "Attack" }]);
        fabClassRepo.Setup(fbr => fbr.GetFabClassesByPublicIds(new List<Guid>(), CancellationToken.None))
            .ReturnsAsync([]);
        artistRepo.Setup(ar => ar.GetByPublicId(fryArtist, CancellationToken.None))
            .ReturnsAsync(new Artist { Name = "Edward Chee" });
        setRepo.Setup(sr => sr.GetByPublicId(auroraSetId, CancellationToken.None))
            .ReturnsAsync(new Set { Name = "1st Strike: Aurora", SetCode = "AUR", ReleaseDate = new DateOnly(2024, 8, 1) });

        var commit = new Mock<ICommit>();

        Card? createdCard = null;

        cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

        var service = new CardService(commit.Object, cardRepo.Object, talentRepo.Object, cardTypeRepo.Object, cardSubTypeRepo.Object, fabClassRepo.Object, artistRepo.Object, setRepo.Object);
        //Act
        var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
            () => service.CreateCard(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"Name '{dto.Name}' already exists.");

        cardRepo.Verify(r => r.Exists(dto.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        talentRepo.Verify(r => r.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, It.IsAny<CancellationToken>()), Times.Never);
        cardTypeRepo.Verify(r => r.GetCardTypesByPublicIds(new List<Guid> { actionType }, It.IsAny<CancellationToken>()), Times.Never);
        cardSubTypeRepo.Verify(r => r.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, It.IsAny<CancellationToken>()), Times.Never);
        fabClassRepo.Verify(r => r.GetFabClassesByPublicIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        artistRepo.Verify(r => r.GetByPublicId(fryArtist, It.IsAny<CancellationToken>()), Times.Never);
        setRepo.Verify(r => r.GetByPublicId(auroraSetId, It.IsAny<CancellationToken>()), Times.Never);
        cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never);
        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    [Fact]
    public async Task CreateCard_WhereObjectsDontExist_ThrowsValidationError()
    {
        // Arrange
        var lightningTalent = Guid.NewGuid();
        var actionType = Guid.NewGuid();
        var attackSubType = Guid.NewGuid();
        var auroraSetId = Guid.NewGuid();
        var fryArtist = Guid.NewGuid();
        var fabClass = Guid.NewGuid();
        var dto = new CardWriteDto
        {
            Name = "Fry",
            Talents = [lightningTalent],
            CardTypes = [actionType],
            CardSubTypes = [attackSubType],
            FabClasses = [fabClass],
            CardVariants = [
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 3,
                Pitch = Contracts.PitchEnumAPI.Red,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR008",
                        ArtistId = fryArtist
                        }
                    ]
                },
                new CardVariantWriteDto {
                CardText = "Go again",
                Cost = 0,
                Block = 0,
                Power = 2,
                Pitch = Contracts.PitchEnumAPI.Yellow,
                SetDetails = [
                    new SetDetailWriteDto {
                        Rarity = Contracts.RarityEnumAPI.Common,
                        SetId = auroraSetId,
                        CollectorNumber = "AUR016",
                        ArtistId = fryArtist
                        }
                    ]
                }
            ]
        };

        var cardRepo = new Mock<ICardRepository>();
        var talentRepo = new Mock<ITalentRepository>();
        var cardTypeRepo = new Mock<ICardTypeRepository>();
        var cardSubTypeRepo = new Mock<ICardSubTypeRepository>();
        var fabClassRepo = new Mock<IFabClassRepository>();
        var artistRepo = new Mock<IArtistRepository>();
        var setRepo = new Mock<ISetRepository>();

        cardRepo.Setup(cr => cr.Exists(dto.Name, null, CancellationToken.None))
            .ReturnsAsync(false);
        talentRepo.Setup(tr => tr.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, CancellationToken.None))
            .ReturnsAsync([]);
        cardTypeRepo.Setup(ctr => ctr.GetCardTypesByPublicIds(new List<Guid> { actionType }, CancellationToken.None))
            .ReturnsAsync([]);
        cardSubTypeRepo.Setup(cstr => cstr.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, CancellationToken.None))
            .ReturnsAsync([]);
        fabClassRepo.Setup(fbr => fbr.GetFabClassesByPublicIds(new List<Guid> { fabClass }, CancellationToken.None))
            .ReturnsAsync([]);
        artistRepo.Setup(ar => ar.GetByPublicId(fryArtist, CancellationToken.None))
            .ReturnsAsync((Artist?)null);
        setRepo.Setup(sr => sr.GetByPublicId(auroraSetId, CancellationToken.None))
            .ReturnsAsync((Set?)null);

        var commit = new Mock<ICommit>();

        Card? createdCard = null;

        cardRepo.Setup(r => r.Create(It.IsAny<Card>())).Callback<Card>(card => createdCard = card);

        var service = new CardService(commit.Object, cardRepo.Object, talentRepo.Object, cardTypeRepo.Object, cardSubTypeRepo.Object, fabClassRepo.Object, artistRepo.Object, setRepo.Object);

        // Act
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateCard(dto, CancellationToken.None));
        // Assert
        ex.Message.Should().Be($"One or more validation errors occurred.");

        cardRepo.Verify(r => r.Exists(dto.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        talentRepo.Verify(r => r.GetTalentsByPublicIds(new List<Guid> { lightningTalent }, It.IsAny<CancellationToken>()), Times.Once);
        cardTypeRepo.Verify(r => r.GetCardTypesByPublicIds(new List<Guid> { actionType }, It.IsAny<CancellationToken>()), Times.Once);
        cardSubTypeRepo.Verify(r => r.GetCardSubTypesByPublicIds(new List<Guid> { attackSubType }, It.IsAny<CancellationToken>()), Times.Once);
        fabClassRepo.Verify(r => r.GetFabClassesByPublicIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        artistRepo.Verify(r => r.GetByPublicId(fryArtist, It.IsAny<CancellationToken>()), Times.Exactly(2));
        setRepo.Verify(r => r.GetByPublicId(auroraSetId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        cardRepo.Verify(r => r.Create(It.Is<Card>(a => a.Name == dto.Name)), Times.Never);
        commit.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    // [Fact]
    // public async Task GetCard_ValidPublicId_ReturnsDto()
    // {
    //     // Arrange
    //     var dto = new CardDto(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));

    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();

    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, null, CancellationToken.None))
    //         .ReturnsAsync(new CardUniquenessResult(false, false));

    //     repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
    //     var service = new CardService(commit.Object, repo.Object);

    //     // Act
    //     var result = await service.GetCard(dto.PublicId, CancellationToken.None);
    //     // Assert
    //     result.PublicId.Should().Be(dto.PublicId);
    //     result.Name.Should().Be(dto.Name);
    //     result.CardCode.Should().Be(dto.CardCode);
    //     result.ReleaseDate.Should().Be(dto.ReleaseDate);
    //     repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    // }
    // [Fact]
    // public async Task GetCard_InvalidPublicId_ReturnsNotFoundException()
    // {
    //     // Arrange
    //     var dto = new CardDto(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();

    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, null, CancellationToken.None))
    //         .ReturnsAsync(new CardUniquenessResult(false, false));

    //     repo.Setup(r => r.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((CardDto?)null);
    //     var service = new CardService(commit.Object, repo.Object);

    //     // Act
    //     var ex = await Assert.ThrowsAsync<NotFoundException>(
    //         () => service.GetCard(dto.PublicId, CancellationToken.None));
    //     // Assert
    //     ex.Message.Should().Be($"'{dto.PublicId}' not found.");
    //     repo.Verify(e => e.GetDtoByPublicId(dto.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    // }

    // [Fact]
    // public async Task GetAllCards_ReturnsCards()
    // {
    //     // Arrange
    //     var dtos = new List<CardDto>
    //     {
    //         new(Guid.NewGuid(), "MistVeil", "MST", DateOnly.FromDateTime(DateTime.Now)),
    //         new(Guid.NewGuid(), "Super Slam", "SLM", DateOnly.FromDateTime(DateTime.Now))
    //     };

    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();

    //     repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
    //     var service = new CardService(commit.Object, repo.Object);

    //     // Act
    //     var result = await service.GetAllCards(CancellationToken.None);
    //     // Assert
    //     result.Count.Should().Be(dtos.Count);
    //     repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    // }

    // [Fact]
    // public async Task GetAllCards_NoCards_ReturnsEmptyList()
    // {
    //     // Arrange
    //     var dtos = new List<CardDto>();

    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();

    //     repo.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
    //     var service = new CardService(commit.Object, repo.Object);

    //     // Act
    //     var result = await service.GetAllCards(CancellationToken.None);
    //     // Assert
    //     result.Count.Should().Be(dtos.Count);
    //     repo.Verify(e => e.GetAll(It.IsAny<CancellationToken>()), Times.Once());
    // }

    // [Fact]
    // public async Task DeleteCard_ValidPublicId_Deletes()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.IsUsed(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    //     repo.Setup(r => r.Delete(card));
    //     // Act
    //     await service.DeleteCard(card.PublicId, CancellationToken.None);
    //     // Assert
    //     repo.Verify(e => e.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.IsUsed(card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.Delete(card), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    // }

    // [Fact]
    // public async Task DeleteCard_AlreadyDeleted_ThrowsNotFoundException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);
    //     repo.Setup(r => r.IsUsed(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    //     repo.Setup(r => r.Delete(card));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<NotFoundException>(
    //                 () => service.DeleteCard(card.PublicId, CancellationToken.None));

    //     // Assert
    //     ex.Message.Should().Be($"'{card.PublicId}' not found.");
    //     repo.Verify(e => e.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.IsUsed(card.Id, It.IsAny<CancellationToken>()), Times.Never());
    //     repo.Verify(e => e.Delete(card), Times.Never());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }

    // [Fact]
    // public async Task DeleteCard_Conflict_ReturnsConflictException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.IsUsed(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    //     repo.Setup(r => r.Delete(card));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<ConflictException>(
    //                 () => service.DeleteCard(card.PublicId, CancellationToken.None));

    //     // Assert
    //     ex.Message.Should().Be($"'{card.Name}' is used by a 'Card'.");
    //     repo.Verify(e => e.GetByPublicId(card.PublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.IsUsed(card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.Delete(card), Times.Never());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }
    // [Fact]
    // public async Task UpdateCard_ValidPublicIdAndName_Updates()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = card.PublicId;
    //     var dto = new CardWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(false, false));

    //     // Act
    //     var updatedCard = await service.UpdateCard(searchPublicId, dto, CancellationToken.None);
    //     // Assert
    //     updatedCard.Should().NotBeNull();
    //     updatedCard.Name.Should().Be(dto.Name);

    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    // }

    // [Fact]
    // public async Task UpdateCard_InvalidPublicId_ThrowsNotFoundException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = Guid.Empty;
    //     var dto = new CardWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);
    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(false, false));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<NotFoundException>(
    //                 () => service.UpdateCard(searchPublicId, dto, CancellationToken.None));

    //     // Assert     
    //     ex.Message.Should().Be($"'{searchPublicId}' not found.");
    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>()), Times.Never());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }

    // [Fact]
    // public async Task UpdateCard_InvalidName_ThrowsAlreadyExistsException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = card.PublicId;
    //     var dto = new CardWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(true, false));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
    //                 () => service.UpdateCard(searchPublicId, dto, CancellationToken.None));

    //     // Assert     
    //     ex.Message.Should().Be($"Name '{dto.Name}' already exists.");
    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }

    // [Fact]
    // public async Task UpdateCard_InvalidCard_ThrowsAlreadyExistsException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = card.PublicId;
    //     var dto = new CardWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(false, true));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
    //                 () => service.UpdateCard(searchPublicId, dto, CancellationToken.None));

    //     // Assert     
    //     ex.Message.Should().Be($"CardCode '{dto.CardCode}' already exists.");
    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }

    // [Fact]
    // public async Task UpdateCard_InvalidNameAndCardCode_ThrowsAlreadyExistsException()
    // {
    //     // Arrange
    //     var card = new Card { Name = "Mistveil", CardCode = "MST", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = card.PublicId;
    //     var dto = new CardWriteDto("Mistveil 2", "MST", DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(true, true));
    //     // Act
    //     var ex = await Assert.ThrowsAsync<AlreadyExistsException>(
    //                 () => service.UpdateCard(searchPublicId, dto, CancellationToken.None));

    //     // Assert     
    //     ex.Message.Should().Be($"Name '{dto.Name}' and CardCode '{dto.CardCode}' already exist.");
    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(dto.Name, dto.CardCode, card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    // }

    // [Fact]
    // public async Task UpdateCard_WithSpaces_TrimsCorrectly()
    // {
    //     // Arrange
    //     var name = " Dave   Davington 2 ";
    //     var trueName = "Dave Davington 2";
    //     var cardCode = " MST ";
    //     var trueCardCode = "MST";
    //     var card = new Card { Name = "CardName", CardCode = "SET", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
    //     var searchPublicId = card.PublicId;
    //     var dto = new CardWriteDto(name, cardCode, DateOnly.FromDateTime(DateTime.Now));
    //     var repo = new Mock<ICardRepository>();
    //     var commit = new Mock<ICommit>();
    //     var service = new CardService(commit.Object, repo.Object);

    //     repo.Setup(r => r.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    //     repo.Setup(r => r.CheckUniqueness(trueName, trueCardCode, card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CardUniquenessResult(false, false));
    //     // Act
    //     var updatedCard = await service.UpdateCard(searchPublicId, dto, CancellationToken.None);

    //     // Assert
    //     updatedCard.Should().NotBeNull();
    //     updatedCard!.Name.Should().Be(trueName);
    //     updatedCard!.CardCode.Should().Be(trueCardCode);

    //     repo.Verify(e => e.GetByPublicId(searchPublicId, It.IsAny<CancellationToken>()), Times.Once());
    //     repo.Verify(e => e.CheckUniqueness(trueName, trueCardCode, card.Id, It.IsAny<CancellationToken>()), Times.Once());
    //     commit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    // }
}
