using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Artists;

public class ArtistService(ICommit commit, IArtistRepository artistRepository) : IArtistService
{
    public async Task<LookupItemDto> CreateArtist(LookupItemWriteDto dto, CancellationToken ct)
    {
        if (await artistRepository.ExistsByName(dto.Name, ct))
            throw new AlreadyExistsException(nameof(Artist.Name), dto.Name);

        var artist = new Artist { Name = dto.Name };

        artistRepository.Create(artist);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(artist.PublicId, dto.Name);
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
            throw new ConflictException(artist.Name, "Card");

        artistRepository.Delete(artist);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateArtist(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var artist = await artistRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        if (await artistRepository.ExistsByNameExcludingId(artist.Id, dto.Name, ct))
            throw new AlreadyExistsException(nameof(Artist.Name), dto.Name);

        artist.Name = dto.Name;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(artist.PublicId, dto.Name);
    }
}
