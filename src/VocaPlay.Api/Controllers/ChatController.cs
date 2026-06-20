// VocaPlay.Api/Controllers/ChatController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Application.Chat.DTOs;
using VocaPlay.Application.Chat.Queries;
using VocaPlay.Application.Common.Interfaces;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly SendChatMessageCommandHandler _send;
    private readonly GetChatHistoryQueryHandler _getHistory;
    private readonly ClearChatHistoryCommandHandler _clearHistory;

    public ChatController(
        ICurrentUserService currentUser,
        SendChatMessageCommandHandler send,
        GetChatHistoryQueryHandler getHistory,
        ClearChatHistoryCommandHandler clearHistory)
    {
        _currentUser = currentUser;
        _send = send;
        _getHistory = getHistory;
        _clearHistory = clearHistory;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Send(ChatRequestDto request, CancellationToken ct)
    {
        var result = await _send.Handle(new SendChatMessageCommand(_currentUser.UserId, request.Message), ct);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetHistory(CancellationToken ct)
    {
        var result = await _getHistory.Handle(new GetChatHistoryQuery(_currentUser.UserId), ct);
        return Ok(result);
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory(CancellationToken ct)
    {
        await _clearHistory.Handle(new ClearChatHistoryCommand(_currentUser.UserId), ct);
        return NoContent();
    }
}
