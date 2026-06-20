using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;

public interface ITalentService
{
    Task<LookupItemDto> CreateTalent(LookupItemWriteDto createLookupItemDto, CancellationToken ct);
    Task<LookupItemDto> GetTalent(Guid guid, CancellationToken ct);
    Task<List<LookupItemDto>> GetAllTalents(CancellationToken ct);
    Task DeleteTalent(Guid guid, CancellationToken ct);
    Task<LookupItemDto> UpdateTalent(Guid publicId, LookupItemWriteDto dto, CancellationToken ct);
}
