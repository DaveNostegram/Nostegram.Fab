using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.Sets.Interfaces;
using Nostegram.Fab.Contracts.Sets;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetsController(ISetService setService) : ControllerBase
{
    private readonly ISetService _setService = setService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<SetDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _setService.GetSet(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<SetDto>>> GetAllSets(CancellationToken ct)
    {
        var setsDTO = await _setService.GetAllSets(ct);

        return Ok(setsDTO);
    }
    [HttpPost]
    public async Task<ActionResult<SetDto>> Create([FromBody] SetWriteDto dto, CancellationToken ct)
    {
        var createdSet = await _setService.CreateSet(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdSet.PublicId }, createdSet);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _setService.DeleteSet(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<SetDto>> Update(
        Guid guid,
        [FromBody] SetWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _setService.UpdateSet(guid, dto, ct);

        return Ok(updated);
    }
}
