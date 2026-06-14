// VocaPlay.Api/Controllers/GameController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Game.Commands;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Application.Game.Queries;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class GameController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetGamePairsQueryHandler _getPairs;
    private readonly SaveGameSessionCommandHandler _saveSession;
    private readonly GetGameSessionsQueryHandler _getSessions;

    public GameController(
        ICurrentUserService currentUser,
        GetGamePairsQueryHandler getPairs,
        SaveGameSessionCommandHandler saveSession,
        GetGameSessionsQueryHandler getSessions)
    {
        _currentUser = currentUser;
        _getPairs = getPairs;
        _saveSession = saveSession;
        _getSessions = getSessions;
    }

    [HttpGet("wordsets/{setId:guid}/game")]
    public async Task<ActionResult<GamePairsDto>> GetPairs(Guid setId, CancellationToken ct)
    {
        var result = await _getPairs.Handle(new GetGamePairsQuery(setId, _currentUser.UserId), ct);
        return Ok(result);
    }

    [HttpPost("game/sessions")]
    public async Task<ActionResult<GameSessionDto>> SaveSession(SaveGameSessionRequestDto request, CancellationToken ct)
    {
        var result = await _saveSession.Handle(new SaveGameSessionCommand(_currentUser.UserId, request.WordSetId, request.Score, request.TotalPairs), ct);
        return Ok(result);
    }

    [HttpGet("game/sessions")]
    public async Task<ActionResult<IReadOnlyList<GameSessionDto>>> GetSessions(CancellationToken ct)
    {
        var result = await _getSessions.Handle(new GetGameSessionsQuery(_currentUser.UserId), ct);
        return Ok(result);
    }
}

public record SaveGameSessionRequestDto(Guid WordSetId, int Score, int TotalPairs);
