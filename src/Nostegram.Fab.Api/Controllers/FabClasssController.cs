using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.FabClasses.Interfaces;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FabClasssController(IFabClassService fabClassService) : ControllerBase
{
    private readonly IFabClassService _fabClassService = fabClassService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _fabClassService.GetFabClass(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAllFabClasss(CancellationToken ct)
    {
        var fabClasssDTO = await _fabClassService.GetAllFabClasses(ct);

        return Ok(fabClasssDTO);
    }
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create([FromBody] LookupItemWriteDto dto, CancellationToken ct)
    {
        var createdFabClass = await _fabClassService.CreateFabClass(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdFabClass.PublicId }, createdFabClass);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _fabClassService.DeleteFabClass(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(
        Guid guid,
        [FromBody] LookupItemWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _fabClassService.UpdateFabClass(guid, dto, ct);

        return Ok(updated);
    }
}
