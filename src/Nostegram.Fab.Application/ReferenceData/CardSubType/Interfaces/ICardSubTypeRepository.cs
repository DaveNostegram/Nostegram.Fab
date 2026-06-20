using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;

public interface ICardSubTypeRepository
{
    void Create(CardSubType cardSubType);
    Task<CardSubType?> GetByPublicId(Guid publicId, CancellationToken ct);
    Task<LookupItemDto?> GetDtoByPublicId(Guid publicId, CancellationToken ct);
    Task<List<LookupItemDto>> GetAll(CancellationToken ct);
    void Delete(CardSubType cardSubType);
    Task<bool> ExistsByName(string name, CancellationToken ct);
    Task<bool> ExistsByNameExcludingId(int excludingCardSubTypeId, string name, CancellationToken ct);
    Task<bool> IsUsed(int Id, CancellationToken ct);
}
