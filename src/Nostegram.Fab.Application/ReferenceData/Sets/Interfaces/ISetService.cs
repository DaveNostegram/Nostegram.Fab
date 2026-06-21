using Nostegram.Fab.Contracts.Sets;

namespace Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;

public interface ISetService
{
    Task<SetDto> CreateSet(SetWriteDto dto, CancellationToken ct);
    Task<SetDto> GetSet(Guid guid, CancellationToken ct);
    Task<List<SetDto>> GetAllSets(CancellationToken ct);
    Task DeleteSet(Guid guid, CancellationToken ct);
    Task<SetDto> UpdateSet(Guid publicId, SetWriteDto dto, CancellationToken ct);
}
