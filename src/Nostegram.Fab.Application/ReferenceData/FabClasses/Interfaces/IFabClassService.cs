using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;

public interface IFabClassService
{
    Task<LookupItemDto> CreateFabClass(LookupItemWriteDto createLookupItemDto, CancellationToken ct);
    Task<LookupItemDto> GetFabClass(Guid guid, CancellationToken ct);
    Task<List<LookupItemDto>> GetAllFabClasses(CancellationToken ct);
    Task DeleteFabClass(Guid guid, CancellationToken ct);
    Task<LookupItemDto> UpdateFabClass(Guid publicId, LookupItemWriteDto dto, CancellationToken ct);
}
