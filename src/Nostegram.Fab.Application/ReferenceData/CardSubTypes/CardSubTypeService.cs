using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.CardSubTypes;

public class CardSubTypeService(ICommit commit, ICardSubTypeRepository cardSubTypeRepository) : ICardSubTypeService
{
    public async Task<LookupItemDto> CreateCardSubType(LookupItemWriteDto dto, CancellationToken ct)
    {
        if (await cardSubTypeRepository.ExistsByName(dto.Name, ct))
            throw new AlreadyExistsException(nameof(CardSubType.Name), dto.Name);

        var cardSubType = new CardSubType { Name = dto.Name };

        cardSubTypeRepository.Create(cardSubType);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardSubType.PublicId, dto.Name);
    }
    public async Task<LookupItemDto> GetCardSubType(Guid publicId, CancellationToken ct)
    {
        var cardSubType = await cardSubTypeRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return cardSubType;
    }
    public async Task<List<LookupItemDto>> GetAllCardSubTypes(CancellationToken ct)
    {
        return await cardSubTypeRepository.GetAll(ct);
    }
    public async Task DeleteCardSubType(Guid publicId, CancellationToken ct)
    {
        var cardSubType = await cardSubTypeRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await cardSubTypeRepository.IsUsed(cardSubType.Id, ct))
            throw new ConflictException(cardSubType.Name, "Card");

        cardSubTypeRepository.Delete(cardSubType);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateCardSubType(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var cardSubType = await cardSubTypeRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        if (await cardSubTypeRepository.ExistsByNameExcludingId(cardSubType.Id, dto.Name, ct))
            throw new AlreadyExistsException(nameof(CardSubType.Name), dto.Name);

        cardSubType.Name = dto.Name;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(cardSubType.PublicId, dto.Name);
    }
}
