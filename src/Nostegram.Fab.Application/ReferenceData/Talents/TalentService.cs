using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Talents;

public class TalentService(ICommit commit, ITalentRepository talentRepository) : ITalentService
{
    public async Task<LookupItemDto> CreateTalent(LookupItemWriteDto dto, CancellationToken ct)
    {
        if (await talentRepository.ExistsByName(dto.Name, ct))
            throw new AlreadyExistsException(nameof(Talent.Name), dto.Name);

        var talent = new Talent { Name = dto.Name };

        talentRepository.Create(talent);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(talent.PublicId, dto.Name);
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

        if (await talentRepository.ExistsByNameExcludingId(talent.Id, dto.Name, ct))
            throw new AlreadyExistsException(nameof(Talent.Name), dto.Name);

        talent.Name = dto.Name;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(talent.PublicId, dto.Name);
    }
}
