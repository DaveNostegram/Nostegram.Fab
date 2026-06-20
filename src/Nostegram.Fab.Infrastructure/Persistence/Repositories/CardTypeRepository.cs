using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Repositories;

public class CardTypeRepository(FabDbContext context) : ICardTypeRepository
{
    private readonly FabDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Create(CardType entity)
    {
        _context.CardTypes.Add(entity);
    }
    public async Task<CardType?> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.CardTypes.Where(e => e.PublicId == publicId).SingleOrDefaultAsync(ct);
    }
    public async Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct)
    {
        return await _context.CardTypes
            .Where(e => e.PublicId == publicId)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetAll(CancellationToken ct)
    {
        return await _context.CardTypes
            .OrderBy(e => e.Name)
            .Select(e => new LookupItemDto(e.PublicId, e.Name))
            .ToListAsync(ct);
    }
    public void Delete(CardType entity)
    {
        _context.CardTypes.Remove(entity);
    }
    public async Task<bool> ExistsByName(string name, CancellationToken ct)
    {
        return await _context.CardTypes
            .AnyAsync(e => e.Name == name, ct);
    }
    public async Task<bool> ExistsByNameExcludingId(int excludingId, string name, CancellationToken ct)
    {
        return await _context.CardTypes
            .AnyAsync(e => e.Id != excludingId && e.Name == name, ct);
    }
    public async Task<bool> IsUsed(int id, CancellationToken ct)
    {
        return await _context.Cards
            .AnyAsync(c => c.CardTypes.Any(e => e.Id == id), ct);
    }
}
