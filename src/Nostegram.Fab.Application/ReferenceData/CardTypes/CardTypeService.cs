using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.CardTypes;

public class CardTypeService(ICommit commit, ICardTypeRepository cardTypeRepository) : ICardTypeService
{
    public async Task<LookupItemDto> CreateCardType(LookupItemWriteDto dto, CancellationToken ct)
    {
        if (await cardTypeRepository.ExistsByName(dto.Name, ct))
            throw new AlreadyExistsException(nameof(CardType.Name), dto.Name);

        var cardType = new CardType { Name = dto.Name };

        cardTypeRepository.Create(cardType);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardType.PublicId, dto.Name);
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

        if (await cardTypeRepository.ExistsByNameExcludingId(cardType.Id, dto.Name, ct))
            throw new AlreadyExistsException(nameof(CardType.Name), dto.Name);

        cardType.Name = dto.Name;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardType.PublicId, dto.Name);
    }
}
