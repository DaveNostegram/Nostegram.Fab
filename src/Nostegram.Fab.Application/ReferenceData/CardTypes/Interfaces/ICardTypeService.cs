using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;

public interface ICardTypeService
{
    Task<LookupItemDto> CreateCardType(LookupItemWriteDto createLookupItemDto, CancellationToken ct);
    Task<LookupItemDto> GetCardType(Guid guid, CancellationToken ct);
    Task<List<LookupItemDto>> GetAllCardTypes(CancellationToken ct);
    Task DeleteCardType(Guid guid, CancellationToken ct);
    Task<LookupItemDto> UpdateCardType(Guid publicId, LookupItemWriteDto dto, CancellationToken ct);
}
