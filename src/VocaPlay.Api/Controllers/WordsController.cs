// VocaPlay.Api/Controllers/WordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Application.Words.DTOs;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wordsets/{setId:guid}/words")]
public class WordsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly AddWordCommandHandler _add;
    private readonly UpdateWordCommandHandler _update;
    private readonly DeleteWordCommandHandler _delete;
    private readonly BulkAddWordsCommandHandler _bulkAdd;

    public WordsController(
        ICurrentUserService currentUser,
        AddWordCommandHandler add,
        UpdateWordCommandHandler update,
        DeleteWordCommandHandler delete,
        BulkAddWordsCommandHandler bulkAdd)
    {
        _currentUser = currentUser;
        _add = add;
        _update = update;
        _delete = delete;
        _bulkAdd = bulkAdd;
    }

    [HttpPost]
    public async Task<ActionResult<WordDto>> Add(Guid setId, WordRequestDto request, CancellationToken ct)
    {
        var result = await _add.Handle(new AddWordCommand(
            setId, _currentUser.UserId,
            request.English, request.Vietnamese, request.Pronunciation,
            request.Level, request.Type, request.ExampleSentence), ct);
        return Ok(result);
    }

    [HttpPut("{wordId:guid}")]
    public async Task<ActionResult<WordDto>> Update(Guid setId, Guid wordId, WordRequestDto request, CancellationToken ct)
    {
        var result = await _update.Handle(new UpdateWordCommand(
            wordId, setId, _currentUser.UserId,
            request.English, request.Vietnamese, request.Pronunciation,
            request.Level, request.Type, request.ExampleSentence), ct);
        return Ok(result);
    }

    [HttpDelete("{wordId:guid}")]
    public async Task<IActionResult> Delete(Guid setId, Guid wordId, CancellationToken ct)
    {
        await _delete.Handle(new DeleteWordCommand(wordId, setId, _currentUser.UserId), ct);
        return NoContent();
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkAddResultDto>> BulkAdd(Guid setId, BulkAddWordsRequestDto request, CancellationToken ct)
    {
        var result = await _bulkAdd.Handle(new BulkAddWordsCommand(setId, _currentUser.UserId, request.Words), ct);
        return Ok(result);
    }
}

public record WordRequestDto(
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence);

public record BulkAddWordsRequestDto(IReadOnlyList<WordInput> Words);
