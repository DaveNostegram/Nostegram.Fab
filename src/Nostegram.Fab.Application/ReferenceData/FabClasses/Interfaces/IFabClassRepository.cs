using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;

public interface IFabClassRepository
{
    void Create(FabClass fabClass);
    Task<FabClass?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<LookupItemDto>> GetAll(CancellationToken ct);
    void Delete(FabClass fabClass);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingFabClassId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
