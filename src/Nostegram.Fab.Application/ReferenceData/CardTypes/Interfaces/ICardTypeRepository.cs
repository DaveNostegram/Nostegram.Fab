using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;

public interface ICardTypeRepository
{
    void Create(CardType cardType);
    Task<CardType?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<LookupItemDto>> GetAll(CancellationToken ct);
    void Delete(CardType cardType);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingCardTypeId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
