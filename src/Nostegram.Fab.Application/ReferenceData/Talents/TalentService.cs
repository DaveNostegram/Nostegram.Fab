using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Normalisations;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Talents;

public class TalentService(ICommit commit, ITalentRepository talentRepository) : ITalentService
{
    public async Task<LookupItemDto> CreateTalent(LookupItemWriteDto dto, CancellationToken ct)
    {
        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(Talent.Name));
        }

        if (await talentRepository.ExistsByName(displayName, ct))
            throw new AlreadyExistsException(displayName);

        var talent = new Talent { Name = displayName };

        talentRepository.Create(talent);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(talent.PublicId, displayName);
    }
    public async Task<LookupItemDto> GetTalent(Guid publicId, CancellationToken ct)
    {
        var talent = await talentRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return talent;
    }
    public async Task<List<LookupItemDto>> GetAllTalents(CancellationToken ct)
    {
        return await talentRepository.GetAll(ct);
    }
    public async Task DeleteTalent(Guid publicId, CancellationToken ct)
    {
        var talent = await talentRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await talentRepository.IsUsed(talent.Id, ct))
            throw new ConflictException(talent.Name, "Card");

        talentRepository.Delete(talent);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateTalent(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var talent = await talentRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(Talent.Name));
        }

        if (await talentRepository.ExistsByNameExcludingId(talent.Id, displayName, ct))
            throw new AlreadyExistsException(displayName);

        talent.Name = displayName;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(talent.PublicId, displayName);
    }
}
