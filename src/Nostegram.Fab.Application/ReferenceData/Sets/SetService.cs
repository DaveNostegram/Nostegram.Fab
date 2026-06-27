using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Normalisations;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Sets;

public class SetService(ICommit commit, ISetRepository setRepository) : ISetService
{
    public async Task<SetDto> CreateSet(SetWriteDto dto, CancellationToken ct)
    {
        var displayName = NameNormaliser.ForDisplay(dto.Name);
        var displaySetCode = NameNormaliser.ForDisplay(dto.SetCode);

        var missingRequired = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(displayName))
            missingRequired.Add(nameof(Set.Name));

        if (string.IsNullOrWhiteSpace(displaySetCode))
            missingRequired.Add(nameof(Set.SetCode));

        if (missingRequired.Count > 0)
            throw new RequiredFieldException(missingRequired);

        var uniqueness = await setRepository.CheckUniqueness(displayName, displaySetCode, null, ct);

        var conflicts = new Dictionary<string, string>();

        if (uniqueness.NameExists)
            conflicts.Add(nameof(Set.Name), displayName);

        if (uniqueness.SetCodeExists)
            conflicts.Add(nameof(Set.SetCode), displaySetCode);

        if (conflicts.Count > 0)
            throw new AlreadyExistsException(conflicts);

        var set = new Set { Name = displayName, SetCode = displaySetCode, ReleaseDate = dto.ReleaseDate };

        setRepository.Create(set);
        await commit.SaveChangesAsync(ct);
        return new SetDto(set.PublicId, displayName, displaySetCode, dto.ReleaseDate);
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

        var displayName = NameNormaliser.ForDisplay(dto.Name);
        var displaySetCode = NameNormaliser.ForDisplay(dto.SetCode);
        var missingRequired = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(displayName))
            missingRequired.Add(nameof(Set.Name));

        if (string.IsNullOrWhiteSpace(displaySetCode))
            missingRequired.Add(nameof(Set.SetCode));

        if (missingRequired.Count > 0)
            throw new RequiredFieldException(missingRequired);

        var uniqueness = await setRepository.CheckUniqueness(displayName, displaySetCode, set.Id, ct);

        var conflicts = new Dictionary<string, string>();

        if (uniqueness.NameExists)
            conflicts.Add(nameof(Set.Name), displayName);

        if (uniqueness.SetCodeExists)
            conflicts.Add(nameof(Set.SetCode), displaySetCode);

        if (conflicts.Count > 0)
            throw new AlreadyExistsException(conflicts);

        set.Name = displayName;
        set.SetCode = displaySetCode;
        set.ReleaseDate = dto.ReleaseDate;

        await commit.SaveChangesAsync(ct);
        return new SetDto(set.PublicId, displayName, displaySetCode, set.ReleaseDate);
    }
}
