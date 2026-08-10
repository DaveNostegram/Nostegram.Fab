using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Sets;

public class SetService(ICommit commit, ISetRepository setRepository) : ISetService
{
    public async Task<SetDto> CreateSet(SetWriteDto dto, CancellationToken ct)
    {
        await CheckUniqueness(dto, null, ct);

        var set = new Set { Name = dto.Name, SetCode = dto.SetCode, ReleaseDate = dto.ReleaseDate };

        setRepository.Create(set);
        await commit.SaveChangesAsync(ct);
        return new SetDto(set.PublicId, dto.Name, dto.SetCode, dto.ReleaseDate);
    }
    public async Task<SetDto> GetSet(Guid publicId, CancellationToken ct)
    {
        var set = await setRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return set;
    }
    public async Task<List<SetDto>> GetAllSets(CancellationToken ct)
    {
        return await setRepository.GetAll(ct);
    }
    public async Task DeleteSet(Guid publicId, CancellationToken ct)
    {
        var set = await setRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await setRepository.IsUsed(set.Id, ct))
            throw new ConflictException(set.Name, "Card");

        setRepository.Delete(set);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<SetDto> UpdateSet(Guid publicId, SetWriteDto dto, CancellationToken ct)
    {
        var set = await setRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        await CheckUniqueness(dto, set.Id, ct);

        set.Name = dto.Name;
        set.SetCode = dto.SetCode;
        set.ReleaseDate = dto.ReleaseDate;

        await commit.SaveChangesAsync(ct);
        return new SetDto(set.PublicId, dto.Name, dto.SetCode, set.ReleaseDate);
    }

    private async Task CheckUniqueness(SetWriteDto dto, int? setId, CancellationToken ct)
    {
        var uniqueness = await setRepository.CheckUniqueness(dto.Name, dto.SetCode, setId, ct);

        var conflicts = new Dictionary<string, string>();

        if (uniqueness.NameExists)
            conflicts.Add(nameof(Set.Name), dto.Name);

        if (uniqueness.SetCodeExists)
            conflicts.Add(nameof(Set.SetCode), dto.SetCode);

        if (conflicts.Count > 0)
            throw new AlreadyExistsException(conflicts);
    }
}
