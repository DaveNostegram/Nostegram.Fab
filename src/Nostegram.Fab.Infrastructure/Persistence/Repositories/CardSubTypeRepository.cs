using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class CardSubTypeRepository(FabDbContext context) : ICardSubTypeRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(CardSubType entity)
    {
        _context.CardSubTypes.Add(entity);
    }
    public async Task<CardSubType?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.CardSubTypes.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.CardSubTypes
            .Where(e => e.PublicId == publicId)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetAll(CancellationToken ct)
    {
        return await _context.CardSubTypes
            .OrderBy(e => e.Name)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .ToListAsync(ct);
    }
    public void Delete(CardSubType entity)
    {
        _context.CardSubTypes.Remove(entity);
    }
    public async Task<bool> ExistsByName(string name, CancellationToken ct)
    {
        return await _context.CardSubTypes
            .AnyAsync(e => e.Name == name, ct);
    }
    public async Task<bool> ExistsByNameExcludingId(int excludingId, string name, CancellationToken ct)
    {
        return await _context.CardSubTypes
            .AnyAsync(e => e.Id != excludingId && e.Name == name, ct);
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.Cards
            .AnyAsync(c => c.CardSubTypes.Any(e => e.Id == id), ct);
    }
}
