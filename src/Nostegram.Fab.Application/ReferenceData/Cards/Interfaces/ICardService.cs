using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;

public interface ICardService
{
    Task<CardDto> CreateCard(CardWriteDto dto, CancellationToken ct);
    Task<CardDto> GetCard(Guid guid, CancellationToken ct);
    Task DeleteCard(Guid guid, CancellationToken ct);
    Task<CardDto> UpdateCard(Guid publicId, CardWriteDto dto, CancellationToken ct);
}
