using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Common.Normalisations;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.FabClasses;

public class FabClassService(ICommit commit, IFabClassRepository fabClassRepository) : IFabClassService
{
    public async Task<LookupItemDto> CreateFabClass(LookupItemWriteDto dto, CancellationToken ct)
    {
        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(FabClass.Name));
        }

        if (await fabClassRepository.ExistsByName(displayName, ct))
            throw new AlreadyExistsException(displayName);

        var fabClass = new FabClass { Name = displayName };

        fabClassRepository.Create(fabClass);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(fabClass.PublicId, displayName);
    }
    public async Task<LookupItemDto> GetFabClass(Guid publicId, CancellationToken ct)
    {
        var fabClass = await fabClassRepository.GetDtoByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        return fabClass;
    }
    public async Task<List<LookupItemDto>> GetAllFabClasses(CancellationToken ct)
    {
        return await fabClassRepository.GetAll(ct);
    }
    public async Task DeleteFabClass(Guid publicId, CancellationToken ct)
    {
        var fabClass = await fabClassRepository.GetByPublicId(publicId, ct)
            ?? throw new NotFoundException($"{publicId}");

        if (await fabClassRepository.IsUsed(fabClass.Id, ct))
            throw new ConflictException(fabClass.Name, "Card");

        fabClassRepository.Delete(fabClass);

        await commit.SaveChangesAsync(ct);
    }
    public async Task<LookupItemDto> UpdateFabClass(Guid publicId, LookupItemWriteDto dto, CancellationToken ct)
    {
        var fabClass = await fabClassRepository.GetByPublicId(publicId, ct)
           ?? throw new NotFoundException($"{publicId}");

        var displayName = NameNormaliser.ForDisplay(dto.Name);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new RequiredFieldException(nameof(FabClass.Name));
        }

        if (await fabClassRepository.ExistsByNameExcludingId(fabClass.Id, displayName, ct))
            throw new AlreadyExistsException(displayName);

        fabClass.Name = displayName;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(fabClass.PublicId, displayName);
    }
}
