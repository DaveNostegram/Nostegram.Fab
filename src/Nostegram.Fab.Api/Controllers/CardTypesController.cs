using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.CardTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardTypesController(ICardTypeService cardTypeService) : ControllerBase
{
    private readonly ICardTypeService _cardTypeService = cardTypeService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _cardTypeService.GetCardType(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAllCardTypes(CancellationToken ct)
    {
        var cardTypesDTO = await _cardTypeService.GetAllCardTypes(ct);

        return Ok(cardTypesDTO);
    }
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create([FromBody] LookupItemWriteDto dto, CancellationToken ct)
    {
        var createdCardType = await _cardTypeService.CreateCardType(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdCardType.PublicId }, createdCardType);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _cardTypeService.DeleteCardType(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(
        Guid guid,
        [FromBody] LookupItemWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _cardTypeService.UpdateCardType(guid, dto, ct);

        return Ok(updated);
    }
}
