using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class ArtistRepository(FabDbContext context) : IArtistRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(Artist entity)
    {
        _context.Artists.Add(entity);
    }
    public async Task<Artist?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Artists.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Artists
            .Where(e => e.PublicId == publicId)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetAll(CancellationToken ct)
    {
        return await _context.Artists
            .OrderBy(e => e.Name)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .ToListAsync(ct);
    }
    public void Delete(Artist entity)
    {
        _context.Artists.Remove(entity);
    }
    public async Task<bool> ExistsByName(string name, CancellationToken ct)
    {
        return await _context.Artists
            .AnyAsync(e => e.Name == name, ct);
    }
    public async Task<bool> ExistsByNameExcludingId(int excludingId, string name, CancellationToken ct)
    {
        return await _context.Artists
            .AnyAsync(e => e.Id != excludingId && e.Name == name, ct);
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.SetDetails
            .AnyAsync(e => e.ArtistId == id, ct);
    }
}
