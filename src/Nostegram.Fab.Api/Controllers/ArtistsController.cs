using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.ReferenceData.Artists.Interfaces;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController(IArtistService artistService) : ControllerBase
{
    private readonly IArtistService _artistService = artistService;

    [HttpGet("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Get(Guid guid, CancellationToken ct)
    {
        var dt = await _artistService.GetArtist(guid, ct);

        return Ok(dt);
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAllArtists(CancellationToken ct)
    {
        var artistsDTO = await _artistService.GetAllArtists(ct);

        return Ok(artistsDTO);
    }
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create([FromBody] LookupItemWriteDto dto, CancellationToken ct)
    {
        var createdArtist = await _artistService.CreateArtist(dto, ct);

        return CreatedAtAction(nameof(Get), new { guid = createdArtist.PublicId }, createdArtist);
    }
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> Delete(Guid guid, CancellationToken ct)
    {
        await _artistService.DeleteArtist(guid, ct);

        return NoContent();
    }

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(
        Guid guid,
        [FromBody] LookupItemWriteDto dto,
        CancellationToken ct)
    {
        var updated = await _artistService.UpdateArtist(guid, dto, ct);

        return Ok(updated);
    }
}
