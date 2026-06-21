using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Normalisations;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Artists;

public class ArtistService(ICommit commit, IArtistRepository artistRepository) : IArtistService
{
    public async Task<LookupItemDto> CreateArtist(LookupItemWriteDto dto, CancellationToken ct)
    {
        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(Artist.Name));
        }

        if (await artistRepository.ExistsByName(displayName, ct))
            throw new AlreadyExistsException(nameof(Artist.Name), displayName);

        var artist = new Artist { Name = displayName };

        artistRepository.Create(artist);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(artist.PublicId, displayName);
    }
    public async Task<LookupItemDto> GetArtist(Guid publicId, CancellationToken ct)
    {
        var artist = await artistRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return artist;
    }
    public async Task<List<LookupItemDto>> GetAllArtists(CancellationToken ct)
    {
        return await artistRepository.GetAll(ct);
    }
    public async Task DeleteArtist(Guid publicId, CancellationToken ct)
    {
        var artist = await artistRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await artistRepository.IsUsed(artist.Id, ct))
            throw new ConflictException(artist.Name, "SetDetail");

        artistRepository.Delete(artist);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateArtist(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var artist = await artistRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(Artist.Name));
        }

        if (await artistRepository.ExistsByNameExcludingId(artist.Id, displayName, ct))
            throw new AlreadyExistsException(nameof(Artist.Name), displayName);

        artist.Name = displayName;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(artist.PublicId, displayName);
    }
}
