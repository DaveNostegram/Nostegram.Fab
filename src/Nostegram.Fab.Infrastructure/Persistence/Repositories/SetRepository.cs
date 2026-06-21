using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Application.ReferenceData.Sets.Results;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class SetRepository(FabDbContext context) : ISetRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(Set entity)
    {
        _context.Sets.Add(entity);
    }
    public async Task<Set?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Sets.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<SetDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.Sets
            .Where(e => e.PublicId == publicId)
            .Select(e => new SetDto(e.PublicId, e.Name, e.SetCode, e.ReleaseDate))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<SetDto>> GetAll(CancellationToken ct)
    {
        return await _context.Sets
            .OrderBy(e => e.Name)
            .Select(e => new SetDto(e.PublicId, e.Name, e.SetCode, e.ReleaseDate))
            .ToListAsync(ct);
    }
    public void Delete(Set entity)
    {
        _context.Sets.Remove(entity);
    }
    public async Task<SetUniquenessResult> CheckUniqueness(string name, string setCode, int? excludingId, CancellationToken ct)
    {
        var matches = await _context.Sets
            .Where(x =>
                (!excludingId.HasValue || x.Id != excludingId.Value) &&
                (x.Name == name || x.SetCode == setCode))
            .Select(x => new
            {
                x.Name,
                x.SetCode
            })
            .ToListAsync(ct);

        return new SetUniquenessResult(
            NameExists: matches.Any(x => x.Name == name),
            SetCodeExists: matches.Any(x => x.SetCode == setCode));
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.SetDetails
            .AnyAsync(e => e.SetId == id, ct);
    }
}
