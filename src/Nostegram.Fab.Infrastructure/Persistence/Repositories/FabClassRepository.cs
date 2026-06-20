using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class FabClassRepository(FabDbContext context) : IFabClassRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(FabClass entity)
    {
        _context.FabClasses.Add(entity);
    }
    public async Task<FabClass?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.FabClasses.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.FabClasses
            .Where(e => e.PublicId == publicId)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetAll(CancellationToken ct)
    {
        return await _context.FabClasses
            .OrderBy(e => e.Name)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .ToListAsync(ct);
    }
    public void Delete(FabClass entity)
    {
        _context.FabClasses.Remove(entity);
    }
    public async Task<bool> ExistsByName(string name, CancellationToken ct)
    {
        return await _context.FabClasses
            .AnyAsync(e => e.Name == name, ct);
    }
    public async Task<bool> ExistsByNameExcludingId(int excludingId, string name, CancellationToken ct)
    {
        return await _context.FabClasses
            .AnyAsync(e => e.Id != excludingId && e.Name == name, ct);
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.Cards
            .AnyAsync(c => c.FabClasses.Any(e => e.Id == id), ct);
    }
}
