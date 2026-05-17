using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;

public interface IArtistRepository
{
    void Create(Artist artist);
    Task<Artist?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<LookupItemDto>> GetAll(CancellationToken ct);
    void Delete(Artist artist);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingArtistId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
