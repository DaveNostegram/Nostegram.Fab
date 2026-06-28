using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.CardSubTypes.Interfaces;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardSubTypesController(ICardSubTypeService cardSubTypeService) : ControllerBase
{
    private readonly ICardSubTypeService _cardSubTypeService = cardSubTypeService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _cardSubTypeService.GetCardSubType(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAllCardSubTypes(CancellationToken ct)
    {
        var cardSubTypesDTO = await _cardSubTypeService.GetAllCardSubTypes(ct);

        return Ok(cardSubTypesDTO);
    }
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create([FromBody] LookupItemWriteDto dto, CancellationToken ct)
    {
        var createdCardSubType = await _cardSubTypeService.CreateCardSubType(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdCardSubType.PublicId }, createdCardSubType);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _cardSubTypeService.DeleteCardSubType(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(
        Guid guid,
        [FromBody] LookupItemWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _cardSubTypeService.UpdateCardSubType(guid, dto, ct);

        return Ok(updated);
    }
}
