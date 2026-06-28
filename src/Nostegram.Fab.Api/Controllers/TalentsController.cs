using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.Talents.Interfaces;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TalentsController(ITalentService talentService) : ControllerBase
{
    private readonly ITalentService _talentService = talentService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _talentService.GetTalent(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAllTalents(CancellationToken ct)
    {
        var talentsDTO = await _talentService.GetAllTalents(ct);

        return Ok(talentsDTO);
    }
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create([FromBody] LookupItemWriteDto dto, CancellationToken ct)
    {
        var createdTalent = await _talentService.CreateTalent(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdTalent.PublicId }, createdTalent);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _talentService.DeleteTalent(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(
        Guid guid,
        [FromBody] LookupItemWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _talentService.UpdateTalent(guid, dto, ct);

        return Ok(updated);
    }
}
