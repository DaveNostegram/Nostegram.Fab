using Nostegram.Fab.Application.ReferenceData.Sets.Results;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;

public interface ISetRepository
{
    void Create(Set set);
    Task<Set?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<SetDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<SetDto>> GetAll(CancellationToken ct);
    void Delete(Set set);
    Task<SetUniquenessResult> CheckUniqueness(string name, string setCode, int? excludingId, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
