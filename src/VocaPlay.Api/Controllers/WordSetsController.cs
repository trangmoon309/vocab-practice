// VocaPlay.Api/Controllers/WordSetsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.WordSets.Commands;
using VocaPlay.Application.WordSets.DTOs;
using VocaPlay.Application.WordSets.Queries;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wordsets")]
public class WordSetsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetWordSetsQueryHandler _getAll;
    private readonly GetWordSetByIdQueryHandler _getById;
    private readonly CreateWordSetCommandHandler _create;
    private readonly UpdateWordSetCommandHandler _update;
    private readonly DeleteWordSetCommandHandler _delete;

    public WordSetsController(
        ICurrentUserService currentUser,
        GetWordSetsQueryHandler getAll,
        GetWordSetByIdQueryHandler getById,
        CreateWordSetCommandHandler create,
        UpdateWordSetCommandHandler update,
        DeleteWordSetCommandHandler delete)
    {
        _currentUser = currentUser;
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WordSetDto>>> GetAll(CancellationToken ct)
    {
        var result = await _getAll.Handle(new GetWordSetsQuery(_currentUser.UserId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WordSetDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getById.Handle(new GetWordSetByIdQuery(id, _currentUser.UserId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WordSetDto>> Create(WordSetRequestDto request, CancellationToken ct)
    {
        var result = await _create.Handle(new CreateWordSetCommand(_currentUser.UserId, request.Title, request.Description), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WordSetDto>> Update(Guid id, WordSetRequestDto request, CancellationToken ct)
    {
        var result = await _update.Handle(new UpdateWordSetCommand(id, _currentUser.UserId, request.Title, request.Description), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _delete.Handle(new DeleteWordSetCommand(id, _currentUser.UserId), ct);
        return NoContent();
    }
}

public record WordSetRequestDto(string Title, string? Description);
