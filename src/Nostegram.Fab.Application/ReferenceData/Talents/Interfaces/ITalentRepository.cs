using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;

public interface ITalentRepository
{
    void Create(Talent talent);
    Task<Talent?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<LookupItemDto>> GetAll(CancellationToken ct);
    void Delete(Talent talent);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingTalentId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
