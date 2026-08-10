using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Application.Exceptions;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.FabClasses;

public class FabClassService(ICommit commit, IFabClassRepository fabClassRepository) : IFabClassService
{
    public async Task<LookupItemDto> CreateFabClass(LookupItemWriteDto dto, CancellationToken ct)
    {
        if (await fabClassRepository.ExistsByName(dto.Name, ct))
            throw new AlreadyExistsException(nameof(FabClass.Name), dto.Name);

        var fabClass = new FabClass { Name = dto.Name };

        fabClassRepository.Create(fabClass);
        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(fabClass.PublicId, dto.Name);
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

        if (await fabClassRepository.ExistsByNameExcludingId(fabClass.Id, dto.Name, ct))
            throw new AlreadyExistsException(nameof(FabClass.Name), dto.Name);

        fabClass.Name = dto.Name;

        await commit.SaveChangesAsync(ct);
        return new LookupItemDto(fabClass.PublicId, dto.Name);
    }
}
