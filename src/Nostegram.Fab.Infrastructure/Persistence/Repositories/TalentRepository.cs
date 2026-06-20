using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class TalentRepository(FabDbContext context) : ITalentRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(Talent entity)
    {
        _context.Talents.Add(entity);
    }
    public async Task<Talent?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Talents.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Talents
            .Where(e => e.PublicId == publicId)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetAll(CancellationToken ct)
    {
        return await _context.Talents
            .OrderBy(e => e.Name)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .ToListAsync(ct);
    }
    public void Delete(Talent entity)
    {
        _context.Talents.Remove(entity);
    }
    public async Task<bool> ExistsByName(string name, CancellationToken ct)
    {
        return await _context.Talents
            .AnyAsync(e => e.Name == name, ct);
    }
    public async Task<bool> ExistsByNameExcludingId(int excludingId, string name, CancellationToken ct)
    {
        return await _context.Talents
            .AnyAsync(e => e.Id != excludingId && e.Name == name, ct);
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.Cards
            .AnyAsync(c => c.Talents.Any(e => e.Id == id), ct);
    }
}
