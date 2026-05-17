using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;

public interface IArtistService
{
    Task<LookupItemDto> CreateArtist(LookupItemWriteDto createLookupItemDto, CancellationToken ct);
    Task<LookupItemDto> GetArtist(Guid guid, CancellationToken ct);
    Task<List<LookupItemDto>> GetAllArtists(CancellationToken ct);
    Task DeleteArtist(Guid guid, CancellationToken ct);
    Task<LookupItemDto> UpdateArtist(Guid publicId, LookupItemWriteDto dto, CancellationToken ct);
}
