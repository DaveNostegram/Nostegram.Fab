using Microsoft.VisualBasic;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Cards;

public class CardService(ICommit commit, ICardRepository cardRepository
, ITalentRepository talentRepository,
ICardTypeRepository cardTypeRepository,
ICardSubTypeRepository cardSubTypeRepository,
IFabClassRepository fabClassRepository,
IArtistRepository artistRepository,
ISetRepository setRepository
) : ICardService
{
    public async Task<CardDto> CreateCard(CardWriteDto dto, CancellationToken ct)
    {
        await CheckDoesNotExist(dto, null, ct);
        var validationErrors = new Dictionary<string, string[]>();
        var subTypesTask = ValidateAndReturnCardSubTypes(dto, validationErrors, ct);
        var typesTask = ValidateAndReturnCardTypes(dto, validationErrors, ct);
        var fabClassesTask = ValidateAndReturnFabClasses(dto, validationErrors, ct);
        var talentsTask = ValidateAndReturnTalents(dto, validationErrors, ct);
        var flipCardTask = ValidateAndReturnFlipCard(dto, validationErrors, ct);
        var cardVariantTask = ValidateAndReturnCardVariants(dto, validationErrors, ct);

        await Task.WhenAll(
            subTypesTask,
            typesTask,
            fabClassesTask,
            talentsTask,
            flipCardTask,
            cardVariantTask
        );

        if (validationErrors.Count != 0)
            throw new ValidationException(validationErrors);

        var subTypes = await subTypesTask;
        var types = await typesTask;
        var fabClasses = await fabClassesTask;
        var talents = await talentsTask;
        var flipCard = await flipCardTask;
        var cardVariants = await cardVariantTask;

        var card = new Card
        {
            Name = dto.Name,
            CardSubTypes = subTypes,
            CardTypes = types,
            FabClasses = fabClasses,
            Talents = talents,
            CardVariants = cardVariants,
            FlipCardId = flipCard?.Id,
            FlipCard = flipCard
        };

        cardRepository.Create(card);
        await commit.SaveChangesAsync(ct);
        return card.ToDto();
    }
    public async Task<CardDto> GetCard(Guid publicId, CancellationToken ct)
    {
        throw new NotImplementedException();
        // var card = await cardRepository.GetDtoByPublicId(publicId, ct)
        //     ?? throw new NotFoundException($"{publicId}");

        // return card;
    }
    public async Task<List<CardDto>> GetAllCards(CancellationToken ct)
    {
        throw new NotImplementedException();
        //return await cardRepository.GetAll(ct);
    }
    public async Task DeleteCard(Guid publicId, CancellationToken ct)
    {
        throw new NotImplementedException();
        // var card = await cardRepository.GetByPublicId(publicId, ct)
        //     ?? throw new NotFoundException($"{publicId}");

        // if (await cardRepository.IsUsed(card.Id, ct))
        //     throw new ConflictException(card.Name, "Card");

        // cardRepository.Delete(card);

        // await commit.SaveChangesAsync(ct);
    }
    public async Task<CardDto> UpdateCard(Guid publicId, CardWriteDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
        // var card = await cardRepository.GetByPublicId(publicId, ct)
        //    ?? throw new NotFoundException($"{publicId}");

        // await CheckUniqueness(dto, card.Id, ct);

        // card.Name = dto.Name;
        // card.CardCode = dto.CardCode;
        // card.ReleaseDate = dto.ReleaseDate;

        // await commit.SaveChangesAsync(ct);
        // return new CardDto(card.PublicId, dto.Name, dto.CardCode, card.ReleaseDate);
    }

    private async Task CheckDoesNotExist(CardWriteDto dto, int? cardId, CancellationToken ct)
    {
        var exists = await cardRepository.Exists(dto.Name, cardId, ct);
        var conflicts = new Dictionary<string, string>();

        if (exists)
            conflicts.Add(nameof(Card.Name), dto.Name);

        if (conflicts.Count > 0)
            throw new AlreadyExistsException(conflicts);
    }
    private async Task<List<Talent>> ValidateAndReturnTalents(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        if (dto.Talents.Count != 0)
        {
            var talentsExists = await talentRepository.GetTalentsByPublicIds(dto.Talents, ct);

            if (dto.Talents.Count > talentsExists.Count)
            {
                var missingCount = dto.Talents.Count - talentsExists.Count;
                validationErrors["Talents"] = [$"{missingCount} referenced Talent(s) do not exist."];
            }
            return talentsExists;
        }
        return [];
    }
    private async Task<List<CardType>> ValidateAndReturnCardTypes(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        if (dto.CardTypes.Count != 0)
        {
            var cardTypesExists = await cardTypeRepository.GetCardTypesByPublicIds(dto.CardTypes, ct);

            if (dto.CardTypes.Count > cardTypesExists.Count)
            {
                var missingCount = dto.CardTypes.Count - cardTypesExists.Count;
                validationErrors["CardTypes"] = [$"{missingCount} referenced CardTypes(s) do not exist."];
            }
            return cardTypesExists;
        }
        return [];
    }
    private async Task<List<CardSubType>> ValidateAndReturnCardSubTypes(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        if (dto.CardSubTypes.Count != 0)
        {
            var cardSubTypesExists = await cardSubTypeRepository.GetCardSubTypesByPublicIds(dto.CardSubTypes, ct);

            if (dto.CardSubTypes.Count > cardSubTypesExists.Count)
            {
                var missingCount = dto.CardSubTypes.Count - cardSubTypesExists.Count;
                validationErrors["CardSubTypes"] = [$"{missingCount} referenced CardSubType(s) do not exist."];
            }
            return cardSubTypesExists;
        }
        return [];
    }
    private async Task<List<FabClass>> ValidateAndReturnFabClasses(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        if (dto.FabClasses.Count != 0)
        {
            var fabClassesExists = await fabClassRepository.GetFabClassesByPublicIds(dto.FabClasses, ct);

            if (dto.FabClasses.Count > fabClassesExists.Count)
            {
                var missingCount = dto.FabClasses.Count - fabClassesExists.Count;
                validationErrors["CardTypes"] = [$"{missingCount} referenced FabClass(s) do not exist."];
            }
            return fabClassesExists;
        }
        return [];
    }
    private async Task<Card?> ValidateAndReturnFlipCard(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        var flipCardGuid = dto.FlipCard ?? Guid.Empty;
        if (flipCardGuid != Guid.Empty)
        {
            var flipCard = await cardRepository.GetByPublicId(flipCardGuid, ct);

            if (flipCard == null)
            {
                validationErrors["FlipCard"] = [$"Referenced FlipCard does not exist."];
                return null;
            }
            else
            {
                return flipCard;
            }

        }
        return null;
    }

    private async Task<List<CardVariant>> ValidateAndReturnCardVariants(CardWriteDto dto, Dictionary<string, string[]> validationErrors, CancellationToken ct)
    {
        List<CardVariant> cardVariants = [];
        bool isErrorState = false;
        foreach (var cardVariantDto in dto.CardVariants)
        {
            CardVariant cardVariant = new()
            {
                CardText = cardVariantDto.CardText,
                Cost = cardVariantDto.Cost,
                Block = cardVariantDto.Block,
                Power = cardVariantDto.Power,
                Health = cardVariantDto.Health,
                Intellect = cardVariantDto.Intellect,
                Pitch = cardVariantDto.Pitch.ToDomain()
            };
            foreach (var setDetailDto in cardVariantDto.SetDetails)
            {
                var artist = await artistRepository.GetByPublicId(setDetailDto.ArtistId, ct);
                var set = await setRepository.GetByPublicId(setDetailDto.SetId, ct);

                if (artist == null)
                {
                    validationErrors["CardVariant"] = [$"Referenced Artist inside card variant pitch '{cardVariantDto.Pitch}' does not exist."];
                }
                if (set == null)
                {
                    validationErrors["CardVariant"] = [$"Referenced Set inside card variant pitch '{cardVariantDto.Pitch}' does not exist."];
                }
                if (set == null || artist == null)
                {
                    isErrorState = true;
                }
                if (!isErrorState && set != null && artist != null)
                {
                    SetDetail setDetail = new()
                    {
                        Rarity = setDetailDto.Rarity.ToDomain(),
                        SetId = set.Id,
                        Set = set,
                        ArtistId = artist.Id,
                        Artist = artist,
                        CollectorNumber = setDetailDto.CollectorNumber
                    };
                    cardVariant.SetDetails.Add(setDetail);
                }
            }
            if (!isErrorState)
            {
                cardVariants.Add(cardVariant);
            }
        }
        if (!isErrorState)
        {
            return cardVariants;
        }
        else
        {
            return [];
        }
    }
}
