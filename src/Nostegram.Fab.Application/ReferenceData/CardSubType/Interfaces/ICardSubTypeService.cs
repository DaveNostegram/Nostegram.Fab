using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;

public interface ICardSubTypeService
{
    Task<LookupItemDto> CreateCardSubType(LookupItemWriteDto createLookupItemDto, CancellationToken ct);
    Task<LookupItemDto> GetCardSubType(Guid guid, CancellationToken ct);
    Task<List<LookupItemDto>> GetAllCardSubTypes(CancellationToken ct);
    Task DeleteCardSubType(Guid guid, CancellationToken ct);
    Task<LookupItemDto> UpdateCardSubType(Guid publicId, LookupItemWriteDto dto, CancellationToken ct);
}
