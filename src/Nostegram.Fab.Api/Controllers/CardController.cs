using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.Cards.Interfaces;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController(ICardService cardService) : ControllerBase
{
    private readonly ICardService _cardService = cardService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<CardDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _cardService.GetCard(guid, ct);

        return Ok(dt);
    }

    [HttpPost]
    public async Task<ActionResult<CardDto>> Create([FromBody] CardWriteDto dto, CancellationToken ct)
    {
        var createdCard = await _cardService.CreateCard(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdCard.PublicId }, createdCard);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _cardService.DeleteCard(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<CardDto>> Update(
        Guid guid,
        [FromBody] CardWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _cardService.UpdateCard(guid, dto, ct);

        return Ok(updated);
    }
}
