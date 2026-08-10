using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;

public interface ICardRepository
{
    void Create(Card card);
    Task<Card?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<CardDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<CardDto?>> GetAll(CancellationToken ct);
    void Delete(Card card);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingCardId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
