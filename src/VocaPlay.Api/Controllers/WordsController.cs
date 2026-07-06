// VocaPlay.Api/Controllers/WordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Application.Words.DTOs;
using VocaPlay.Application.Words.Queries;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/words")]
public class WordsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetWordsQueryHandler _getAll;
    private readonly AddWordCommandHandler _add;
    private readonly UpdateWordCommandHandler _update;
    private readonly DeleteWordCommandHandler _delete;
    private readonly BulkAddWordsCommandHandler _bulkAdd;

    public WordsController(
        ICurrentUserService currentUser,
        GetWordsQueryHandler getAll,
        AddWordCommandHandler add,
        UpdateWordCommandHandler update,
        DeleteWordCommandHandler delete,
        BulkAddWordsCommandHandler bulkAdd)
    {
        _currentUser = currentUser;
        _getAll = getAll;
        _add = add;
        _update = update;
        _delete = delete;
        _bulkAdd = bulkAdd;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WordDto>>> GetAll(CancellationToken ct)
    {
        var result = await _getAll.Handle(new GetWordsQuery(_currentUser.UserId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WordDto>> Add(WordRequestDto request, CancellationToken ct)
    {
        var result = await _add.Handle(new AddWordCommand(
            _currentUser.UserId,
            request.English, request.Vietnamese, request.Pronunciation,
            request.Level, request.Type, request.ExampleSentence, request.EnglishDefinition), ct);
        return Ok(result);
    }

    [HttpPut("{wordId:guid}")]
    public async Task<ActionResult<WordDto>> Update(Guid wordId, WordRequestDto request, CancellationToken ct)
    {
        var result = await _update.Handle(new UpdateWordCommand(
            wordId, _currentUser.UserId,
            request.English, request.Vietnamese, request.Pronunciation,
            request.Level, request.Type, request.ExampleSentence, request.EnglishDefinition), ct);
        return Ok(result);
    }

    [HttpDelete("{wordId:guid}")]
    public async Task<IActionResult> Delete(Guid wordId, CancellationToken ct)
    {
        await _delete.Handle(new DeleteWordCommand(wordId, _currentUser.UserId), ct);
        return NoContent();
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkAddResultDto>> BulkAdd(BulkAddWordsRequestDto request, CancellationToken ct)
    {
        var result = await _bulkAdd.Handle(new BulkAddWordsCommand(_currentUser.UserId, request.Words), ct);
        return Ok(result);
    }
}

public record WordRequestDto(
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence,
    string? EnglishDefinition);

public record BulkAddWordsRequestDto(IReadOnlyList<WordInput> Words);
