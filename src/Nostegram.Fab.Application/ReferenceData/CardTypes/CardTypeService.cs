using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Normalisations;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.CardTypes;

public class CardTypeService(ICommit commit, ICardTypeRepository cardTypeRepository) : ICardTypeService
{
    public async Task<LookupItemDto> CreateCardType(LookupItemWriteDto dto, CancellationToken ct)
    {
        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(CardType.Name));
        }

        if (await cardTypeRepository.ExistsByName(displayName, ct))
            throw new AlreadyExistsException(displayName);

        var cardType = new CardType { Name = displayName };

        cardTypeRepository.Create(cardType);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardType.PublicId, displayName);
    }
    public async Task<LookupItemDto> GetCardType(Guid publicId, CancellationToken ct)
    {
        var cardType = await cardTypeRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return cardType;
    }
    public async Task<List<LookupItemDto>> GetAllCardTypes(CancellationToken ct)
    {
        return await cardTypeRepository.GetAll(ct);
    }
    public async Task DeleteCardType(Guid publicId, CancellationToken ct)
    {
        var cardType = await cardTypeRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await cardTypeRepository.IsUsed(cardType.Id, ct))
            throw new ConflictException(cardType.Name, "Card");

        cardTypeRepository.Delete(cardType);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateCardType(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var cardType = await cardTypeRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(CardType.Name));
        }

        if (await cardTypeRepository.ExistsByNameExcludingId(cardType.Id, displayName, ct))
            throw new AlreadyExistsException(displayName);

        cardType.Name = displayName;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardType.PublicId, displayName);
    }
}
